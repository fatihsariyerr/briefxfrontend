﻿using System.Xml;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Npgsql;
namespace pulseui.Pages;

public class GamingInternationalModel : PageModel
{
    private readonly ILogger<GamingInternationalModel> _logger;




  public static string UsdRate { get; set; }
  public static string EuroRate { get; set; }
  public static string GramAltinRate { get; set; }
  public static string BtcRate { get; set; }

  public GamingInternationalModel(ILogger<GamingInternationalModel> logger)
    {
        _logger = logger;
    }
  private string _connectionString;
  public List<Haber> news { get; set; } = new List<Haber> { };
  public List<SonDakika> sondakikahaberleri { get; set; } = new List<SonDakika> { };


  public async Task<JsonResult> OnGetLoadMoreAsync(int skip, int take = 15)
  {
    var moreNews = new List<Haber>();
    _connectionString = "User ID=briefxdbuser;Password=Sariyer123.;Server=188.245.43.5;Port=32542;Database=briefxprod;";

    using (var connection = new NpgsqlConnection(_connectionString))
    {
      await connection.OpenAsync();
      string query = "SELECT * FROM newsinternational WHERE category='gaming' ORDER BY publishdate DESC LIMIT @take OFFSET @skip";

      using (var command = new NpgsqlCommand(query, connection))
      {
        command.Parameters.AddWithValue("@take", take);
        command.Parameters.AddWithValue("@skip", skip);

        using (var reader = await command.ExecuteReaderAsync())
        {
          while (await reader.ReadAsync())
          {
            var publishedAt = reader.GetDateTime(reader.GetOrdinal("publishdate"));


            DateTime now = DateTime.Now;
            var guncelzaman = publishedAt;
            TimeSpan timeDifference = now - guncelzaman;

            string timeAgo;
            if (guncelzaman.Day != now.Day)
            {
              timeAgo = guncelzaman.ToString() + " GMT";
            }
            else
            {
              timeAgo = guncelzaman.Hour.ToString() + ":" + guncelzaman.Minute.ToString("D2") + ":" + guncelzaman.Second.ToString("D2") + " GMT";
            }

            var haber = new Haber
            {
              Title = reader.GetString(reader.GetOrdinal("title")),
              ImageUrl = reader.IsDBNull(reader.GetOrdinal("image")) ? "/assets/img/briefxlogo.png" : reader.GetString(reader.GetOrdinal("image")),
              Link = reader.GetString(reader.GetOrdinal("link")),
              Publisher = reader.GetString(reader.GetOrdinal("publisher")),
              PublishedAtFormatted = timeAgo
            };

            moreNews.Add(haber);
          }
        }
      }
    }

    return new JsonResult(moreNews);
  }
  public async Task<string> FetchBTCLastFieldValue()
  {
    var url = "https://webservice.foreks.com/foreks-web-widget/qbOBC";
    using (HttpClient client = new HttpClient())
    {
      var response = await client.GetAsync(url);
      var pageContent = await response.Content.ReadAsStringAsync();
      var htmlDoc = new HtmlDocument();

      htmlDoc.LoadHtml(pageContent);

      var lastFieldNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='lastField']");
      if (lastFieldNode != null)
      {
        string originalValue = lastFieldNode.InnerText.Trim();


        var numericValue = double.Parse(originalValue.Replace("$", "").Replace(",", ""), System.Globalization.CultureInfo.InvariantCulture);

        var formattedValue = $"{Math.Floor(numericValue):N0}";

        return formattedValue;
      }

      return null;
    }
  }

  public async Task<string> FetchLastFieldValue()
  {

    var url = "https://webservice.foreks.com/foreks-web-widget/RoyVJ";
    using (HttpClient client = new HttpClient())
    {
      var response = await client.GetAsync(url);
      var pageContent = await response.Content.ReadAsStringAsync();
      var htmlDoc = new HtmlDocument();

      htmlDoc.LoadHtml(pageContent);

      var lastFieldNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='lastField']");
      if (lastFieldNode != null)
      {
        string originalValue = lastFieldNode.InnerText.Trim();


        var numericValue = double.Parse(originalValue.Replace("$", "").Replace(",", ""), System.Globalization.CultureInfo.InvariantCulture);

        var formattedValue = $"{Math.Floor(numericValue):N0}";
        return formattedValue;
      }

      return null;
    }
  }





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
        XmlNode forexBuyingNode = usdNode.SelectSingleNode("ForexSelling");
        if (forexBuyingNode != null)
        {
          string originalValue = forexBuyingNode.InnerText;
          int decimalIndex = originalValue.IndexOf('.');

          if (decimalIndex != -1 && decimalIndex + 3 <= originalValue.Length)
          {

            string result = originalValue.Substring(0, decimalIndex + 3);
            return result;
          }



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
        XmlNode forexBuyingNode = usdNode.SelectSingleNode("ForexSelling");
        if (forexBuyingNode != null)
        {
          string originalValue = forexBuyingNode.InnerText;
          int decimalIndex = originalValue.IndexOf('.');

          if (decimalIndex != -1 && decimalIndex + 3 <= originalValue.Length)
          {

            string result = originalValue.Substring(0, decimalIndex + 3);
            return result;
          }
        }
      }
      return "Değer bulunamadı";
    }
  }

  public async Task OnGetAsync(string search = null)
  {

    BtcRate = await FetchBTCLastFieldValue();
    GramAltinRate = await FetchLastFieldValue();
    UsdRate = await GetUsdRateAsync();
    EuroRate = await GetEuroRateAsync();


    Sondakika();
    _connectionString = "User ID=briefxdbuser;Password=Sariyer123.;Server=188.245.43.5;Port=32542;Database=briefxprod;";

    using (var connection = new NpgsqlConnection(_connectionString))
    {
      connection.Open();
      string query = "SELECT * FROM newsinternational where category='gaming' ORDER BY publishdate DESC";
      if (!string.IsNullOrEmpty(search))
      {
        query = "SELECT * FROM newsinternational where title ILIKE @search ORDER BY publishdate DESC";
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
            var guncelzaman = publishedAt;
            TimeSpan timeDifference = now - guncelzaman;

            string timeAgo;
            if (guncelzaman.Day != now.Day)
            {
              timeAgo = guncelzaman.ToString() + " GMT";
            }
            else
            {
              timeAgo = guncelzaman.Hour.ToString() + ":" + guncelzaman.Minute.ToString("D2") + ":" + guncelzaman.Second.ToString("D2") + " GMT";
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
                        FROM newsinternational
                        WHERE category IN ('news', 'sport', 'life', 'tech')
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
              timeAgo = $"{(int)timeDifference.TotalMinutes} minutes ago";
            }
            else if (timeDifference.TotalHours < 24)
            {
              timeAgo = $"{(int)timeDifference.TotalHours} hours ago";
            }
            else
            {
              timeAgo = $"{(int)timeDifference.TotalDays} days ago";
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

