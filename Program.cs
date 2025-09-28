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
app.MapGet("/manifest.json", async (HttpContext context) =>
{
  context.Response.ContentType = "application/json";
  var manifestPath = Path.Combine(app.Environment.WebRootPath, "manifest.json");

  if (File.Exists(manifestPath))
  {
    await context.Response.SendFileAsync(manifestPath);
  }
  else
  {
    context.Response.StatusCode = 404;
    await context.Response.WriteAsync("Manifest bulunamadı");
  }
});

// Service Worker endpoint
app.MapGet("/sw.js", async (HttpContext context) =>
{
  context.Response.ContentType = "application/javascript";
  var swPath = Path.Combine(app.Environment.WebRootPath, "sw.js");

  if (File.Exists(swPath))
  {
    await context.Response.SendFileAsync(swPath);
  }
  else
  {
    context.Response.StatusCode = 404;
    await context.Response.WriteAsync("Service Worker bulunamadı");
  }
});

// Offline sayfası için fallback
app.MapFallback(async (HttpContext context) =>
{
  // Eğer istek bir API değilse ve HTML kabul ediyorsa
  if (context.Request.Headers.Accept.ToString().Contains("text/html"))
  {
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync(
        Path.Combine(app.Environment.WebRootPath, "offline.html")
    );
  }
  else
  {
    context.Response.StatusCode = 404;
  }
});
app.Run();
