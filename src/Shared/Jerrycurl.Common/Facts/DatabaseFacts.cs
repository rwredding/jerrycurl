namespace Jerrycurl.Facts
{
    internal static class DatabaseFacts
    {
        public const string SqlServerMoniker = "sqlserver";
        public const string PostgresMoniker = "postgres";
        public const string SqliteMoniker = "sqlite";
        public const string OracleMoniker = "oracle";
        public const string MySqlMoniker = "mysql";

        public static string GetNuGetPackage(string moniker)
        {
            switch (moniker)
            {
                case SqlServerMoniker:
                    return "Jerrycurl.Vendors.SqlServer";
                case PostgresMoniker:
                    return "Jerrycurl.Vendors.Postgres";
                case SqliteMoniker:
                    return "Jerrycurl.Vendors.Sqlite";
                case OracleMoniker:
                    return "Jerrycurl.Vendors.Oracle";
                case MySqlMoniker:
                    return "Jerrycurl.Vendors.MySql";
                default:
                    return null;
            }
        }

        public static string GetToolsNuGetPackage(string moniker)
        {
            switch (moniker)
            {
                case SqlServerMoniker:
                    return "Jerrycurl.Tools.Vendors.SqlServer";
                case PostgresMoniker:
                    return "Jerrycurl.Tools.Vendors.Postgres";
                case SqliteMoniker:
                    return "Jerrycurl.Tools.Vendors.Sqlite";
                case OracleMoniker:
                    return "Jerrycurl.Tools.Vendors.Oracle";
                case MySqlMoniker:
                    return "Jerrycurl.Tools.Vendors.MySql";
                default:
                    return null;
            }
        }
    }
}
