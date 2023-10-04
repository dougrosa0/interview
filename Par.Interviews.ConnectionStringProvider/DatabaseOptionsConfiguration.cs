using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Par.Interviews.ConnectionStringProvider
{
    public class DatabaseOptionsConfiguration : IConfigureOptions<DatabaseOptions>
    {
        private readonly IConfiguration _configuration;

        public DatabaseOptionsConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Configure(DatabaseOptions options)
        {
            options.ConnectionString = _configuration.GetConnectionString(OptionsConstants.ConnectionStringConfig);
            options.DatabaseName = _configuration.GetSection(OptionsConstants.DatabaseName)?.Value;
            options.IsDevelopmentEnvironment = (_configuration.GetValue<string>(OptionsConstants.EnvironmentVariableName) ?? OptionsConstants.Development) == OptionsConstants.Development;
            var ttlString = _configuration.GetSection(OptionsConstants.ConnectionStringTtlConfig)?.Value;
            options.ConnectionStringTtlSeconds = int.TryParse(ttlString, out var ttl)
                ? ttl
                : OptionsConstants.DefaultConnectionStringTtl;

            _configuration.GetSection("RDS").Bind(options.ProxyOptions);
        }
    }
}