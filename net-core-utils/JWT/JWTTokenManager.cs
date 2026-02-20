using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;


namespace CoreUtils
{

    public interface IJWTTokenManager
    {
        string GenerateToken(IReadOnlyDictionary<string, string> claims, string issuer = "", string audience = "", int expiresMinutes = 60, string encryptionAlgorithm = SecurityAlgorithms.HmacSha256Signature);
        ClaimsPrincipal GetClaims(string token);
    }

    public class JWTTokenManager : IJWTTokenManager
    {
        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        byte[] secretKey;

        public JWTTokenManager() { }

        public JWTTokenManager(string key)
        {
            this.secretKey = Encoding.UTF8.GetBytes(key);
        }

        public string GenerateToken(IReadOnlyDictionary<string, string> claims, string issuer = "", string audience = "", int expiresMinutes = 60, string encryptionAlgorithm = SecurityAlgorithms.HmacSha256Signature)
        {
            var payloadClaims = claims.Select(c => new Claim(c.Key, c.Value));

            var tokenDesc = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(payloadClaims),
                Expires = DateTime.UtcNow.AddMinutes(expiresMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(this.secretKey), encryptionAlgorithm),
                Issuer = issuer,
                Audience = audience
            };

            var token = tokenHandler.CreateToken(tokenDesc);
            return tokenHandler.WriteToken(token);

        }

        public ClaimsPrincipal GetClaims(string token)
        {
            var claims = this.tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = false,
                ValidateLifetime = true,
                ValidateAudience = false,
                IssuerSigningKey = new SymmetricSecurityKey(this.secretKey),
                ClockSkew = TimeSpan.Zero,
            }, out SecurityToken validatedToken);
            return claims;
        }
    }

}
