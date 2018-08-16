using System.Net.Http;
using System.Threading;

namespace Pwned.AspNetCore
{
    public interface IPwnedPasswordService
    {
        HttpClient Client { get; }

        System.Threading.Tasks.Task<(bool pwned, long count)> IsPasswordPwned(string password, CancellationToken token = default);
    }
}