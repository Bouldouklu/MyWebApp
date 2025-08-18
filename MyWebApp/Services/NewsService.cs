using System.Text.Json;
using System.Text.Json.Serialization;

namespace MyWebApp.Services
{
    public class NewsService
    {
        private readonly HttpClient _httpClient;
        private const string TechSpotRssUrl = "https://www.techspot.com/backend.xml";
        private const string Rss2JsonApiUrl = "https://api.rss2json.com/v1/api.json";
        private const string FallbackApiUrl = "https://www.toptal.com/developers/feed2json/convert";

        public NewsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            // Set a user agent to avoid potential blocking
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
        }

        public async Task<List<NewsArticle>?> GetTechSpotNewsAsync(int count = 10)
        {
            Console.WriteLine($"🔄 Starting GetTechSpotNewsAsync with count: {count}");
            
            // Try primary RSS2JSON service first
            var result = await TryRss2JsonAsync(count);
            if (result != null)
            {
                Console.WriteLine($"✅ Primary service returned {result.Count} articles");
                return result;
            }
            
            Console.WriteLine("⚠️ Primary service failed, trying fallback...");
            
            // Try fallback service
            result = await TryFallbackServiceAsync(count);
            if (result != null)
            {
                Console.WriteLine($"✅ Fallback service returned {result.Count} articles");
                return result;
            }
            
            Console.WriteLine("❌ All services failed");
            return null;
        }

        private async Task<List<NewsArticle>?> TryRss2JsonAsync(int count)
        {
            try
            {
                var apiUrl = $"{Rss2JsonApiUrl}?rss_url={Uri.EscapeDataString(TechSpotRssUrl)}&count={count}";
                Console.WriteLine($"🌐 Calling: {apiUrl}");
                
                var response = await _httpClient.GetStringAsync(apiUrl);
                Console.WriteLine($"📋 Response length: {response.Length} characters");
                Console.WriteLine($"📄 Response preview: {response.Substring(0, Math.Min(200, response.Length))}...");
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                var rssResponse = JsonSerializer.Deserialize<Rss2JsonResponse>(response, options);
                
                if (rssResponse?.Status == "ok" && rssResponse.Items != null)
                {
                    Console.WriteLine($"✅ Parsed {rssResponse.Items.Count} items from RSS2JSON");
                    return ProcessArticles(rssResponse.Items);
                }
                else
                {
                    Console.WriteLine($"❌ RSS2JSON Error - Status: {rssResponse?.Status}, Message: {rssResponse?.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ RSS2JSON Exception: {ex.Message}");
            }
            
            return null;
        }

        private async Task<List<NewsArticle>?> TryFallbackServiceAsync(int count)
        {
            try
            {
                var apiUrl = $"{FallbackApiUrl}?url={Uri.EscapeDataString(TechSpotRssUrl)}";
                Console.WriteLine($"🌐 Calling fallback: {apiUrl}");
                
                var response = await _httpClient.GetStringAsync(apiUrl);
                Console.WriteLine($"📋 Fallback response length: {response.Length} characters");
                
                // Parse the different format from Toptal's service
                var feedData = JsonSerializer.Deserialize<ToptalFeedResponse>(response);
                
                if (feedData?.Items != null)
                {
                    Console.WriteLine($"✅ Parsed {feedData.Items.Count} items from fallback service");
                    var limitedItems = feedData.Items.Take(count).ToList();
                    return ProcessToptalArticles(limitedItems);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Fallback service exception: {ex.Message}");
            }
            
            return null;
        }

        private static List<NewsArticle> ProcessArticles(List<RssItem> items)
        {
            var articles = new List<NewsArticle>();
            
            foreach (var item in items)
            {
                var article = new NewsArticle
                {
                    Title = item.Title ?? "",
                    Description = CleanDescription(item.Description ?? ""),
                    Link = item.Link ?? "",
                    PublishDate = ParseDate(item.PubDate),
                    Author = item.Author ?? "TechSpot",
                    Thumbnail = ExtractThumbnail(item),
                    Categories = ExtractCategories(item.Categories)
                };
                
                articles.Add(article);
            }
            
            return articles.OrderByDescending(a => a.PublishDate).ToList();
        }

        private static List<NewsArticle> ProcessToptalArticles(List<ToptalItem> items)
        {
            var articles = new List<NewsArticle>();
            
            foreach (var item in items)
            {
                var article = new NewsArticle
                {
                    Title = item.Title ?? "",
                    Description = CleanDescription(item.Summary ?? item.ContentText ?? ""),
                    Link = item.Url ?? "",
                    PublishDate = ParseToptalDate(item.DatePublished),
                    Author = "TechSpot",
                    Thumbnail = "", // Toptal service might not provide thumbnails
                    Categories = new List<string> { "Technology" }
                };
                
                articles.Add(article);
            }
            
            return articles.OrderByDescending(a => a.PublishDate).ToList();
        }

        public async Task<List<NewsArticle>?> GetLatestNewsAsync(int count = 5)
        {
            try
            {
                Console.WriteLine($"Getting latest news with count: {count}");
                var result = await GetTechSpotNewsAsync(count);
                Console.WriteLine($"Latest news result: {result?.Count ?? 0} articles");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetLatestNewsAsync: {ex.Message}");
                return null;
            }
        }

        private static string CleanDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                return "";

            // Remove HTML tags for display
            var cleaned = System.Text.RegularExpressions.Regex.Replace(description, "<.*?>", string.Empty);
            
            // Decode HTML entities
            cleaned = System.Net.WebUtility.HtmlDecode(cleaned);
            
            // Limit length for display
            if (cleaned.Length > 200)
            {
                cleaned = cleaned.Substring(0, 200) + "...";
            }
            
            return cleaned.Trim();
        }

        private static DateTime ParseDate(string? dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString))
                return DateTime.Now;

            try
            {
                // Try parsing different date formats
                if (DateTime.TryParse(dateString, out var date))
                    return date;
                
                // Try RFC 2822 format (common in RSS)
                if (DateTime.TryParseExact(dateString, "ddd, dd MMM yyyy HH:mm:ss zzz", 
                    System.Globalization.CultureInfo.InvariantCulture, 
                    System.Globalization.DateTimeStyles.None, out var rfc2822Date))
                    return rfc2822Date;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing date '{dateString}': {ex.Message}");
            }

            return DateTime.Now;
        }

        private static DateTime ParseToptalDate(string? dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString))
                return DateTime.Now;

            try
            {
                if (DateTime.TryParse(dateString, out var date))
                    return date;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing Toptal date '{dateString}': {ex.Message}");
            }

            return DateTime.Now;
        }

        private static List<string> ExtractCategories(List<string>? categories)
        {
            if (categories == null || !categories.Any())
                return new List<string> { "Technology" };

            return categories.Take(3).ToList(); // Limit to 3 categories
        }

        private static string ExtractThumbnail(RssItem item)
        {
            // Try multiple sources for thumbnail
            if (!string.IsNullOrEmpty(item.Thumbnail))
                return item.Thumbnail;

            // Try to extract image from description/content
            if (!string.IsNullOrEmpty(item.Description))
            {
                var imgMatch = System.Text.RegularExpressions.Regex.Match(
                    item.Description, 
                    @"<img[^>]+src=[""']([^""']+)[""'][^>]*>", 
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                
                if (imgMatch.Success)
                    return imgMatch.Groups[1].Value;
            }

            if (!string.IsNullOrEmpty(item.Content))
            {
                var imgMatch = System.Text.RegularExpressions.Regex.Match(
                    item.Content, 
                    @"<img[^>]+src=[""']([^""']+)[""'][^>]*>", 
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                
                if (imgMatch.Success)
                    return imgMatch.Groups[1].Value;
            }

            // Try enclosure if it exists
            if (item.Enclosure != null)
            {
                try
                {
                    var enclosureJson = JsonSerializer.Serialize(item.Enclosure);
                    var enclosureObj = JsonSerializer.Deserialize<Dictionary<string, object>>(enclosureJson);
                    
                    if (enclosureObj != null && enclosureObj.ContainsKey("url"))
                    {
                        var url = enclosureObj["url"]?.ToString();
                        if (!string.IsNullOrEmpty(url) && IsImageUrl(url))
                            return url;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing enclosure: {ex.Message}");
                }
            }

            return ""; // No thumbnail found
        }

        private static bool IsImageUrl(string url)
        {
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg" };
            return imageExtensions.Any(ext => url.ToLower().Contains(ext));
        }

        public static string GetCategoryIcon(string category)
        {
            return category.ToLower() switch
            {
                var c when c.Contains("gaming") || c.Contains("game") => "🎮",
                var c when c.Contains("hardware") => "🔧",
                var c when c.Contains("software") => "💻",
                var c when c.Contains("mobile") || c.Contains("phone") => "📱",
                var c when c.Contains("security") => "🔒",
                var c when c.Contains("ai") || c.Contains("artificial") => "🤖",
                var c when c.Contains("review") => "⭐",
                var c when c.Contains("deal") => "💰",
                _ => "🔧"
            };
        }

        public static string GetTimeAgo(DateTime publishDate)
        {
            var timeSpan = DateTime.Now - publishDate;

            if (timeSpan.Days > 0)
                return $"{timeSpan.Days} day{(timeSpan.Days > 1 ? "s" : "")} ago";
            
            if (timeSpan.Hours > 0)
                return $"{timeSpan.Hours} hour{(timeSpan.Hours > 1 ? "s" : "")} ago";
            
            if (timeSpan.Minutes > 0)
                return $"{timeSpan.Minutes} minute{(timeSpan.Minutes > 1 ? "s" : "")} ago";
            
            return "Just now";
        }
    }

    // Models for RSS2JSON API response
    public class Rss2JsonResponse
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }
        
        [JsonPropertyName("message")]
        public string? Message { get; set; }
        
        [JsonPropertyName("feed")]
        public FeedInfo? Feed { get; set; }
        
        [JsonPropertyName("items")]
        public List<RssItem>? Items { get; set; }
    }

    public class FeedInfo
    {
        [JsonPropertyName("url")]
        public string? Url { get; set; }
        
        [JsonPropertyName("title")]
        public string? Title { get; set; }
        
        [JsonPropertyName("link")]
        public string? Link { get; set; }
        
        [JsonPropertyName("author")]
        public string? Author { get; set; }
        
        [JsonPropertyName("description")]
        public string? Description { get; set; }
        
        [JsonPropertyName("image")]
        public string? Image { get; set; }
    }

    public class RssItem
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }
        
        [JsonPropertyName("pubDate")]
        public string? PubDate { get; set; }
        
        [JsonPropertyName("link")]
        public string? Link { get; set; }
        
        [JsonPropertyName("guid")]
        public string? Guid { get; set; }
        
        [JsonPropertyName("author")]
        public string? Author { get; set; }
        
        [JsonPropertyName("thumbnail")]
        public string? Thumbnail { get; set; }
        
        [JsonPropertyName("description")]
        public string? Description { get; set; }
        
        [JsonPropertyName("content")]
        public string? Content { get; set; }
        
        [JsonPropertyName("enclosure")]
        public object? Enclosure { get; set; }
        
        [JsonPropertyName("categories")]
        public List<string>? Categories { get; set; }
    }

    // Models for Toptal fallback service
    public class ToptalFeedResponse
    {
        [JsonPropertyName("items")]
        public List<ToptalItem>? Items { get; set; }
    }

    public class ToptalItem
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }
        
        [JsonPropertyName("url")]
        public string? Url { get; set; }
        
        [JsonPropertyName("summary")]
        public string? Summary { get; set; }
        
        [JsonPropertyName("content_text")]
        public string? ContentText { get; set; }
        
        [JsonPropertyName("date_published")]
        public string? DatePublished { get; set; }
    }

    // Models for our application
    public class NewsArticle
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Link { get; set; } = "";
        public DateTime PublishDate { get; set; }
        public string Author { get; set; } = "";
        public string Thumbnail { get; set; } = "";
        public List<string> Categories { get; set; } = new();
    }
}