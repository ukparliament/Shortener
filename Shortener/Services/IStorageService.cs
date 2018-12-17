namespace Shortener
{
    using System.Threading.Tasks;

    public interface IStorageService
    {
        Task Add(string id, string url);

        Task<bool> ContainsKey(string id);

        Task<string> GetValue(string id);
    }
}