namespace Par.Interviews.ConnectionStringProvider
{
    public static class OptionsConstants
    {
        public const string ConnectionStringConfig = "EmployeeConnection";
        public const string EnvironmentVariableName = "ASPNETCORE_ENVIRONMENT";
        public const string DatabaseName = "DATABASE_NAME";
        public const string Development = "Development";
        public const string ConnectionStringTtlConfig = "ConnectionStringTtl";
        public const string ProxyOptionsConfig = "RDS";
        public const int DefaultConnectionStringTtl = 600; // 10 minutes
    }
}