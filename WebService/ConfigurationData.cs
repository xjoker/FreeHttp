namespace FreeHttp.WebService
{
    public class ConfigurationData
    {
        private static readonly string url_dev = "http://localhost:5000/";

        public static string BaseUrl { get; } = "https://api.lulianqi.com/";

        public static string RuleVersion { get; } = "2.0";
    }
}