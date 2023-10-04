using Microsoft.Extensions.Options;
using System;

namespace Par.Interviews.ConnectionStringProvider
{
    public interface IConnectionStringProvider
    {
        string ReadOnlyConnectionString { get; }
        string WriteConnectionString { get; }
    }

    public class ConnectionStringProvider : IConnectionStringProvider
    {
        public string ReadOnlyConnectionString => _readOnlyConnectionString.Value;
        public string WriteConnectionString => _writeConnectionString.Value;

        private readonly Cache<string> _readOnlyConnectionString;
        private readonly Cache<string> _writeConnectionString;

        public ConnectionStringProvider(
            IOptions<DatabaseOptions> databaseOptions,
            IConnectionStringFactory factory)
        {
            if (databaseOptions == null || databaseOptions.Value == null)
            {
                throw new ArgumentNullException(nameof(databaseOptions));
            }
            _readOnlyConnectionString = new Cache<string>(() => factory.Create(false), databaseOptions.Value.ConnectionStringTtlSeconds);
            _writeConnectionString = new Cache<string>(() => factory.Create(true), databaseOptions.Value.ConnectionStringTtlSeconds);
        }
    }
}