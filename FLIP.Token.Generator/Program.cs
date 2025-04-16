using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;


var tokenHandler = new JwtSecurityTokenHandler();
var key = Encoding.ASCII.GetBytes("FLIPSecretTokenForProcessingIds!!");

var tokenDescriptor = new SecurityTokenDescriptor
{
    Subject = new ClaimsIdentity([]),
    Expires = DateTime.UtcNow.AddMonths(3),
    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
};

var token = tokenHandler.CreateToken(tokenDescriptor);

Console.WriteLine(tokenHandler.WriteToken(token));
