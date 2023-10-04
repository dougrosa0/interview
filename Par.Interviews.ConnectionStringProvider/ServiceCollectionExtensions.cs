using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Par.Interviews.ConnectionStringProvider
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddConnectionProvider(this IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddSingleton<IAwsIamTokenProvider, AwsIamTokenProvider>()
                .AddSingleton<IConnectionStringProvider, ConnectionStringProvider>()
                .AddSingleton<IConnectionStringFactory, ConnectionStringFactory>()
                .AddSingleton<IConfigureOptions<DatabaseOptions>, DatabaseOptionsConfiguration>();

            return serviceCollection;
        }
    }
}