using MongoDB.Bson;

namespace Pulsar.BuildingBlocks.DataFactory
{
    public class GeneratorBase
    {
        private Random _rng;

        public GeneratorBase(Random rng)
        {
            this._rng = rng;
        }

        public IEnumerable<T> All<T>() where T : struct, Enum => Enum.GetValues<T>();
        public bool NextBool() => _rng.NextBoolean();
        public byte NextByte() => NextByte(null, null);
        public byte NextByte(byte? min, byte? max)
        {
            min ??= byte.MinValue;
            max ??= byte.MaxValue;
            return _rng.NextByte(min.Value, max.Value);
        }
        public char NextChar() => NextChar(null, null);
        public char NextChar(char? min, char? max)
        {
            min ??= char.MinValue;
            max ??= char.MaxValue;
            return _rng.NextChar(min.Value, max.Value);
        }
        public DateOnly NextDateOnly() => NextDateOnly(null, null);
        public DateOnly NextDateOnly(DateOnly? min, DateOnly? max)
        {
            min ??= DateOnly.MinValue;
            max ??= DateOnly.MaxValue;
            return _rng.NextDateOnly(min.Value, max.Value);
        }
        public DateTime NextDateTime() => NextDateTime(null, null);
        public DateTime NextDateTime(DateTime? min, DateTime? max)

        {
            min ??= DateTime.MinValue;
            max ??= DateTime.MaxValue;
            return _rng.NextDateTime(min.Value, max.Value);
        }
        public decimal NextDecimal() => NextDecimal(null, null);
        public decimal NextDecimal(decimal? min, decimal? max)
        {
            min ??= decimal.MinValue;
            max ??= decimal.MaxValue;
            return _rng.NextDecimal(min.Value, max.Value);
        }
        public double NextDouble() => NextDouble(null, null);
        public double NextDouble(double? min, double? max)
        {
            min ??= double.MinValue;
            max ??= double.MaxValue;
            return _rng.NextDouble(min.Value, max.Value);
        }


        public T NextEnum<T>() where T : struct, Enum => _rng.NextEnum<T>();
        public IEnumerable<T> NextEnumerable<T>(Func<int, T> generator) => NextEnumerable(generator, 0, 100);
        public IEnumerable<T> NextEnumerable<T>(Func<int, T> generator, int? minlen, int? maxlen)
        {
            var len = NextInt(minlen, maxlen);
            for (int i = 0; i < len; i++)
            {
                yield return generator(i);
            }
        }
        public IEnumerable<T> NextEnumerable<T>(Func<int, T> generator, int len)
        {
            for (int i = 0; i < len; i++)
            {
                yield return generator(i);
            }
        }
        public float NextFloat() => NextFloat(null, null);

        public float NextFloat(float? min, float? max)
        {
            min ??= float.MinValue;
            max ??= float.MaxValue;
            return _rng.NextFloat(min.Value, max.Value);
        }
        public Guid NextGuid()
        {
            return _rng.NextGuid();
        }

        public int NextInt() => NextInt(null, null);

        public int NextInt(int? min, int? max)
        {
            min ??= int.MinValue;
            max ??= int.MaxValue;
            return _rng.Next(min.Value, max.Value);
        }
        public long NextLong() => NextLong(null, null);
        public long NextLong(long? min, long? max)
        {
            min ??= long.MinValue;
            max ??= long.MaxValue;
            return _rng.NextInt64(min.Value, max.Value);
        }
        public ObjectId NextObjectId()
        {
            return _rng.NextObjectId();
        }
        public short NextShort() => NextShort(null, null);
        public short NextShort(short? min, short? max)
        {
            min ??= short.MinValue;
            max ??= short.MaxValue;
            return _rng.NextShort(min.Value, max.Value);
        }
        public string NextString() => _rng.NextString(0, 100);
        public string NextString(int? minlen, int? maxlen) => _rng.NextString(minlen ?? 0, maxlen ?? checked((minlen ?? 0) + 100));

        // email{}@gmail.com
        public string NextString(string template) => _rng.NextString(template);
        public TimeOnly NextTimeOnly() => NextTimeOnly(null, null);
        public TimeOnly NextTimeOnly(TimeOnly? min, TimeOnly? max)
        {
            min ??= TimeOnly.MinValue;
            max ??= TimeOnly.MaxValue;
            return _rng.NextTimeOnly(min.Value, max.Value);
        }
        public TimeSpan NextTimeSpan() => NextTimeSpan(null, null);
        public TimeSpan NextTimeSpan(TimeSpan? min, TimeSpan? max)
        {
            min ??= TimeSpan.MinValue;
            max ??= TimeSpan.MaxValue;
            return _rng.NextTimeSpan(min.Value, max.Value);
        }
        public T OneOf<T>(params T[] values) => values[_rng.Next(0, values.Length)];
        public T? OrDefault<T>(Func<T> value) => _rng.NextDouble() <= Constants.NULL_PROBABILITY ? default(T) : value();
        public T? OrDefault<T>(T value) => _rng.NextDouble() <= Constants.NULL_PROBABILITY ? default(T) : value;
        public T? OrNull<T>(Func<T> value) where T : struct => _rng.NextDouble() <= Constants.NULL_PROBABILITY ? null : value();
        public T? OrNull<T>(T value) where T : struct => _rng.NextDouble() <= Constants.NULL_PROBABILITY ? null : value;
    }
}