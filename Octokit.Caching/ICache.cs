using System.Threading.Tasks;

namespace Octokit.Caching
{
    public interface ICache
    {
        Task<T> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value);
    }
}