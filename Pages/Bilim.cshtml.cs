using System.Xml;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Npgsql;
namespace pulseui.Pages;

public class BilimModel : PageModel
{
    private readonly ILogger<BilimModel> _logger;



  public BilimModel(ILogger<BilimModel> logger)
    {
        _logger = logger;
    }
 
  private string _connectionString;
  public List<Haber> news { get; set; } = new List<Haber> { };
  public List<SonDakika> sondakikahaberleri { get; set; } = new List<SonDakika> { };
  public async Task<string> GetEuroRateAsync()
  {
    var url = "https://www.tcmb.gov.tr/kurlar/today.xml";
    using (HttpClient client = new HttpClient())
    {
      var response = await client.GetStringAsync(url);
      XmlDocument xmlDocument = new XmlDocument();
      xmlDocument.LoadXml(response);

      XmlNode usdNode = xmlDocument.SelectSingleNode("//Currency[@CurrencyCode='EUR']");
      if (usdNode != null)
      {
        XmlNode forexBuyingNode = usdNode.SelectSingleNode("ForexBuying");
        if (forexBuyingNode != null)
        {
          return forexBuyingNode.InnerText;
        }
      }
      return "Değer bulunamadı";
    }
  }
  public async Task<string> GetUsdRateAsync()
  {
    var url = "https://www.tcmb.gov.tr/kurlar/today.xml";
    using (HttpClient client = new HttpClient())
    {
      var response = await client.GetStringAsync(url);
      XmlDocument xmlDocument = new XmlDocument();
      xmlDocument.LoadXml(response);

      XmlNode usdNode = xmlDocument.SelectSingleNode("//Currency[@CurrencyCode='USD']");
      if (usdNode != null)
      {
        XmlNode forexBuyingNode = usdNode.SelectSingleNode("ForexBuying");
        if (forexBuyingNode != null)
        {
          return forexBuyingNode.InnerText;
        }
      }
      return "Değer bulunamadı";
    }
  }
  public void OnGet(string search = null) 
  {
  
    Sondakika();
    _connectionString = "User ID=briefxdbuser;Password=Sariyer123.;Server=188.245.43.5;Port=32542;Database=briefxprod;";

    using (var connection = new NpgsqlConnection(_connectionString))
    {
      connection.Open();

      string query = "SELECT * FROM news where category='bilim' ORDER BY publishdate DESC";
      if (!string.IsNullOrEmpty(search))
      {
        query = " SELECT * FROM news where title ILIKE @search ORDER BY publishdate DESC";
      }

      using (var command = new NpgsqlCommand(query, connection))
      {
        if (!string.IsNullOrEmpty(search))
        {
          command.Parameters.AddWithValue("@search", "%" + search + "%"); 
        }

        using (var reader = command.ExecuteReader())
        {
          while (reader.Read())
          {
            var publishedAt = reader.GetDateTime(reader.GetOrdinal("publishdate")); 

            DateTime now = DateTime.Now;
            TimeSpan timeDifference = now - publishedAt;

            string timeAgo;
            if (timeDifference.TotalMinutes < 60)
            {
              timeAgo = $"{(int)timeDifference.TotalMinutes} dakika önce";
            }
            else if (timeDifference.TotalHours < 24)
            {
              timeAgo = $"{(int)timeDifference.TotalHours} saat önce";
            }
            else
            {
              timeAgo = $"{(int)timeDifference.TotalDays} gün önce";
            }


            var cekilenhaber = new Haber
            {
              Title = reader.GetString(reader.GetOrdinal("title")),
              ImageUrl = reader.IsDBNull(reader.GetOrdinal("image")) ? "/assets/img/briefxlogo.png" : reader.GetString(reader.GetOrdinal("image")),
              Link = reader.GetString(reader.GetOrdinal("link")),
              Publisher = reader.GetString(reader.GetOrdinal("publisher")),
              PublishedAtFormatted = timeAgo 
            };

            news.Add(cekilenhaber);
          }
        }
      }
    }
  }

  public void Sondakika()
  {
    _connectionString = "User ID=briefxdbuser;Password=Sariyer123.;Server=188.245.43.5;Port=32542;Database=briefxprod;";

    using (var connection = new NpgsqlConnection(_connectionString))
    {
      connection.Open();

      string query = @"
            SELECT *
            FROM (
                SELECT *,
                       ROW_NUMBER() OVER (PARTITION BY category ORDER BY publishdate DESC) AS rn
                FROM news
                WHERE category IN ('gundem', 'spor', 'yasam', 'bilim')
            ) AS subquery
            WHERE rn = 1;";



      using (var command = new NpgsqlCommand(query, connection))
      {


        using (var reader = command.ExecuteReader())
        {
          while (reader.Read())
          {
            var publishedAt = reader.GetDateTime(reader.GetOrdinal("publishdate"));

            DateTime now = DateTime.Now;
            TimeSpan timeDifference = now - publishedAt;

            string timeAgo;
            if (timeDifference.TotalMinutes < 60)
            {
              timeAgo = $"{(int)timeDifference.TotalMinutes} dakika önce";
            }
            else if (timeDifference.TotalHours < 24)
            {
              timeAgo = $"{(int)timeDifference.TotalHours} saat önce";
            }
            else
            {
              timeAgo = $"{(int)timeDifference.TotalDays} gün önce";
            }


            var cekilenhaber = new SonDakika
            {
              Title = reader.GetString(reader.GetOrdinal("title")),
              ImageUrl = reader.IsDBNull(reader.GetOrdinal("image")) ? "/assets/img/briefxlogo.png" : reader.GetString(reader.GetOrdinal("image")),
              Link = reader.GetString(reader.GetOrdinal("link")),
              Publisher = reader.GetString(reader.GetOrdinal("publisher")),
              PublishedAtFormatted = timeAgo
            };

            sondakikahaberleri.Add(cekilenhaber);
          }
        }
      }
    }
  }

  public class SonDakika
  {
    public int Id { get; set; }
    public string Title { get; set; }
    public string ImageUrl { get; set; }
    public string Link { get; set; }
    public string Publisher { get; set; }
    public string PublishedAtFormatted { get; set; }
    public string Category { get; set; }
  }

  public class Haber
    {
      public int Id { get; set; }
      public string Title { get; set; }
      public string ImageUrl { get; set; }
      public string Link { get; set; }
      public string Publisher { get; set; }
      public string PublishedAtFormatted { get; set; }
      public string Category { get; set; }
  }

}

