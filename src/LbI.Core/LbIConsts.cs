using LbI.Debugging;

namespace LbI;

public class LbIConsts
{
    public const string LocalizationSourceName = "LbI";

    public const string ConnectionStringName = "Default";

    public const bool MultiTenancyEnabled = false;


    /// <summary>
    /// Default pass phrase for SimpleStringCipher decrypt/encrypt operations
    /// </summary>
    public static readonly string DefaultPassPhrase =
        DebugHelper.IsDebug ? "gsKxGZ012HLL3MI5" : "1b1ab2ea8bb44b758cc143feff7fa0e9";
}
