using Amazon;
using Amazon.RDS.Util;

namespace Par.Interviews.ConnectionStringProvider
{
    public interface IAwsIamTokenProvider
    {
        string GetIamToken(DbProxyOptions options, bool allowWrites);
    }

    public class AwsIamTokenProvider : IAwsIamTokenProvider
    {
        public string GetIamToken(DbProxyOptions options, bool allowWrites)
        {
            return allowWrites
                ? RDSAuthTokenGenerator.GenerateAuthToken(RegionEndpoint.USEast1, options.ReadWriteEndpoint, options.Port, options.Username)
                : RDSAuthTokenGenerator.GenerateAuthToken(RegionEndpoint.USEast1, options.ReadOnlyEndpoint, options.Port, options.Username);
        }
    }
}