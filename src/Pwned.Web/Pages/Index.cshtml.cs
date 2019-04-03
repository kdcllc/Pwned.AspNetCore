using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Pwned.AspNetCore;
using Pwned.AspNetCore.Models;

namespace Pwned.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IPwnedBreachService _pwnedBreachService;
        private readonly IPwnedPasswordService _pwnedPasswordService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IPwnedBreachService pwnedService, 
            IPwnedPasswordService pwnedPasswordService,
            ILogger<IndexModel> logger)
        {
            _pwnedBreachService = pwnedService;
            _pwnedPasswordService = pwnedPasswordService;
            _logger = logger;
        }

        public string EmailAccount = "test@example.com";
        public IList<int> BreachesCountByAccount { get; private set; }
        public IList<int> BreachesAllCount { get; private set; }

        public IList<(bool pwned,long count)> Pwned { get; set; }
        public string Password = @"P@ssword";

        public async Task OnGetAsync()
        {
            var breaches = new List<int>();
            var allBreaches = new List<int>();
            var passwords = new List<(bool pwned, long count)>();

            for (var i = 0; i < 2; i++)
            {
                var breachesResult = await _pwnedBreachService.GetBreachesByAccountAsync(EmailAccount,true,false);
                breaches.Add(breachesResult.Count);

                //var result = await _pwnedService.GetAllBreachesAsync("adobe.com");
                var allBreachesResult = await _pwnedBreachService.GetAllBreachesAsync();
                allBreaches.Add(allBreachesResult.Count);

                //var result = await _pwnedBreachService.GetAllDataClasses();

                //var result = await _pwnedBreachService.GetPastByAccount(account);

                var passResult = await _pwnedPasswordService.IsPasswordPwnedAsync("P@ssword");
                passwords.Add(passResult);
            }

            BreachesCountByAccount = breaches;
            BreachesAllCount = allBreaches;
            Pwned = passwords;
        }
    }
}
