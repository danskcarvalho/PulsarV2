namespace Pulsar.BuildingBlocks.UnitTests.Mocking.MongoDB;

class Collection<T> : Collection where T : class
{
    private readonly List<BsonDocument> _documents = new List<BsonDocument>();
    private readonly HashSet<ObjectId> _documentIds = new HashSet<ObjectId>();
    private readonly List<KeyExtractor> _keyExtractors = new List<KeyExtractor>();

    public Collection()
    {
    }

    private Collection(List<BsonDocument> documents, HashSet<ObjectId> documentIds, List<KeyExtractor> keyExtractors)
    {
        _documents = documents;
        _documentIds = documentIds;
        _keyExtractors = keyExtractors;
    }

    public void AddUniqueKey<TKey>(Func<T, TKey> keyExtractor) where TKey : notnull
    {
        _keyExtractors.Add(new KeyExtractor<TKey>(keyExtractor));
    }

    public void InsertMany(IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            var bson = ToBsonDocument(item);
            if (!bson.Contains("_id"))
                throw new InvalidOperationException("no _id");
            var id = bson["_id"].AsObjectId;
            if (_documentIds.Contains(id))
                throw new InvalidOperationException("document already inserted into the collection");

            StoreKeys(item);
            _documentIds.Add(id);
            _documents.Add(bson);

        }
    }

    public T? FindById(ObjectId id)
    {
        if (!_documentIds.Contains(id))
            return null;

        foreach (var item in _documents)
        {
            var did = item["_id"].AsObjectId;
            if (did == id)
                return FromBsonDocument<T>(item);
        }

        return null;
    }

    public IEnumerable<T> Find(IFindSpecification<T> specification)
    {
        // -- this implementation runs the risk of not being correct if the any of the lambas called mutates the object
        var spec = specification.GetSpec();
        var pred = spec.Predicate.Compile();
        var r = _documents.Select((d, i) => (Index: i, Document: FromBsonDocument<T>(d))).Where(t => pred(t.Document));
        if (spec.OrderBy.Count != 0)
        {
            var comparer = new ObjectComparer();
            IOrderedEnumerable<(int Index, T Document)>? ordered = null;
            foreach (var ob in spec.OrderBy)
            {
                if (ob is Ascending<T> asc)
                {
                    var expr = asc.Expression.Compile();
                    if (ordered == null)
                        ordered = r.OrderBy(x => expr(x.Document), comparer);
                    else
                        ordered = ordered.ThenBy(x => expr(x.Document), comparer);
                }
                else
                {
                    var desc = (Descending<T>)ob;
                    var expr = desc.Expression.Compile();
                    if (ordered == null)
                        ordered = r.OrderByDescending(x => expr(x.Document), comparer);
                    else
                        ordered = ordered.ThenByDescending(x => expr(x.Document), comparer);
                }
            }
            r = ordered!;
        }

        if (spec.Skip != null)
            r = r.Skip(spec.Skip.Value);
        if (spec.Limit != null)
            r = r.Take(spec.Limit.Value);

        return r.Select(x => _documents[x.Index]).Select(x => FromBsonDocument<T>(x)).ToList();
    }

    public IEnumerable<TProjection> Find<TProjection>(IFindSpecification<T, TProjection> specification)
    {
        // -- this implementation runs the risk of not being correct if the any of the lambas called mutates the object
        var spec = specification.GetSpec();
        var pred = spec.Predicate.Compile();
        var proj = spec.Projection.Compile();
        var r = _documents.Select((d, i) => (Index: i, Document: FromBsonDocument<T>(d))).Where(t => pred(t.Document));
        if (spec.OrderBy.Count != 0)
        {
            var comparer = new ObjectComparer();
            IOrderedEnumerable<(int Index, T Document)>? ordered = null;
            foreach (var ob in spec.OrderBy)
            {
                if (ob is Ascending<T> asc)
                {
                    var expr = asc.Expression.Compile();
                    if (ordered == null)
                        ordered = r.OrderBy(x => expr(x.Document), comparer);
                    else
                        ordered = ordered.ThenBy(x => expr(x.Document), comparer);
                }
                else
                {
                    var desc = (Descending<T>)ob;
                    var expr = desc.Expression.Compile();
                    if (ordered == null)
                        ordered = r.OrderByDescending(x => expr(x.Document), comparer);
                    else
                        ordered = ordered.ThenByDescending(x => expr(x.Document), comparer);
                }
            }
            r = ordered!;
        }

        if (spec.Skip != null)
            r = r.Skip(spec.Skip.Value);
        if (spec.Limit != null)
            r = r.Take(spec.Limit.Value);

        return r.Select(x => _documents[x.Index]).Select(x => proj(FromBsonDocument<T>(x))).ToList();
    }

    public int DeleteMany(IDeleteSpecification<T> specification, int? limit = null)
    {
        var spec = specification.GetSpec();
        var pred = spec.Predicate.Compile();
        HashSet<ObjectId> toBeRemoved = new HashSet<ObjectId>();
        _documents.RemoveAll(d =>
        {
            if (toBeRemoved.Count >= limit)
                return false;

            var t = FromBsonDocument<T>(d);
            var id = d["_id"].AsObjectId;
            if (pred(t))
            {
                DeleteKeys(t);
                toBeRemoved.Add(id);
                return true;
            }
            else
                return false;
        });

        _documentIds.ExceptWith(toBeRemoved);
        return toBeRemoved.Count;
    }

    public int UpdateMany(IUpdateSpecification<T> specification, int? limit = null)
    {
        // -- this implementation runs the risk of not being correct if the any of the lambas called mutates
        var spec = specification.GetSpec();
        var pred = spec.Predicate.Compile();
        var docs = _documents.Where(d => pred(FromBsonDocument<T>(d))).ToList();
        if (limit != null)
            docs = docs.Take(limit.Value).ToList();

        foreach (var doc in docs)
        {
            var originalDoc = (BsonDocument)doc.DeepClone();
            var originalModel = FromBsonDocument<T>(originalDoc);

            DeleteKeys(originalModel);
            try
            {
                var injector = new UpdateInjector<T>()
                {
                    Target = doc
                };
                foreach (var cmd in spec.Commands)
                {
                    cmd.InjectField(injector);
                }
            }
            catch
            {
                CopyTo(doc, originalDoc);
                StoreKeys(originalModel);
                throw;
            }

            try
            {
                StoreKeys(FromBsonDocument<T>(doc));
            }
            catch
            {
                CopyTo(doc, originalDoc);
                StoreKeys(originalModel);
                throw;
            }
        }

        return docs.Count;
    }

    private void CopyTo(BsonDocument destination, BsonDocument source)
    {
        destination.Clear();
        foreach (var elem in source)
        {
            destination[elem.Name] = elem.Value.DeepClone();
        }
    }

    public bool Exists(ObjectId id)
    {
        return _documentIds.Contains(id);
    }

    public int Replace(T item, ObjectId id, long? version = null, string? stringPropertyName = null)
    {
        if (!_documentIds.Contains(id))
            return 0;

        for (int i = 0; i < _documents.Count; i++)
        {
            var did = _documents[i]["_id"].AsObjectId;
            if (did == id && VersionIsSame(_documents[i], version, stringPropertyName))
            {
                var originalDoc = _documents[i];
                var originalModel = FromBsonDocument<T>(originalDoc);

                DeleteKeys(originalModel);
                _documents[i] = ToBsonDocument(item);
                try
                {
                    StoreKeys(FromBsonDocument<T>(_documents[i]));
                }
                catch
                {
                    _documents[i] = originalDoc;
                    StoreKeys(originalModel);
                    throw;
                }
                return 1;
            }
        }

        return 0;
    }

    public override Collection Clone()
    {
        return new Collection<T>(
            _documents.Select(d => (BsonDocument)d.DeepClone()).ToList(),
            new HashSet<ObjectId>(_documentIds),
            _keyExtractors.Select(ks => ks.Clone()).ToList());
    }


    private void StoreKeys(T model)
    {
        foreach (var ks in _keyExtractors)
        {
            ks.StoreKey(model);
        }
    }

    private void DeleteKeys(T model)
    {

        foreach (var ks in _keyExtractors)
        {
            ks.DeleteKey(model);
        }
    }

    private bool VersionIsSame(BsonDocument bsonDocument, long? version, string? stringPropertyName)
    {
        if (version == null)
            return true;
        if (bsonDocument.Contains(stringPropertyName!))
        {
            var docVersion = bsonDocument[stringPropertyName!].IsInt32 ? (long)bsonDocument[stringPropertyName!].AsInt32 : (long)bsonDocument[stringPropertyName!].AsInt64;
            return docVersion == version;
        }
        else
            return false;
    }

    private Y FromBsonDocument<Y>(BsonDocument d)
    {
        return BsonSerializer.Deserialize<Y>(d);
    }

    private BsonDocument ToBsonDocument<Y>(Y item)
    {
        return item.ToBsonDocument();
    }

    class ObjectComparer : IComparer<object>
    {
        public int Compare(object? x, object? y)
        {
            if (x == null && y == null)
                return 0;

            if (x == null)
                return -1;

            if (y == null)
                return 1;

            if (x.GetType().IsAssignableFrom(y.GetType()) && x is IComparable c1)
                return c1.CompareTo(y);
            if (y.GetType().IsAssignableFrom(x.GetType()) && y is IComparable c2)
                return -c2.CompareTo(x);

            return x.GetType().FullName!.CompareTo(y.GetType().FullName);
        }
    }

    class ValueContainer<Y>
    {
        public required Y Value { get; set; }
    }

    abstract class KeyExtractor
    {
        public abstract void StoreKey(T model);
        public abstract void DeleteKey(T model);
        public abstract KeyExtractor Clone();
    }

    class KeyExtractor<TKey> : KeyExtractor where TKey : notnull
    {
        Func<T, TKey> _extractor;
        HashSet<TKey> _keys;

        public KeyExtractor(Func<T, TKey> extractor)
        {
            _extractor = extractor;
            _keys = new HashSet<TKey>();
        }
        private KeyExtractor(Func<T, TKey> extractor, HashSet<TKey> keys)
        {
            this._keys = keys; 
            this._extractor = extractor;
        }

        public override void DeleteKey(T model)
        {
            var key = _extractor(model);
            if(_keys.Contains(key))
                _keys.Remove(key);
        }

        public override void StoreKey(T model)
        {
            var key = _extractor(model);
            if (_keys.Contains(key))
                throw new DuplicatedKeyException($"duplicated key {key}");
            _keys.Add(key);
        }

        public override KeyExtractor Clone()
        {
            return new KeyExtractor<TKey>(_extractor, new HashSet<TKey>(_keys));
        }
    }
}

abstract class Collection
{
    public abstract Collection Clone();
}