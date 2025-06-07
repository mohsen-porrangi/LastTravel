using BuildingBlocks.Contracts.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace UserManagement.API.Common;

public class JwtTokenService(IOptions<AutenticationOptions> autenticationOption, IConfiguration config) : ITokenService
{



    public string GenerateToken(User user, IEnumerable<string> permissionCodes)
    {

        var claims = new List<Claim>
            {
                     new("MasterIdentityId", user.IdentityId.ToString()),
                     new("UserId", user.Id.ToString()),
                     new(ClaimTypes.Name, $"{user.Name} {user.Family}" ?? string.Empty),
                     new(ClaimTypes.MobilePhone, user.MasterIdentity.Mobile),
                     new("NationalCode", user.NationalCode ?? string.Empty)
            };

        claims.AddRange(permissionCodes.Select(code => new Claim("permission", code)));

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(autenticationOption.Value.SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
               issuer: autenticationOption.Value.Issuer,
               audience: autenticationOption.Value.Audience,
               claims: claims,
               expires: DateTime.UtcNow.AddMinutes(autenticationOption.Value.TokenExpirationMinutes),
               signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }



    public bool ValidateToken(string token)
    {
        if (string.IsNullOrEmpty(token))
            return false;
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(autenticationOption.Value.SecretKey);
        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = autenticationOption.Value.Issuer,
                ValidateAudience = true,
                ValidAudience = autenticationOption.Value.Audience,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);
            return true;
        }
        catch
        {
            return false;
        }
    }
    //    var claims = new List<Claim>
    //    {
    //        new(ClaimTypes.NameIdentifier, user.Id.ToString()),
    //        new(ClaimTypes.Name, $"{user.Name} {user.Family}")
    //    };

    //    claims.AddRange(permissionCodes.Select(code => new Claim("permission", code)));

    //    var key = new SymmetricSecurityKey(
    //        Encoding.UTF8.GetBytes(config["Jwt:Key"]!)
    //    );

    //    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    //    var token = new JwtSecurityToken(
    //        issuer: config["Jwt:Issuer"],
    //        audience: config["Jwt:Audience"],
    //        claims: claims,
    //        expires: DateTime.UtcNow.AddHours(2),
    //        signingCredentials: creds);

    //    return new JwtSecurityTokenHandler().WriteToken(token);
    //}

    //public bool ValidateToken(string token)
    //{
    //    if (string.IsNullOrEmpty(token))
    //        return false;

    //    var tokenHandler = new JwtSecurityTokenHandler();
    //    var key = Encoding.UTF8.GetBytes(config["Jwt:Key"]!);

    //    try
    //    {
    //        tokenHandler.ValidateToken(token, new TokenValidationParameters
    //        {
    //            ValidateIssuerSigningKey = true,
    //            IssuerSigningKey = new SymmetricSecurityKey(key),
    //            ValidateIssuer = true,
    //            ValidIssuer = config["Jwt:Issuer"],
    //            ValidateAudience = true,
    //            ValidAudience = config["Jwt:Audience"],
    //            ClockSkew = TimeSpan.Zero
    //        }, out SecurityToken validatedToken);

    //        return true;
    //    }
    //    catch
    //    {
    //        return false;
    //    }
    //}    
}