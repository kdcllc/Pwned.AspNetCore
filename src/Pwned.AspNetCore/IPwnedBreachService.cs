using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Pwned.AspNetCore.Models;

namespace Pwned.AspNetCore
{
    public interface IPwnedBreachService
    {
        HttpClient Client { get; }

        Task<List<Breach>> GetAllBreachesAsync(string domain = "", CancellationToken token = default);
        Task<List<string>> GetAllDataClasses(CancellationToken token = default);
        Task<List<Breach>> GetBreachesByAccountAsync(string emailAccount, bool unVerified = false, bool fullResponse = true, string domain = "", CancellationToken token = default);
        Task<List<PastAccount>> GetPastByAccount(string emailAccount, CancellationToken token = default);
    }
}