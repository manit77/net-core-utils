using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace net_core_utils_test
{
    [TestClass]
    public class TestJWTTokenManager
    {
        [TestMethod]
        public void TestToken()
        {
            string key = "asdfasdfasdfasdfasdfasdfasdfasdfasdfasdf";
            CoreUtils.JWTTokenManager jWTTokenManager = new CoreUtils.JWTTokenManager(key);
            Dictionary<string, string> claims = new Dictionary<string, string>();
            claims["username"] = "testusername";
            claims["userroles"] = "admin,user";

            string token = jWTTokenManager.GenerateToken(claims, 1);

            Debug.WriteLine(token);

            Assert.IsNotNull(token);
            Assert.IsTrue(token.Length > 0);

            //verify the claims
            var verifiedClaims = jWTTokenManager.GetClaims(token);

            foreach (var claim in verifiedClaims.Claims)
            {
                Debug.WriteLine($"{claim}");
            }
            
            Assert.IsNotNull(verifiedClaims);
            Assert.IsTrue(verifiedClaims.Claims.Count() > 0);



        }
    }
}
