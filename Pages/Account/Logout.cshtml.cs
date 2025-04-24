using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace pulseui.Pages.Account
{
  public class LogoutModel : PageModel
    {
    public async Task<IActionResult> OnGetAsync()
    {
      // Kullanıcı oturumunu sonlandır
      await HttpContext.SignOutAsync("Cookies");

      // Ana sayfaya yönlendir
      return RedirectToPage("/Index");
    }
  }
}

