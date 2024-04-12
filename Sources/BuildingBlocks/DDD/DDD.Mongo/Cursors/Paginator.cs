using System.Linq.Expressions;
using static Pulsar.BuildingBlocks.DDD.Mongo.Cursors.Paginator;

namespace Pulsar.BuildingBlocks.DDD.Mongo.Cursors;

public abstract class Paginator<TElement> where TElement : class
{
    public abstract PageCursor<TElement, object> ForLimit(int limit);
}

public class Paginator
{
    public static PaginatorBuilder Builder => new PaginatorBuilder();

    public class PaginatorBuilder
    {
        internal PaginatorBuilder() { }

        public SortedPaginatorBuilder1<TElement, TElement> For<TElement>() where TElement : class
        {
            return new SortedPaginatorBuilder1<TElement, TElement>();
        }

        public SortedPaginatorBuilder1<TElement, TProjection> For<TElement, TProjection>() where TElement : class
        {
            return new SortedPaginatorBuilder1<TElement, TProjection>();
        }
    }

    public class SortedPaginatorBuilder1<TElement, TProjection> where TElement : class
    {
        internal SortedPaginatorBuilder1() { }

        public SortedPaginatorBuilder2<TElement, TProjection> SortBy<TValue>(Expression<Func<TElement, TValue>> expression, Expression<Func<TProjection, TValue>>? projection = null)
        {
            if (projection == null && typeof(TElement) != typeof(TProjection))
                throw new InvalidOperationException("projection must be specified");

            List<string> propertyNames = new List<string>();
            TraversePropertyChain(expression, propertyNames);
            if (propertyNames.Count == 0)
                throw new InvalidOperationException("invalid expression");

            var result = new SortedPaginatorBuilder2<TElement, TProjection>
            {
                _sort1 = string.Join('.', propertyNames),
                _sort1Chain = propertyNames
            };

            if (projection != null)
            {
                List<string> propertyNames2 = new List<string>();
                TraversePropertyChain(projection, propertyNames2);
                result._proj1Chain = propertyNames2;

                if (propertyNames2.Count == 0)
                    throw new InvalidOperationException("invalid expression");
            }

            return result;
        }

        private void TraversePropertyChain<TU, TV>(Expression<Func<TU, TV>> expression, List<string> propertyNames)
        {
            if (expression.Body is MemberExpression memberExpr)
            {
                MemberExpression? me = memberExpr;
                // Traverse the expression tree to get property names
                while (me != null)
                {
                    propertyNames.Insert(0, me.Member.Name);
                    me = me.Expression as MemberExpression;
                }
            }
        }
    }

    public class SortedPaginatorBuilder2<TElement, TProjection> : Paginator<TElement> where TElement : class
    {
        internal string _sort1;
        private string? _sort2;
        internal List<string> _sort1Chain;
        private List<string>? _sort2Chain;
        internal List<string>? _proj1Chain;
        private List<string>? _proj2Chain;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal SortedPaginatorBuilder2() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public SortedPaginatorBuilder2<TElement, TProjection> AndBy<TValue>(Expression<Func<TElement, TValue>> expression, Expression<Func<TProjection, TValue>>? projection = null)
        {
            if (this._sort2 is not null)
                throw new InvalidOperationException("cannot call this method twice");

            List<string> propertyNames = new List<string>();
            TraversePropertyChain(expression, propertyNames);
            if (propertyNames.Count == 0)
                throw new InvalidOperationException("invalid expression");

            this._sort2 = string.Join('.', propertyNames);
            this._sort2Chain = propertyNames;

            if (projection != null)
            {
                List<string> propertyNames2 = new List<string>();
                TraversePropertyChain(projection, propertyNames2);
                _proj2Chain = propertyNames2;

                if (propertyNames2.Count == 0)
                    throw new InvalidOperationException("invalid expression");
            }

            return this;
        }

        public override PageCursor<TElement, object> ForLimit(int limit)
        {
            return new PageCursor<TElement, object> { _limit = limit, _sort1 = _sort1, _sort1Chain = _sort1Chain, _sort2 = _sort2, _sort2Chain = _sort2Chain, _proj1Chain = _proj1Chain, _proj2Chain = _proj2Chain };
        }

        private void TraversePropertyChain<TU, TV>(Expression<Func<TU, TV>> expression, List<string> propertyNames)
        {
            if (expression.Body is MemberExpression memberExpr)
            {
                MemberExpression? me = memberExpr;
                // Traverse the expression tree to get property names
                while (me != null)
                {
                    propertyNames.Insert(0, me.Member.Name);
                    me = me.Expression as MemberExpression;
                }
            }
        }
    }

    public class PageCursor<TElement, TFilter> where TElement : class
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal string _sort1;
        internal string? _sort2;
        internal List<string> _sort1Chain;
        internal List<string>? _sort2Chain;
        internal TFilter? _filter;
        internal int _limit;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private object? _lastSort1;
        private object? _lastSort2;
        internal List<string>? _proj1Chain;
        internal List<string>? _proj2Chain;

        public PageCursor<TElement, TFilter> ForToken(string? cursor)
        {
            if (cursor is not null)
            {
                var serialized = cursor.FromBase64Json<SerializedCursor<TFilter>>();
                if (serialized != null)
                {
                    this._filter = serialized.Filter;
                    this._lastSort1 = serialized.LastSort1;
                    this._lastSort2 = serialized.LastSort2;
                }
            }
            return this;
        }

        private string? ToToken()
        {
            if (this._lastSort1 == null && this._lastSort2 == null)
                return null;

            var serialized = new SerializedCursor<TFilter>()
            {
                Filter = _filter,
                LastSort1 = _lastSort1,
                LastSort2 = _lastSort2
            };
            return serialized.ToBase64Json();
        }

        public PageCursor<TElement, TAnotherFilter> ForFilter<TAnotherFilter>(TAnotherFilter? filter)
        {
            return new PageCursor<TElement, TAnotherFilter>
            {
                _filter = (TAnotherFilter?)(object?)this._filter ?? filter,
                _limit = _limit,
                _sort1 = _sort1,
                _sort1Chain = _sort1Chain,
                _sort2 = _sort2,
                _sort2Chain = _sort2Chain,
                _proj1Chain = _proj1Chain,
                _proj2Chain = _proj2Chain,
                _lastSort1 = this._lastSort1,
                _lastSort2 = this._lastSort2
            };
        }

        internal string? Next(TElement last)
        {
            var next = new PageCursor<TElement, TFilter>
            {
                _sort1 = _sort1,
                _sort2 = _sort2,
                _sort1Chain = _sort1Chain,
                _sort2Chain = _sort2Chain,
                _limit = this._limit,
                _filter = this._filter,
                _proj1Chain = _proj1Chain,
                _proj2Chain = _proj2Chain,
                _lastSort1 = GetNext(last, _sort1Chain),
                _lastSort2 = _sort2Chain != null ? GetNext(last, _sort2Chain) : null
            };
            return next.ToToken();
        }

        internal string? Next<TProjection>(TProjection last) where TProjection : class
        {
            var next = new PageCursor<TProjection, TFilter>
            {
                _sort1 = _sort1,
                _sort2 = _sort2,
                _sort1Chain = _sort1Chain,
                _sort2Chain = _sort2Chain,
                _limit = this._limit,
                _filter = this._filter,
                _proj1Chain = _proj1Chain,
                _proj2Chain = _proj2Chain,
                _lastSort1 = GetNext(last, _proj1Chain ?? _sort1Chain),
                _lastSort2 = _sort2Chain != null ? GetNext(last, _proj2Chain ?? _sort2Chain) : null
            };
            return next.ToToken();
        }

        private object? GetNext(object last, List<string> chain)
        {
            object? current = last;
            var i = 0;
            while (current != null && i < chain.Count)
            {
                var prop = current.GetType().GetProperty(chain[i]);
                if (prop == null)
                {
                    break;
                }
                current = prop.GetValue(current);
                i++;
            }

            return current;
        }

        internal string? Token => ToToken();

        internal bool TokenIsNotNull => this._lastSort1 != null || this._lastSort2 != null;

        internal (string Name, object? Value) SortField1 => (_sort1, _lastSort1);

        internal (string Name, object? Value)? SortField2 => _sort2 is null ? throw new InvalidOperationException("no second field to sort") : (_sort2, _lastSort2);

        internal bool HasSortField2 => _sort2 is not null;

        internal TFilter? Filter => _filter;

        internal int Limit => _limit;
    }

    private class SerializedCursor<TFilter>
    {
        public TFilter? Filter { get; set; } 
        public object? LastSort1 { get; set; }
        public object? LastSort2 { get; set; }
    }
}
