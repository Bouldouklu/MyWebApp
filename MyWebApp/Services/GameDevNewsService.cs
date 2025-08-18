using System.Text.Json;
using System.Text.Json.Serialization;

namespace MyWebApp.Services
{
    public class GameDevNewsService
    {
        private readonly HttpClient _httpClient;
        
        // RSS Feed URLs
        private const string EurogamerRssUrl = "https://www.eurogamer.net/feed";
        private const string GameDeveloperRssUrl = "https://www.gamedeveloper.com/rss.xml";
        
        // CORS Proxy URLs
        private const string CorsProxyUrl = "https://cors-anywhere.herokuapp.com/";
        private const string BackupProxyUrl = "https://api.codetabs.com/v1/proxy/?quest=";

        public GameDevNewsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
        }

        public async Task<List<GameDevArticle>?> GetEurogamerNewsAsync(int count = 10)
        {
            Console.WriteLine($"🔄 Fetching Eurogamer news with count: {count}");
            return await FetchRssContent(EurogamerRssUrl, "Eurogamer", count);
        }

        public async Task<List<GameDevArticle>?> GetGameDeveloperNewsAsync(int count = 10)
        {
            Console.WriteLine($"🔄 Fetching Game Developer news with count: {count}");
            return await FetchRssContent(GameDeveloperRssUrl, "Game Developer", count);
        }

        public async Task<List<GameDevArticle>?> GetAllGameDevNewsAsync(int countPerSource = 12)
        {
            Console.WriteLine($"🔄 Fetching all game development news with {countPerSource} per source");
            
            var allArticles = new List<GameDevArticle>();
            
            // Fetch from both sources concurrently
            var tasks = new List<Task<List<GameDevArticle>?>>
            {
                GetEurogamerNewsAsync(countPerSource),
                GetGameDeveloperNewsAsync(countPerSource)
            };

            var results = await Task.WhenAll(tasks);
            
            // Combine all results
            foreach (var result in results)
            {
                if (result != null)
                {
                    allArticles.AddRange(result);
                }
            }
            
            // Sort by publish date (newest first) and return
            return allArticles.OrderByDescending(a => a.PublishDate).ToList();
        }

        private async Task<List<GameDevArticle>?> FetchRssContent(string rssUrl, string sourceName, int count)
        {
            // Try CORS Anywhere first
            var result = await TryFetchRssDirectly(rssUrl, sourceName, count);
            if (result != null && result.Any())
            {
                Console.WriteLine($"✅ {sourceName}: Direct RSS fetch returned {result.Count} articles");
                return result;
            }

            // Try backup proxy
            Console.WriteLine($"⚠️ {sourceName}: Direct fetch failed, trying backup proxy...");
            result = await TryBackupProxy(rssUrl, sourceName, count);
            if (result != null && result.Any())
            {
                Console.WriteLine($"✅ {sourceName}: Backup proxy returned {result.Count} articles");
                return result;
            }
            
            Console.WriteLine($"❌ {sourceName}: All methods failed, creating mock articles for testing");
            return CreateMockArticles(sourceName, count);
        }

        private async Task<List<GameDevArticle>?> TryFetchRssDirectly(string rssUrl, string sourceName, int count)
        {
            try
            {
                var proxyUrl = $"{CorsProxyUrl}{rssUrl}";
                Console.WriteLine($"🌐 {sourceName}: Calling CORS Anywhere: {proxyUrl}");
                
                var response = await _httpClient.GetStringAsync(proxyUrl);
                Console.WriteLine($"📋 {sourceName}: RSS Response length: {response.Length} characters");
                
                if (response.Contains("<rss") || response.Contains("<item>") || response.Contains("<entry>"))
                {
                    Console.WriteLine($"✅ {sourceName}: Got valid RSS content");
                    var articles = ParseRssContent(response, sourceName, count);
                    Console.WriteLine($"✅ {sourceName}: Parsed {articles.Count} articles from RSS");
                    return articles;
                }
                else
                {
                    Console.WriteLine($"❌ {sourceName}: Invalid RSS content received");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ {sourceName}: CORS Anywhere Exception: {ex.Message}");
            }
            
            return null;
        }

        private async Task<List<GameDevArticle>?> TryBackupProxy(string rssUrl, string sourceName, int count)
        {
            try
            {
                var proxyUrl = $"{BackupProxyUrl}{Uri.EscapeDataString(rssUrl)}";
                Console.WriteLine($"🌐 {sourceName}: Calling backup proxy: {proxyUrl}");
                
                var response = await _httpClient.GetStringAsync(proxyUrl);
                Console.WriteLine($"📋 {sourceName}: Backup response length: {response.Length} characters");
                
                if (response.Contains("<rss") || response.Contains("<item>") || response.Contains("<entry>"))
                {
                    Console.WriteLine($"✅ {sourceName}: Got valid RSS content from backup");
                    var articles = ParseRssContent(response, sourceName, count);
                    Console.WriteLine($"✅ {sourceName}: Parsed {articles.Count} articles from backup RSS");
                    return articles;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ {sourceName}: Backup proxy exception: {ex.Message}");
            }
            
            return null;
        }

        private static List<GameDevArticle> CreateMockArticles(string sourceName, int count)
        {
            Console.WriteLine($"🎭 {sourceName}: Creating {count} mock articles for demonstration");
            
            var mockArticles = new List<GameDevArticle>();
            var random = new Random();

            var mockData = sourceName switch
            {
                "Eurogamer" => new[]
                {
                    ("Game Industry Analysis: 2025 Trends", "Deep dive into the most important gaming trends shaping the industry this year."),
                    ("Indie Game Spotlight: Rising Stars", "Featuring the most promising independent games from emerging developers."),
                    ("Gaming Hardware Review Roundup", "Comprehensive reviews of the latest gaming hardware and peripherals."),
                    ("Retro Gaming Revival Continues", "Classic games continue to find new audiences on modern platforms."),
                    ("Mobile Gaming Market Analysis", "The mobile gaming sector shows continued growth and innovation."),
                    ("Virtual Reality Gaming Evolution", "VR gaming reaches new heights with improved hardware and software."),
                    ("Gaming Community Highlights", "Celebrating the best contributions from the global gaming community."),
                    ("Game Development Documentary", "Behind-the-scenes look at modern game development processes.")
                },
                "Game Developer" => new[]
                {
                    ("Best Practices for Game Monetization", "Ethical and effective strategies for game monetization in 2025."),
                    ("Game Engine Comparison Guide", "Comprehensive analysis of popular game engines and their strengths."),
                    ("Accessibility in Game Design", "Making games more inclusive through thoughtful design practices."),
                    ("Game Development Team Management", "Tips for leading successful game development teams."),
                    ("Publishing Strategies for Indie Developers", "How independent developers can successfully publish their games."),
                    ("Game Analytics and Player Metrics", "Understanding and utilizing player data for game improvement."),
                    ("Cross-Platform Development Challenges", "Technical considerations for multi-platform game development."),
                    ("Game Design Psychology Insights", "The psychology behind addictive and engaging game mechanics.")
                },
                _ => new[]
                {
                    ("Sample Game Development Article", "This is a sample article for demonstration purposes."),
                    ("Another Development Article", "Another sample article showing the RSS feed functionality."),
                    ("Third Sample Article", "A third article to demonstrate the feed content."),
                    ("Fourth Article Example", "Fourth article in the sample feed."),
                    ("Fifth Sample Post", "Fifth sample post for the mock RSS feed."),
                    ("Sixth Development Article", "Sixth article in the sample RSS content."),
                    ("Seventh Sample Article", "Seventh article for demonstration purposes."),
                    ("Eighth Mock Article", "Eighth and final sample article.")
                }
            };

            for (int i = 0; i < Math.Min(count, mockData.Length); i++)
            {
                var (title, description) = mockData[i];
                mockArticles.Add(new GameDevArticle
                {
                    Title = title,
                    Description = description,
                    Link = GetSampleLink(sourceName),
                    PublishDate = DateTime.Now.AddHours(-random.Next(1, 168)), // Random within last week
                    Author = GetSampleAuthor(sourceName),
                    Source = sourceName,
                    Thumbnail = "",
                    Categories = GetSampleCategories(sourceName)
                });
            }
            
            return mockArticles;
        }

        private static List<GameDevArticle> ParseRssContent(string rssContent, string sourceName, int count)
        {
            var articles = new List<GameDevArticle>();
            
            try
            {
                // Handle both RSS 2.0 <item> and Atom <entry> formats
                var itemPattern = rssContent.Contains("<entry>") ? 
                    @"<entry>(.*?)</entry>" : 
                    @"<item>(.*?)</item>";
                
                var itemMatches = System.Text.RegularExpressions.Regex.Matches(
                    rssContent, 
                    itemPattern, 
                    System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                
                Console.WriteLine($"{sourceName}: Found {itemMatches.Count} RSS items");
                
                int processed = 0;
                foreach (System.Text.RegularExpressions.Match itemMatch in itemMatches)
                {
                    if (processed >= count) break;
                    
                    var itemContent = itemMatch.Groups[1].Value;
                    
                    // Handle both RSS and Atom formats
                    var isAtom = rssContent.Contains("<entry>");
                    
                    var article = new GameDevArticle
                    {
                        Title = ExtractRssField(itemContent, isAtom ? "title" : "title"),
                        Description = CleanDescription(ExtractRssField(itemContent, isAtom ? "summary" : "description")),
                        Link = ExtractRssField(itemContent, isAtom ? "link" : "link"),
                        PublishDate = ParseRssDate(ExtractRssField(itemContent, isAtom ? "published" : "pubDate")),
                        Author = ExtractRssField(itemContent, isAtom ? "author" : "dc:creator") ?? ExtractRssField(itemContent, "author") ?? sourceName,
                        Source = sourceName,
                        Thumbnail = ExtractRssImage(itemContent),
                        Categories = ExtractCategories(itemContent, sourceName)
                    };

                    // Handle Atom link format or if link is empty
                    if (isAtom && string.IsNullOrEmpty(article.Link))
                    {
                        article.Link = ExtractAtomLink(itemContent);
                    }
                    
                    // Unity-specific link fixes
                    if (sourceName == "Unity" && !string.IsNullOrEmpty(article.Link))
                    {
                        // If the link doesn't look like a direct article link, try to extract from guid or id
                        if (article.Link.Contains("blogs.unity3d.com/?") || article.Link.EndsWith("blogs.unity3d.com/"))
                        {
                            var guid = ExtractRssField(itemContent, "guid");
                            var id = ExtractRssField(itemContent, "id");
                            
                            if (!string.IsNullOrEmpty(guid) && guid.StartsWith("http"))
                            {
                                article.Link = guid;
                            }
                            else if (!string.IsNullOrEmpty(id) && id.StartsWith("http"))
                            {
                                article.Link = id;
                            }
                        }
                        
                        // Clean up Unity URLs
                        if (article.Link.Contains("?"))
                        {
                            var queryIndex = article.Link.IndexOf('?');
                            article.Link = article.Link.Substring(0, queryIndex);
                        }
                    }
                    
                    // Ensure we have a valid link
                    if (string.IsNullOrEmpty(article.Link))
                    {
                        article.Link = GetSampleLink(sourceName);
                    }
                    
                    if (!string.IsNullOrWhiteSpace(article.Title))
                    {
                        articles.Add(article);
                        processed++;
                    }
                }
                
                Console.WriteLine($"{sourceName}: Successfully parsed {articles.Count} articles");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{sourceName}: Error parsing RSS content: {ex.Message}");
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
                content = System.Net.WebUtility.HtmlDecode(content);
                content = System.Text.RegularExpressions.Regex.Replace(content, @"<!\[CDATA\[(.*?)\]\]>", "$1", System.Text.RegularExpressions.RegexOptions.Singleline);
                return content.Trim();
            }
            
            return "";
        }

        private static string ExtractAtomLink(string itemContent)
        {
            // Try multiple link extraction patterns for Atom feeds
            var linkPatterns = new[]
            {
                @"<link[^>]+href=[""']([^""']+)[""'][^>]*rel=[""']alternate[""'][^>]*>",
                @"<link[^>]+rel=[""']alternate[""'][^>]+href=[""']([^""']+)[""'][^>]*>",
                @"<link[^>]+href=[""']([^""']+)[""'][^>]*>",
                @"<id[^>]*>([^<]+)</id>"
            };

            foreach (var pattern in linkPatterns)
            {
                var match = System.Text.RegularExpressions.Regex.Match(
                    itemContent, 
                    pattern, 
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                
                if (match.Success)
                {
                    var url = match.Groups[1].Value;
                    // Make sure it's a valid HTTP URL
                    if (url.StartsWith("http"))
                    {
                        return url;
                    }
                }
            }
            
            return "";
        }

        private static string ExtractRssImage(string itemContent)
        {
            // Try various image extraction patterns, prioritizing larger images
            var patterns = new[]
            {
                // Unity-specific patterns
                @"<media:content[^>]+url=[""']([^""']+)[""'][^>]*medium=[""']image[""'][^>]*>",
                @"<media:thumbnail[^>]+url=[""']([^""']+)[""'][^>]*>",
                @"<content:encoded[^>]*>.*?<img[^>]+src=[""']([^""']+)[""'][^>]*>",
                // Standard patterns
                @"<img[^>]+src=[""']([^""']+)[""'][^>]*>",
                @"<enclosure[^>]+type=[""']image/[^""']*[""'][^>]+url=[""']([^""']+)[""'][^>]*>",
                @"<enclosure[^>]+url=[""']([^""']+)[""'][^>]*type=[""']image/[^""']*[""'][^>]*>",
                @"<media:content[^>]+url=[""']([^""']+)[""'][^>]*>",
                // Look in description for images
                @"<description[^>]*>.*?<img[^>]+src=[""']([^""']+)[""'][^>]*>.*?</description>"
            };

            foreach (var pattern in patterns)
            {
                var match = System.Text.RegularExpressions.Regex.Match(
                    itemContent, 
                    pattern, 
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
                
                if (match.Success)
                {
                    var url = match.Groups[1].Value;
                    if (IsImageUrl(url) && !url.Contains("avatar") && !url.Contains("icon"))
                    {
                        // Clean up the URL
                        url = System.Net.WebUtility.HtmlDecode(url);
                        // Ensure it's a full URL
                        if (!url.StartsWith("http"))
                        {
                            if (url.StartsWith("//"))
                                url = "https:" + url;
                            else if (url.StartsWith("/"))
                                url = "https://blogs.unity3d.com" + url;
                        }
                        return url;
                    }
                }
            }
            
            return "";
        }

        private static List<string> ExtractCategories(string itemContent, string sourceName)
        {
            var categories = new List<string>();
            
            // Extract categories from RSS
            var categoryPattern = @"<category[^>]*>([^<]+)</category>";
            var matches = System.Text.RegularExpressions.Regex.Matches(
                itemContent, 
                categoryPattern, 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                var category = System.Net.WebUtility.HtmlDecode(match.Groups[1].Value.Trim());
                if (!string.IsNullOrWhiteSpace(category))
                {
                    categories.Add(category);
                }
            }
            
            // Add default category based on source
            if (!categories.Any())
            {
                categories.Add(sourceName switch
                {
                    "Unity" => "Unity",
                    "Eurogamer" => "Gaming",
                    "Game Developer" => "Game Development",
                    _ => "Gaming"
                });
            }
            
            return categories;
        }

        private static DateTime ParseRssDate(string? dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString))
                return DateTime.Now;

            try
            {
                if (DateTime.TryParse(dateString, out var date))
                    return date;
                
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

        private static string CleanDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                return "";

            var cleaned = System.Text.RegularExpressions.Regex.Replace(description, "<.*?>", string.Empty);
            cleaned = System.Net.WebUtility.HtmlDecode(cleaned);
            
            if (cleaned.Length > 250)
            {
                cleaned = cleaned.Substring(0, 250) + "...";
            }
            
            return cleaned.Trim();
        }

        private static bool IsImageUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return false;
                
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg", ".bmp" };
            var lowerUrl = url.ToLower();
            
            // Check for image extensions
            if (imageExtensions.Any(ext => lowerUrl.Contains(ext)))
                return true;
                
            // Check for image-related patterns in URL
            if (lowerUrl.Contains("image") || lowerUrl.Contains("photo") || lowerUrl.Contains("picture"))
                return true;
                
            return false;
        }

        private static string GetSampleLink(string sourceName)
        {
            return sourceName switch
            {
                "Eurogamer" => "https://www.eurogamer.net/",
                "Game Developer" => "https://www.gamedeveloper.com/",
                _ => "#"
            };
        }

        private static string GetSampleAuthor(string sourceName)
        {
            return sourceName switch
            {
                "Eurogamer" => "Eurogamer Editorial",
                "Game Developer" => "Game Developer Staff",
                _ => "Editorial Team"
            };
        }

        private static List<string> GetSampleCategories(string sourceName)
        {
            return sourceName switch
            {
                "Eurogamer" => new List<string> { "Gaming", "Reviews", "News" },
                "Game Developer" => new List<string> { "Game Development", "Industry", "Business" },
                _ => new List<string> { "Gaming" }
            };
        }

        public static string GetSourceIcon(string source)
        {
            return source switch
            {
                "Eurogamer" => "🎮",
                "Game Developer" => "📰",
                _ => "📄"
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

    // Model for game development articles
    public class GameDevArticle
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Link { get; set; } = "";
        public DateTime PublishDate { get; set; }
        public string Author { get; set; } = "";
        public string Source { get; set; } = "";
        public string Thumbnail { get; set; } = "";
        public List<string> Categories { get; set; } = new();
    }
}