using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace Unite.Analysis.Web.Handlers.Helpers;

public static class TokenHelper
{
    private static JwtSecurityTokenHandler _tokenHandler = new();

    public static string Generate(byte[] keyBytes)
    {
        var key = new SymmetricSecurityKey(keyBytes);
        var algorythm = SecurityAlgorithms.HmacSha256Signature;
        var credentials = new SigningCredentials(key, algorythm);
        
        var descriptor = new SecurityTokenDescriptor
        {
            // Subject = new System.Security.Claims.ClaimsIdentity([new System.Security.Claims.Claim("id", "analysis@unite.net")]),
            Expires = DateTime.UtcNow.AddMinutes(10),
            SigningCredentials = credentials
        };

        var tokenValue = _tokenHandler.CreateToken(descriptor);
        var tokenString = _tokenHandler.WriteToken(tokenValue);

        return tokenString;
    }
}
