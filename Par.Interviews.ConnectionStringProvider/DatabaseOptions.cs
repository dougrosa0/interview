namespace Par.Interviews.ConnectionStringProvider
{
    public class DatabaseOptions
    {
        public string ConnectionString { get; set; }
        public int ConnectionStringTtlSeconds { get; set; }
        public string DatabaseName { get; set; }
        public bool IsDevelopmentEnvironment { get; set; }
        public DbProxyOptions ProxyOptions { get; set; } = new DbProxyOptions();
    }
}