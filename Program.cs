using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using pulseui.Pages;
using pulseui.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddHostedService<SitemapBackgroundService>();
builder.Services.AddRazorPages();
builder.Services.AddRazorPages()
        .AddMvcOptions(options =>
        {
          options.Filters.Add(new IgnoreAntiforgeryTokenAttribute());
        });
builder.Services.AddSession(options =>
{
  options.IdleTimeout = TimeSpan.FromMinutes(30);
  options.Cookie.HttpOnly = true;
  options.Cookie.IsEssential = true;
});
builder.Services.AddHttpClient();
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
  app.UseExceptionHandler("/Error");
  app.UseHsts();  // HSTS için
}

app.UseStaticFiles();
app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
