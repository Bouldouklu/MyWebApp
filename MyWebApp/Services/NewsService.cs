using System.Text.Json;
using System.Text.Json.Serialization;

namespace MyWebApp.Services
{
    public class NewsService
    {
        private readonly HttpClient _httpClient;
        private const string TechSpotRssUrl = "https://www.techspot.com/backend.xml";
        private const string CorsProxyUrl = "https://cors-anywhere.herokuapp.com/";
        private const string BackupProxyUrl = "https://api.codetabs.com/v1/proxy/?quest=";

        public NewsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            // Set a user agent to avoid potential blocking
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
        }

        public async Task<List<NewsArticle>?> GetTechSpotNewsAsync(int count = 10)
        {
            Console.WriteLine($"🔄 Starting GetTechSpotNewsAsync with count: {count}");
            
            // Try CORS Anywhere first
            var result = await TryFetchRssDirectly(count);
            if (result != null)
            {
                Console.WriteLine($"✅ Direct RSS fetch returned {result.Count} articles");
                return result;
            }

            // Try backup proxy
            Console.WriteLine("⚠️ Direct fetch failed, trying backup proxy...");
            result = await TryBackupProxy(count);
            if (result != null)
            {
                Console.WriteLine($"✅ Backup proxy returned {result.Count} articles");
                return result;
            }
            
            Console.WriteLine("❌ All methods failed, creating mock articles for testing");
            return CreateMockArticles(count);
        }

        private async Task<List<NewsArticle>?> TryFetchRssDirectly(int count)
        {
            try
            {
                var proxyUrl = $"{CorsProxyUrl}{TechSpotRssUrl}";
                Console.WriteLine($"🌐 Calling CORS Anywhere: {proxyUrl}");
                
                var response = await _httpClient.GetStringAsync(proxyUrl);
                Console.WriteLine($"📋 RSS Response length: {response.Length} characters");
                
                if (response.Contains("<rss") || response.Contains("<item>"))
                {
                    Console.WriteLine($"✅ Got valid RSS content");
                    var articles = ParseRssContent(response, count);
                    Console.WriteLine($"✅ Parsed {articles.Count} articles from RSS");
                    return articles;
                }
                else
                {
                    Console.WriteLine($"❌ Invalid RSS content received");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ CORS Anywhere Exception: {ex.Message}");
            }
            
            return null;
        }

        private async Task<List<NewsArticle>?> TryBackupProxy(int count)
        {
            try
            {
                var proxyUrl = $"{BackupProxyUrl}{Uri.EscapeDataString(TechSpotRssUrl)}";
                Console.WriteLine($"🌐 Calling backup proxy: {proxyUrl}");
                
                var response = await _httpClient.GetStringAsync(proxyUrl);
                Console.WriteLine($"📋 Backup response length: {response.Length} characters");
                
                if (response.Contains("<rss") || response.Contains("<item>"))
                {
                    Console.WriteLine($"✅ Got valid RSS content from backup");
                    var articles = ParseRssContent(response, count);
                    Console.WriteLine($"✅ Parsed {articles.Count} articles from backup RSS");
                    return articles;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Backup proxy exception: {ex.Message}");
            }
            
            return null;
        }

        private static List<NewsArticle> CreateMockArticles(int count)
        {
            Console.WriteLine($"🎭 Creating {count} mock articles for demonstration");
            
            var mockArticles = new List<NewsArticle>();
            var random = new Random();
            
            var sampleTitles = new[]
            {
                "Latest GPU Benchmarks Show Performance Gains",
                "New AI Breakthrough in Machine Learning",
                "Cybersecurity Alert: Critical Vulnerability Found",
                "Gaming Performance Analysis: RTX vs Radeon",
                "Mobile Technology Advances in 2025",
                "Cloud Computing Trends for Enterprises",
                "Open Source Software Development Updates",
                "Hardware Review: Latest SSD Performance"
            };
            
            for (int i = 0; i < Math.Min(count, sampleTitles.Length); i++)
            {
                mockArticles.Add(new NewsArticle
                {
                    Title = sampleTitles[i],
                    Description = $"This is a sample article about {sampleTitles[i].ToLower()}. Content would normally be loaded from TechSpot RSS feed.",
                    Link = "https://www.techspot.com",
                    PublishDate = DateTime.Now.AddHours(-random.Next(1, 24)),
                    Author = "TechSpot",
                    Thumbnail = "",
                    Categories = new List<string> { "Technology", "Demo" }
                });
            }
            
            return mockArticles;
        }

        private static List<NewsArticle> ParseRssContent(string rssContent, int count)
        {
            var articles = new List<NewsArticle>();
            
            try
            {
                // Simple RSS parsing - look for <item> tags
                var itemMatches = System.Text.RegularExpressions.Regex.Matches(
                    rssContent, 
                    @"<item>(.*?)</item>", 
                    System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                
                Console.WriteLine($"Found {itemMatches.Count} RSS items");
                
                int processed = 0;
                foreach (System.Text.RegularExpressions.Match itemMatch in itemMatches)
                {
                    if (processed >= count) break;
                    
                    var itemContent = itemMatch.Groups[1].Value;
                    
                    var article = new NewsArticle
                    {
                        Title = ExtractRssField(itemContent, "title"),
                        Description = CleanDescription(ExtractRssField(itemContent, "description")),
                        Link = ExtractRssField(itemContent, "link"),
                        PublishDate = ParseRssDate(ExtractRssField(itemContent, "pubDate")),
                        Author = "TechSpot",
                        Thumbnail = ExtractRssImage(itemContent),
                        Categories = new List<string> { "Technology" }
                    };
                    
                    if (!string.IsNullOrWhiteSpace(article.Title))
                    {
                        articles.Add(article);
                        processed++;
                    }
                }
                
                Console.WriteLine($"Successfully parsed {articles.Count} articles");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing RSS content: {ex.Message}");
            }
            
            return articles.OrderByDescending(a => a.PublishDate).ToList();
        }

        private static string ExtractRssField(string itemContent, string fieldName)
        {
            var pattern = $@"<{fieldName}[^>]*>(.*?)</{fieldName}>";
            var match = System.Text.RegularExpressions.Regex.Match(
                itemContent, 
                pattern, 
                System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            
            if (match.Success)
            {
                var content = match.Groups[1].Value;
                // Decode HTML entities and remove CDATA
                content = System.Net.WebUtility.HtmlDecode(content);
                content = System.Text.RegularExpressions.Regex.Replace(content, @"<!\[CDATA\[(.*?)\]\]>", "$1", System.Text.RegularExpressions.RegexOptions.Singleline);
                return content.Trim();
            }
            
            return "";
        }

        private static string ExtractRssImage(string itemContent)
        {
            // Try to find images in the description or content
            var imgPattern = @"<img[^>]+src=[""']([^""']+)[""'][^>]*>";
            var match = System.Text.RegularExpressions.Regex.Match(
                itemContent, 
                imgPattern, 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            
            // Try to find enclosure URL
            var enclosurePattern = @"<enclosure[^>]+url=[""']([^""']+)[""'][^>]*>";
            match = System.Text.RegularExpressions.Regex.Match(
                itemContent, 
                enclosurePattern, 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            
            if (match.Success)
            {
                var url = match.Groups[1].Value;
                if (IsImageUrl(url))
                {
                    return url;
                }
            }
            
            return "";
        }

        private static DateTime ParseRssDate(string? dateString)
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
                Console.WriteLine($"Error parsing RSS date '{dateString}': {ex.Message}");
            }

            return DateTime.Now;
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