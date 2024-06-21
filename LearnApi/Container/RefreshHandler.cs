using LearnApi.Repos;
using LearnApi.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace LearnApi.Container
{
    public class RefreshHandler : IRefreshHandler
    {
        private readonly ApplicationDbContext _context;
        public RefreshHandler(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<string> GenerateRefreshToken(string username)
        {
            var randomNumber = new byte[32];
            using (var randomNumberGenerator = RandomNumberGenerator.Create())
            {
                randomNumberGenerator.GetBytes(randomNumber);
                string refreshToken = Convert.ToBase64String(randomNumber);
                var checkUser = await _context.Users.Where(x => x.Username == username).FirstOrDefaultAsync();
                if (checkUser != null)
                {
                    var checkExistingToken = await _context.RefreshTokens.Where(x => x.UserId == checkUser.Id).FirstOrDefaultAsync();
                    if (checkExistingToken != null)
                    {
                        checkExistingToken.RefreshToken1 = refreshToken;
                    }
                    else
                    {
                        await _context.RefreshTokens.AddAsync(new Repos.Models.RefreshToken
                        {
                            RefreshToken1 = refreshToken,
                            UserId = checkUser.Id
                        });
                    }
                    await _context.SaveChangesAsync();
                }
                return refreshToken;
            }
        }
    }
}
