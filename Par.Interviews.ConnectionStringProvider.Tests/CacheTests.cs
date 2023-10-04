using System;
using System.Threading;
using Xunit;

namespace Par.Interviews.ConnectionStringProvider.Tests
{
    public class CacheTests
    {
        [Fact]
        public void ZeroTtlShouldAlwaysCallFactory()
        {
            // Arrange
            var expiringLazy = new Cache<long>(() => DateTime.Now.Ticks, 0);

            // Act
            var value1 = expiringLazy.Value;
            var value2 = expiringLazy.Value;

            // Assert
            Assert.NotEqual(value1, value2);
        }

        [Fact]
        public void CallBeforeExpiryShouldNotCallValueFactoryAgain()
        {
            // Arrange
            var expiringLazy = new Cache<long>(() => DateTime.Now.Ticks, 5);

            // Act
            var value1 = expiringLazy.Value;
            var value2 = expiringLazy.Value;

            // Assert
            Assert.Equal(value1, value2);
        }

        [Fact]
        public void CallAfterExpiryShouldCallValueFactory()
        {
            // Arrange
            var expiringLazy = new Cache<long>(() => DateTime.Now.Ticks, 1);

            // Act
            var value1 = expiringLazy.Value;
            Thread.Sleep(1500);
            var value2 = expiringLazy.Value;

            // Assert
            Assert.NotEqual(value1, value2);
        }
    }
}