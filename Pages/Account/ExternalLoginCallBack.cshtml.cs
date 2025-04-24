using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace pulseui.Pages.Account
{
  public class ExternalLoginCallBackModel : PageModel
    {
    public async Task<IActionResult> OnGetAsync(string returnUrl = null)
    {
      var result = await HttpContext.AuthenticateAsync("External");

      if (!result.Succeeded)
      {
        return RedirectToPage("/Account/Login");
      }

      // Gerekli kullanıcı bilgilerini alın
      var emailClaim = result.Principal.FindFirst(ClaimTypes.Email);
      var nameClaim = result.Principal.FindFirst(ClaimTypes.Name);

      // Kimlik oluşturun
      var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, emailClaim?.Value),
                new Claim(ClaimTypes.Name, nameClaim?.Value),
                // İhtiyacınıza göre diğer talepleri ekleyin
            };

      var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
      var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

      // Kullanıcının kimliğini kaydedin
      await HttpContext.SignInAsync("Cookies", claimsPrincipal);

      // Harici kimlik doğrulamadan çıkış yapın
      await HttpContext.SignOutAsync("External");

      if (returnUrl != null)
      {
        return LocalRedirect(returnUrl);
      }

      return RedirectToPage("/Index");
    }
  }
}

