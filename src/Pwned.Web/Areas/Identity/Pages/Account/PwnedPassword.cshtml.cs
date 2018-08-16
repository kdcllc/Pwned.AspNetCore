using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Pwned.Web.Areas.Identity.Pages.Account
{
    public class PwnedPasswordModel : PageModel
    {
        public long Count { get; set; }
        public string ReturnUrl { get; set; }

        public IActionResult OnGet(long count, string returnUrl = null)
        {
            Count = count;
            ReturnUrl = returnUrl;

            return Page();
        }
    }
}
