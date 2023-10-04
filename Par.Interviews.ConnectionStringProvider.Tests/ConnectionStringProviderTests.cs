using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Threading;
using Xunit;

namespace Par.Interviews.ConnectionStringProvider.Tests
{
    public class ConnectionStringProviderTests
    {
        private Mock<IConfiguration> _config;
        private Mock<ILogger<ConnectionStringFactory>> _logger;
        private readonly Mock<IConfigurationSection> _connectionStringSectionMock;
        private readonly Mock<IConfigurationSection> _connectionStringTtlSectionMock;
        private readonly Mock<IConfigurationSection> _environmentSectionMock;
        private readonly Mock<IConfigurationSection> _databaseNameSectionMock;
        private readonly Mock<IAwsIamTokenProvider> _tokenProviderMock;
        private Mock<IOptions<DatabaseOptions>> _databaseOptions;

        public ConnectionStringProviderTests()
        {
            _config = new Mock<IConfiguration>();
            _logger = new Mock<ILogger<ConnectionStringFactory>>();
            _connectionStringSectionMock = new Mock<IConfigurationSection>();
            _connectionStringTtlSectionMock = new Mock<IConfigurationSection>();
            _environmentSectionMock = new Mock<IConfigurationSection>();
            _databaseNameSectionMock = new Mock<IConfigurationSection>();
            _tokenProviderMock = new Mock<IAwsIamTokenProvider>();
            _databaseOptions = new Mock<IOptions<DatabaseOptions>>();

            _config.Setup(x => x.GetSection("ConnectionStrings"))
                .Returns(_connectionStringSectionMock.Object);
        }

        [Fact]
        public void ShouldCreateReadConnectionString()
        {
            // Arrange
            var configConnectionString =
                "Host=localhost-READ;Port=5159;Database=royaltiesdb;Username=postgres;Password=postgres";
            var databaseOverride = "database2";
            var token = "token";

            MockServices(configConnectionString, databaseOverride, token, false, string.Empty);

            var proxyOptions = _databaseOptions.Object.Value.ProxyOptions;

            var factory = new ConnectionStringFactory(
                _logger.Object,
                _databaseOptions.Object,
                _tokenProviderMock.Object);

            var classUnderTest = new ConnectionStringProvider(
                _databaseOptions.Object,
                factory);

            // Act
            var result = classUnderTest.ReadOnlyConnectionString;

            // Assert
            Assert.Equal(
                $"Host={proxyOptions.ReadOnlyEndpoint};Port={proxyOptions.Port};Database={databaseOverride};Username={proxyOptions.Username};Password={token};SSL Mode=Require;Trust Server Certificate=True",
                result);
        }

        [Fact]
        public void ShouldCreateWriteConnectionString()
        {
            // Arrange
            var configConnectionString = "Host=localhost-WRITE;Port=5159;Database=royaltiesdb;Username=postgres;Password=postgres";
            var databaseOverride = "database2";
            var token = "token";

            MockServices(configConnectionString, databaseOverride, token, true, string.Empty);

            var proxyOptions = _databaseOptions.Object.Value.ProxyOptions;

            var factory = new ConnectionStringFactory(
                _logger.Object,
                _databaseOptions.Object,
                _tokenProviderMock.Object);

            var classUnderTest = new ConnectionStringProvider(
                _databaseOptions.Object,
                factory);

            // Act
            var result = classUnderTest.WriteConnectionString;

            // Assert
            Assert.Equal(
                $"Host={proxyOptions.ReadWriteEndpoint};Port={proxyOptions.Port};Database={databaseOverride};Username={proxyOptions.Username};Password={token};SSL Mode=Require;Trust Server Certificate=True",
                result);
        }

        private void MockServices(
            string configConnectionString,
            string databaseOverride,
            string token,
            bool allowWrites,
            string ttl)
        {
            _connectionStringSectionMock
                .Setup(x => x[OptionsConstants.ConnectionStringConfig])
                .Returns(configConnectionString);
            _config
                .Setup(x => x.GetSection(OptionsConstants.ConnectionStringTtlConfig))
                .Returns(_connectionStringTtlSectionMock.Object);
            _connectionStringTtlSectionMock
                .Setup(x => x.Value)
                .Returns(ttl);
            _config
                .Setup(x => x.GetSection(OptionsConstants.EnvironmentVariableName))
                .Returns(_environmentSectionMock.Object);
            _environmentSectionMock
                .Setup(x => x.Value)
                .Returns("dev");
            _config
                .Setup(x => x.GetSection(OptionsConstants.DatabaseName))
                .Returns(_databaseNameSectionMock.Object);
            _databaseNameSectionMock
                .Setup(x => x.Value)
                .Returns(databaseOverride);

            _config
                .Setup(x => x.GetSection(OptionsConstants.ProxyOptionsConfig))
                .Returns(new Mock<IConfigurationSection>().Object);

            _databaseOptions.Setup(x => x.Value).Returns(() =>
            {
                var options = new DatabaseOptions();
                new DatabaseOptionsConfiguration(_config.Object).Configure(options);
                options.ProxyOptions = new DbProxyOptions
                {
                    ReadOnlyEndpoint = "localhost-READ",
                    ReadWriteEndpoint = "localhost-WRITE",
                    Port = 5159,
                    Username = "me"
                };
                return options;
            });

            _tokenProviderMock
                .Setup(x => x.GetIamToken(It.IsAny<DbProxyOptions>(), allowWrites))
                .Returns(token);
        }

        [Theory]
        [InlineData("", 1, 0, 1)]
        [InlineData("", 2, 1, 1)]
        [InlineData("0", 2, 0, 2)]
        [InlineData("10", 2, 1, 1)]
        [InlineData("1", 3, 1.5, 1)]
        [InlineData("100", 2, 1, 1)]
        public void ExpiryTest(string ttlConfig, int providerCallCount, int secondsBetweenCalls, int expectedFactoryCallCount)
        {
            // Arrange
            MockServices(string.Empty, string.Empty, string.Empty, false, ttlConfig);

            var factoryMock = new Mock<IConnectionStringFactory>();
            factoryMock.Setup(mock => mock.Create(It.IsAny<bool>())).Returns("Connection String");

            var classUnderTest = new ConnectionStringProvider(
                _databaseOptions.Object,
                factoryMock.Object);

            // Act
            for (int i = 0; i < providerCallCount; i++)
            {
                var result = classUnderTest.WriteConnectionString;
                Thread.Sleep(secondsBetweenCalls * 100);
            }

            // Assert
            factoryMock.Verify(mock => mock.Create(It.IsAny<bool>()), Times.Exactly(expectedFactoryCallCount));
        }
    }
}