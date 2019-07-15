namespace Docms.IntegrationTests
{
    static class Constants
    {
        internal static string TestUrlBase = "http://localhost:51693";

        internal static string UrlFor(string path)
        {
            return TestUrlBase + path;
        }
    }
}
