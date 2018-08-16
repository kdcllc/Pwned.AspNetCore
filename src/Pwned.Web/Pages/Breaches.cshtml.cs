using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pwned.AspNetCore;
using Pwned.AspNetCore.Models;

namespace Pwned.Web.Pages
{
    public class BreachesModel : PageModel
    {
        private readonly IPwnedBreachService _pwnedBreach;

        public List<Breach> Breaches { get; set; }

        public BreachesModel(IPwnedBreachService pwnedBreach)
        {
            _pwnedBreach = pwnedBreach;
        }

        public async Task<IActionResult> OnGet(string emailAccount="")
        {
            var result = new List<Breach>();

            if (!string.IsNullOrEmpty(emailAccount))
            {
                result = await _pwnedBreach.GetBreachesByAccountAsync(emailAccount);
            }
            else
            {
                result = await _pwnedBreach.GetAllBreachesAsync();
            }

            Breaches = result;
            return Page();
        }
    }
}
