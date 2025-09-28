using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace pulseui.Services
{
  public class SitemapBackgroundService : BackgroundService
  {
    private readonly ILogger<SitemapBackgroundService> _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly string _connectionString = "User ID=briefxdbuser;Password=Sariyer123.;Server=188.245.92.157;Port=32542;Database=briefxprod;";
    private readonly string _baseUrl = "https://briefx.app"; // Sitenizin URL'i
    private readonly TimeSpan _updateInterval = TimeSpan.FromMinutes(5); // 5 dakika

    public SitemapBackgroundService(ILogger<SitemapBackgroundService> logger, IWebHostEnvironment environment)
    {
      _logger = logger;
      _environment = environment;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      _logger.LogInformation("Sitemap Background Service başlatıldı: {time}", DateTimeOffset.Now);

      while (!stoppingToken.IsCancellationRequested)
      {
        try
        {
          await GenerateSitemapXmlAsync();
          _logger.LogInformation("Sitemap.xml güncellendi: {time}", DateTimeOffset.Now);
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Sitemap güncellenirken hata oluştu");
        }

        await Task.Delay(_updateInterval, stoppingToken);
      }
    }

    private async Task GenerateSitemapXmlAsync()
    {
      // XML namespace tanımı
      XNamespace xmlns = "http://www.sitemaps.org/schemas/sitemap/0.9";

      // Ana sitemap elementini oluştur
      var root = new XElement(xmlns + "urlset");

      // Ana sayfa URL'i ekle
      root.Add(
          new XElement(xmlns + "url",
              new XElement(xmlns + "loc", _baseUrl),
              new XElement(xmlns + "lastmod", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:sszzz")),
              new XElement(xmlns + "changefreq", "daily"),
              new XElement(xmlns + "priority", "1.0")
          )
      );

      // Diğer statik sayfaları ekle
      var staticPages = new[] {
                "/Bilim",
                "/Yasam",
                "/Gaming",
                "/Testler",
                 "/Spor",
                "/News",
                "/Life",
                "/Technology",
                 "/GamingInternational",
                 "/Sport",
                "/Tests",
                "/Index",
                "/"
            };

      foreach (var page in staticPages)
      {
        root.Add(
            new XElement(xmlns + "url",
                new XElement(xmlns + "loc", $"{_baseUrl}{page}"),
                new XElement(xmlns + "lastmod", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:sszzz")),
                new XElement(xmlns + "changefreq", "weekly"),
                new XElement(xmlns + "priority", "0.8")
            )
        );
      }

      using (var connection = new NpgsqlConnection(_connectionString))
      {
        await connection.OpenAsync();

        string query = "SELECT slug, publishdate FROM news ORDER BY publishdate DESC";

        using (var command = new NpgsqlCommand(query, connection))
        using (var reader = await command.ExecuteReaderAsync())
        {
          while (await reader.ReadAsync())
          {
            var slug = reader.GetString(reader.GetOrdinal("slug"));
            var publishDate = reader.GetDateTime(reader.GetOrdinal("publishdate"));

            // Detail sayfası URL'i: briefx.app/detail/slug
            var url = $"{_baseUrl}/detail/{slug}";

            root.Add(
                new XElement(xmlns + "url",
                    new XElement(xmlns + "loc", url),
                    new XElement(xmlns + "lastmod", publishDate.ToString("yyyy-MM-ddTHH:mm:sszzz")),
                    new XElement(xmlns + "changefreq", "weekly"),
                    new XElement(xmlns + "priority", "0.7")
                )
            );
          }
        }
      }

      // XML dokümanını oluştur
      var document = new XDocument(
          new XDeclaration("1.0", "utf-8", null),
          root);

      // XML dosyasını wwwroot/sitemap.xml olarak kaydet
      string sitemapPath = Path.Combine(_environment.WebRootPath, "sitemap.xml");

      await using (var fileStream = new FileStream(sitemapPath, FileMode.Create, FileAccess.Write))
      {
        await document.SaveAsync(fileStream, SaveOptions.None, CancellationToken.None);
      }
    }
  }
}