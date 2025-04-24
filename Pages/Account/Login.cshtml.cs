using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace pulseui.Pages.Account
{
  public class LoginModel : PageModel
    {
    public IActionResult OnGet()
    {
      if (User.Identity.IsAuthenticated)
      {
        return RedirectToPage("/Index");
      }

      return Page();
    }

    public IActionResult OnPostGoogle()
    {
      var properties = new AuthenticationProperties
      {
        RedirectUri = Url.Page("/Account/ExternalLoginCallback",
                                new { returnUrl = Url.Page("/Index") })
      };

      return Challenge(properties, "Google");
    }
  }
}

