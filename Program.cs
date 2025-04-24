using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using pulseui.Pages;

var builder = WebApplication.CreateBuilder(args);

// Authentication servislerini ekleyin
builder.Services.AddAuthentication(options =>
{
  options.DefaultScheme = "Cookies";
  options.DefaultChallengeScheme = "Google";
})
.AddCookie("Cookies", options =>
{
  options.LoginPath = "/Account/Login";
  options.LogoutPath = "/Account/Logout";
})
.AddGoogle("Google", options =>
{
  // Google Cloud Console'dan alınan bilgilerle değiştirilecek
  options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
  options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
  options.CallbackPath = "/signin-google";
});


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
builder.Services.AddAuthentication();
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
  app.UseExceptionHandler("/Error");
  app.UseHsts();  // HSTS için
}

app.UseStaticFiles();
app.UseRouting();

// Authentication middleware'i UseAuthorization'dan önce ekleyin
app.UseAuthentication();
app.UseSession();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
