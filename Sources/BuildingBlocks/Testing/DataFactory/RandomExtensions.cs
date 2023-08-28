using MongoDB.Bson;
using System.Text;

namespace Pulsar.BuildingBlocks.DataFactory;

static class RandomExtensions
{
    private static readonly string _Chars = "abcdefghijklmnopqrstuvxywzABCDEFGHIJKLMNOPQRSTUVXYWZ0123456789_ ";

    public static Guid NextGuid(this Random random)
    {
        var _bytes = new byte[16];
        random.NextBytes(_bytes);
        return new Guid(_bytes);
    }
    public static ObjectId NextObjectId(this Random random)
    {
        var _bytes = new byte[12];
        random.NextBytes(_bytes);
        return new ObjectId(_bytes);
    }
    public static byte NextByte(this Random random, byte min, byte max)
    {
        if (max <= min)
            throw new ArgumentOutOfRangeException("max", "max must be > min!");

        return (byte)random.Next((int)min, (int)max);
    }
    public static short NextShort(this Random random, short min, short max)
    {
        if (max <= min)
            throw new ArgumentOutOfRangeException("max", "max must be > min!");

        return (short)random.Next((int)min, (int)max);
    }
    public static long NextLong(this Random random, long min, long max)
    {
        if (max <= min)
            throw new ArgumentOutOfRangeException("max", "max must be > min!");

        //Working with ulong so that modulo works correctly with values > long.MaxValue
        ulong uRange = (ulong)(max - min);

        //Prevent a modolo bias; see https://stackoverflow.com/a/10984975/238419
        //for more information.
        //In the worst case, the expected number of calls is 2 (though usually it's
        //much closer to 1) so this loop doesn't really hurt performance at all.
        ulong ulongRand;
        do
        {
            byte[] buf = new byte[8];
            random.NextBytes(buf);
            ulongRand = (ulong)BitConverter.ToInt64(buf, 0);
        } while (ulongRand > ulong.MaxValue - ((ulong.MaxValue % uRange) + 1) % uRange);

        return (long)(ulongRand % uRange) + min;
    }
    public static decimal NextDecimal(this Random random, decimal min, decimal max)
    {
        if (max <= min)
            throw new ArgumentOutOfRangeException("max", "max must be > min!");

        var d = (decimal)random.NextDouble();
        return min * (1.0m - d) + max * d;
    }
    public static double NextDouble(this Random random, double min, double max)
    {
        if (max <= min)
            throw new ArgumentOutOfRangeException("max", "max must be > min!");

        var d = (double)random.NextDouble();
        return min * (1.0 - d) + max * d;
    }
    public static float NextFloat(this Random random, float min, float max)
    {
        if (max <= min)
            throw new ArgumentOutOfRangeException("max", "max must be > min!");

        var d = (float)random.NextDouble();
        return min * (1.0f - d) + max * d;
    }
    public static DateTime NextDateTime(this Random random, DateTime min, DateTime max)
    {
        if (max <= min)
            throw new ArgumentOutOfRangeException("max", "max must be > min!");

        min = min.ToUniversalTime();
        max = max.ToUniversalTime();
        var ticks = random.NextLong(min.Ticks, max.Ticks);
        return new DateTime(ticks, DateTimeKind.Utc);
    }
    public static DateOnly NextDateOnly(this Random random, DateOnly min, DateOnly max)
    {
        if (max <= min)
            throw new ArgumentOutOfRangeException("max", "max must be > min!");

        var dayNumber = random.Next(min.DayNumber, max.DayNumber + 1);
        return DateOnly.FromDayNumber(dayNumber);
    }
    public static TimeOnly NextTimeOnly(this Random random, TimeOnly min, TimeOnly max)
    {
        if (max <= min)
            throw new ArgumentOutOfRangeException("max", "max must be > min!");

        var ticks = random.NextLong(min.Ticks, max.Ticks + 1);
        return new TimeOnly(ticks);
    }
    public static TimeSpan NextTimeSpan(this Random random, TimeSpan min, TimeSpan max)
    {
        if (max <= min)
            throw new ArgumentOutOfRangeException("max", "max must be > min!");

        var ticks = random.NextLong(min.Ticks, max.Ticks + 1);
        return new TimeSpan(ticks);
    }
    public static bool NextBoolean(this Random random)
    {
        return random.Next() % 2 == 0;
    }
    public static char NextChar(this Random random, char min, char max)
    {
        if (max <= min)
            throw new ArgumentOutOfRangeException("max", "max must be > min!");

        return (char)random.NextShort((short)min, (short)max);
    }
    public static string NextString(this Random random, int minlen, int maxlen)
    {
        if (maxlen <= minlen)
            throw new ArgumentOutOfRangeException("maxLength", "maxLength must be > minLength!");
        if (minlen < 0)
            throw new ArgumentOutOfRangeException("minLength", "minLength must be > 0!");

        StringBuilder b = new StringBuilder();
        var len = random.Next(minlen, maxlen + 1);
        for (int i = 0; i < len; i++)
        {
            b.Append(_Chars[random.Next(0, _Chars.Length)]);
        }
        return b.ToString();
    }
    public static T NextEnum<T>(this Random random) where T : struct, Enum
    {
        var values = Enum.GetValues<T>();
        return values[random.Next(0, values.Length)];
    }
    public static string NextString(this Random random, string template)
    {
        StringBuilder b = new StringBuilder(template);
        while (b.Contains("{}"))
            b.Replace("{}", random.Next(0, 100_000_000).ToString());
        return b.ToString();
    }
    private static bool Contains(this StringBuilder builder, string substring)
    {
        for (int i = 0; i < builder.Length; i++)
        {
            if (builder.Length - i >= substring.Length)
            {
                for (int j = 0; j < substring.Length; j++)
                {
                    var b = builder[i + j] == substring[j];
                    if (!b)
                        goto Next;
                }
                return true;
            }
        Next:;
        }
        return false;
    }
}
