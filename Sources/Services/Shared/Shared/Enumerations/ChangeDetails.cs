namespace Pulsar.Services.Shared.Enumerations;

[Flags]
public enum ChangeDetails
{
    None = 0,
    Basic = 1 << 1,
    NonBasic = 1 << 2
}

public static class ChangeDetailsExtensions
{
    public static bool HasBasic(this ChangeDetails cd) => (cd & ChangeDetails.Basic) == ChangeDetails.Basic;
    public static bool HasNonBasic(this ChangeDetails cd) => (cd & ChangeDetails.NonBasic) == ChangeDetails.NonBasic;
}
