using LearnApi.Model;
using LearnApi.Repos;
using LearnApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LearnApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtSettings _jwtSettings;
        private readonly IRefreshHandler _refreshHandler;
        public AuthorizeController(ApplicationDbContext context, IOptions<JwtSettings> options, IRefreshHandler refreshHandler)
        {
            _context = context;
            _jwtSettings = options.Value;
            _refreshHandler = refreshHandler;
        }

        [HttpPost("GenerateToken")]
        public async Task<IActionResult> GenerateToken([FromBody] UserCredentials userCredentials)
        {
            var user = await _context.Users.Where(x => x.Username == userCredentials.Username && x.Password == userCredentials.Password).FirstOrDefaultAsync();
            if (user != null)
            {
                //generate token
                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenKey = Encoding.UTF8.GetBytes(_jwtSettings.securityKey);
                var tokenDesc = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name,user.Username),
                        new Claim(ClaimTypes.Role,user.Role),
                    }),
                    Expires = DateTime.Now.AddSeconds(30),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256)
                };
                var token = tokenHandler.CreateToken(tokenDesc);
                var finalToken = tokenHandler.WriteToken(token);
                return Ok(new TokenResponse() { Token = finalToken, RefreshToken = await _refreshHandler.GenerateRefreshToken(userCredentials.Username) });
            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpPost("GenerateRefreshToken")]
        public async Task<IActionResult> GenerateRefreshToken([FromBody] TokenResponse token)
        {
            var user = await _context.RefreshTokens.Where(x => x.RefreshToken1 == token.RefreshToken).FirstOrDefaultAsync();
            if (user != null)
            {
                //generate token
                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenKey = Encoding.UTF8.GetBytes(_jwtSettings.securityKey);
                SecurityToken securityToken;
                var principle = tokenHandler.ValidateToken(token.Token, new TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(tokenKey),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                }, out securityToken);

                var _token = securityToken as JwtSecurityToken;
                if (_token != null && _token.Header.Alg.Equals(SecurityAlgorithms.HmacSha256))
                {
                    string username = principle.Identity?.Name;
                    var checkUser = await _context.Users.Where(x => x.Username == username).FirstOrDefaultAsync();
                    var checkExistingRefreshToken = checkUser != null ? await _context.RefreshTokens.Where(x => x.UserId == checkUser.Id && x.RefreshToken1 == token.RefreshToken).FirstOrDefaultAsync() : null;
                    if (checkExistingRefreshToken != null)
                    {
                        var newToken = new JwtSecurityToken(
                            claims: principle.Claims.ToArray(),
                            expires: DateTime.Now.AddSeconds(30),
                            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.securityKey)),
                            SecurityAlgorithms.HmacSha256)
                            );

                        var finalToken = tokenHandler.WriteToken(newToken);
                        return Ok(new TokenResponse() { Token = finalToken, RefreshToken = await _refreshHandler.GenerateRefreshToken(username) });
                    }
                    else
                    {
                        return Unauthorized();
                    }
                }
                else
                {
                    return Unauthorized();
                }
                //return Ok(new TokenResponse() { Token = finalToken, RefreshToken = await _refreshHandler.GenerateRefreshToken(userCredentials.Username) });
            }
            else
            {
                return Unauthorized();
            }
        }
    }
}
