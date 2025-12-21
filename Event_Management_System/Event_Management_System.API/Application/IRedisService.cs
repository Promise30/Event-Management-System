namespace Event_Management_System.API.Application
{
    public interface IRedisService
    {
        Task<T> GetAsync<T>(string key);
        Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> valueFactory, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null);
        Task RemoveAsync(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null);

    }
}
