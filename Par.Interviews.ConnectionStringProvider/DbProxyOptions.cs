namespace Par.Interviews.ConnectionStringProvider
{
    public class DbProxyOptions
    {
        public string ReadOnlyEndpoint { get; set; }
        public string ReadWriteEndpoint { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
    }
}