using System.Collections.Generic;
using System.Diagnostics;
using CoreUtils;
using FluentAssertions;
using Xunit;

namespace net_core_utils_test
{
    public class TestJWTTokenManager
    {
        [Fact]
        public void TestToken()
        {
            // Arrange
            string key = "asdfasdfasdfasdfasdfasdfasdfasdfasdfasdf";
            var jwtManager = new JWTTokenManager(key);

            var claims = new Dictionary<string, string>
            {
                ["username"] = "testusername",
                ["userroles"] = "admin,user"
            };

            // Act
            string token = jwtManager.GenerateToken(claims, "", "", 1);

            Debug.WriteLine($"Generated Token: {token}");

            // Assert
            token.Should().NotBeNullOrEmpty("token should be generated successfully");

            var verifiedClaims = jwtManager.GetClaims(token);

            verifiedClaims.Should().NotBeNull("claims should be verified successfully");
            verifiedClaims.Claims.Should().NotBeEmpty("claims should contain entries");

            // Verify specific claims by Type
            verifiedClaims.Claims.Should().ContainSingle(c => c.Type == "username" && c.Value == "testusername");
            verifiedClaims.Claims.Should().ContainSingle(c => c.Type == "userroles" && c.Value == "admin,user");

            // Output for debugging
            foreach (var claim in verifiedClaims.Claims)
            {
                Debug.WriteLine($"{claim.Type} = {claim.Value}");
            }
        }
    }
}
