using Amazon.Extensions.NETCore.Setup;
using BrinkAboveStore.OpenTrace;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using System;

namespace Par.Interviews.ConnectionStringProvider
{
    public interface IConnectionStringFactory
    {
        string Create(bool allowWrites);
    }

    public class ConnectionStringFactory : IConnectionStringFactory
    {
        private readonly ILogger<ConnectionStringFactory> _logger;
        private readonly DatabaseOptions _databaseOptions;
        private readonly IAwsIamTokenProvider _tokenProvider;

        public ConnectionStringFactory(
            ILogger<ConnectionStringFactory> logger,
            IOptions<DatabaseOptions> databaseOptions,
            IAwsIamTokenProvider tokenProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _databaseOptions = databaseOptions.Value ?? throw new ArgumentNullException(nameof(databaseOptions));
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
        }

        public string Create(bool allowWrites)
        {
            using (OpenTracingContextFactory.Current.Tracer.BuildSpan("ConnectionStringProvider.GetConnectionString").StartActive())
            {
                // use connection string builder starting with what is local
                var builder = new NpgsqlConnectionStringBuilder(_databaseOptions.ConnectionString);

                if (!string.IsNullOrWhiteSpace(_databaseOptions.DatabaseName))
                {
                    builder.Database = _databaseOptions.DatabaseName;
                }

                if (!_databaseOptions.IsDevelopmentEnvironment)
                {
                    if (string.IsNullOrEmpty(builder.Database))
                    {
                        throw new ConfigurationException($"{OptionsConstants.DatabaseName} is expected check configuration");
                    }

                    string password = null;
                    try
                    {
                        _logger.LogDebug("generating auth token...");
                        password = _tokenProvider.GetIamToken(_databaseOptions.ProxyOptions, allowWrites);
                        _logger.LogDebug("token acquired...");
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "generating auth token failed, {message}", e.Message);
                    }

                    builder.Host = allowWrites ? _databaseOptions.ProxyOptions.ReadWriteEndpoint : _databaseOptions.ProxyOptions.ReadOnlyEndpoint;
                    builder.Username = _databaseOptions.ProxyOptions.Username;
                    builder.Password = password;
                    builder.Port = _databaseOptions.ProxyOptions.Port;
                    builder.SslMode = SslMode.Require;
                    builder.TrustServerCertificate = true;
                }

                _logger.LogInformation(
                    "Building connection string: host={Host}, port={Port}, database={Database}, user={User}, pooling={Pooling}, command timeout: {CommandTimeout}",
                    builder.Host, builder.Port, builder.Database, builder.Username, builder.Pooling, builder.CommandTimeout);

                return builder.ConnectionString;
            }
        }
    }
}