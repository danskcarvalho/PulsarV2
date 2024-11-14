using System.Globalization;

namespace Pulsar.Services.Shared;

public static class SharedConstants
{
    public static readonly CultureInfo DefaultCulture = new CultureInfo("pt-BR");
	public static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;
	public static readonly bool IgnoreCase = true;
}
