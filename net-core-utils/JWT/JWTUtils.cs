using System;
using System.Text;
using System.Text.Json;

namespace CoreUtils;

public static class JwtUtils
{
    public static (string HeaderJson, string PayloadJson) DecodeJwt(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("token is required", nameof(token));

        var parts = token.Split('.');
        if (parts.Length < 2)
            throw new ArgumentException("invalid jwt token format", nameof(token));

        var headerJson = Encoding.UTF8.GetString(Base64UrlDecode(parts[0]));
        var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(parts[1]));
        return (headerJson, payloadJson);
    }

    public static JsonDocument DecodeJwtPayloadAsJson(string token)
    {
        var (_, payload) = DecodeJwt(token);
        return JsonDocument.Parse(payload);
    }

    private static byte[] Base64UrlDecode(string input)
    {
        string s = input.Replace('-', '+').Replace('_', '/');
        switch (s.Length % 4)
        {
            case 2: s += "=="; break;
            case 3: s += "="; break;
            case 0: break;
            default: throw new FormatException("Illegal base64url string!");
        }
        return Convert.FromBase64String(s);
    }
}