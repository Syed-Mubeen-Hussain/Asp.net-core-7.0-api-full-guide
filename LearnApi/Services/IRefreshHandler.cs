namespace LearnApi.Services
{
    public interface IRefreshHandler
    {
        Task<string> GenerateRefreshToken(string username);
    }
}
