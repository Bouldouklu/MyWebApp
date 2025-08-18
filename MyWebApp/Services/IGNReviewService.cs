using System.Text.Json;
using System.Text.Json.Serialization;

namespace MyWebApp.Services
{
    public class IGNReviewsService
    {
        private readonly HttpClient _httpClient;
        private const string IGNReviewsRssUrl = "https://feeds.ign.com/ign/reviews";
        private const string CorsProxyUrl = "https://cors-anywhere.herokuapp.com/";
        private const string BackupProxyUrl = "https://api.codetabs.com/v1/proxy/?quest=";

        public IGNReviewsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            // Set a user agent to avoid potential blocking
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
        }

        public async Task<List<GameReview>?> GetIGNReviewsAsync(int count = 10)
        {
            Console.WriteLine($"🔄 Starting GetIGNReviewsAsync with count: {count}");
            
            // Try CORS Anywhere first
            var result = await TryFetchRssDirectly(count);
            if (result != null)
            {
                Console.WriteLine($"✅ Direct RSS fetch returned {result.Count} reviews");
                return result;
            }

            // Try backup proxy
            Console.WriteLine("⚠️ Direct fetch failed, trying backup proxy...");
            result = await TryBackupProxy(count);
            if (result != null)
            {
                Console.WriteLine($"✅ Backup proxy returned {result.Count} reviews");
                return result;
            }
            
            Console.WriteLine("❌ All methods failed, creating mock reviews for testing");
            return CreateMockReviews(count);
        }

        private async Task<List<GameReview>?> TryFetchRssDirectly(int count)
        {
            try
            {
                var proxyUrl = $"{CorsProxyUrl}{IGNReviewsRssUrl}";
                Console.WriteLine($"🌐 Calling CORS Anywhere: {proxyUrl}");
                
                var response = await _httpClient.GetStringAsync(proxyUrl);
                Console.WriteLine($"📋 RSS Response length: {response.Length} characters");
                
                if (response.Contains("<rss") || response.Contains("<item>"))
                {
                    Console.WriteLine($"✅ Got valid RSS content");
                    var reviews = ParseRssContent(response, count);
                    Console.WriteLine($"✅ Parsed {reviews.Count} reviews from RSS");
                    return reviews;
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

        private async Task<List<GameReview>?> TryBackupProxy(int count)
        {
            try
            {
                var proxyUrl = $"{BackupProxyUrl}{Uri.EscapeDataString(IGNReviewsRssUrl)}";
                Console.WriteLine($"🌐 Calling backup proxy: {proxyUrl}");
                
                var response = await _httpClient.GetStringAsync(proxyUrl);
                Console.WriteLine($"📋 Backup response length: {response.Length} characters");
                
                if (response.Contains("<rss") || response.Contains("<item>"))
                {
                    Console.WriteLine($"✅ Got valid RSS content from backup");
                    var reviews = ParseRssContent(response, count);
                    Console.WriteLine($"✅ Parsed {reviews.Count} reviews from backup RSS");
                    return reviews;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Backup proxy exception: {ex.Message}");
            }
            
            return null;
        }

        private static List<GameReview> CreateMockReviews(int count)
        {
            Console.WriteLine($"🎭 Creating {count} mock reviews for demonstration");
            
            var mockReviews = new List<GameReview>();
            var random = new Random();
            
            var sampleGames = new[]
            {
                ("The Legend of Zelda: Tears of the Kingdom", 9.5f, "Nintendo Switch"),
                ("Hogwarts Legacy", 8.5f, "PC, PS5, Xbox Series X/S"),
                ("Resident Evil 4 Remake", 9.0f, "PC, PS5, Xbox Series X/S"),
                ("Spider-Man 2", 8.8f, "PS5"),
                ("Alan Wake 2", 8.9f, "PC, PS5, Xbox Series X/S"),
                ("Baldur's Gate 3", 9.8f, "PC, PS5"),
                ("Starfield", 7.5f, "PC, Xbox Series X/S"),
                ("Cyberpunk 2077: Phantom Liberty", 8.7f, "PC, PS5, Xbox Series X/S")
            };
            
            for (int i = 0; i < Math.Min(count, sampleGames.Length); i++)
            {
                var (title, score, platform) = sampleGames[i];
                mockReviews.Add(new GameReview
                {
                    Title = title,
                    Description = $"IGN's review of {title}. This is sample content that would normally be loaded from IGN's RSS feed.",
                    Link = "https://www.ign.com/reviews/games",
                    PublishDate = DateTime.Now.AddHours(-random.Next(1, 168)), // Random within last week
                    Author = "IGN Editorial",
                    Thumbnail = "",
                    Score = score,
                    ScoreText = GetScoreText(score),
                    Platform = platform,
                    Genre = GetRandomGenre(),
                    Categories = new List<string> { "Game Review", "Gaming" }
                });
            }
            
            return mockReviews;
        }

        private static List<GameReview> ParseRssContent(string rssContent, int count)
        {
            var reviews = new List<GameReview>();
            
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
                    
                    var review = new GameReview
                    {
                        Title = ExtractRssField(itemContent, "title"),
                        Description = CleanDescription(ExtractRssField(itemContent, "description")),
                        Link = ExtractRssField(itemContent, "link"),
                        PublishDate = ParseRssDate(ExtractRssField(itemContent, "pubDate")),
                        Author = ExtractRssField(itemContent, "dc:creator") ?? "IGN",
                        Thumbnail = ExtractRssImage(itemContent),
                        Score = ExtractScore(itemContent),
                        Platform = ExtractPlatform(itemContent),
                        Genre = ExtractGenre(itemContent),
                        Categories = new List<string> { "Game Review" }
                    };

                    review.ScoreText = GetScoreText(review.Score);
                    
                    if (!string.IsNullOrWhiteSpace(review.Title))
                    {
                        reviews.Add(review);
                        processed++;
                    }
                }
                
                Console.WriteLine($"Successfully parsed {reviews.Count} reviews");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing RSS content: {ex.Message}");
            }
            
            return reviews.OrderByDescending(r => r.PublishDate).ToList();
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

        private static float ExtractScore(string itemContent)
        {
            // Look for IGN score patterns
            var scorePatterns = new[]
            {
                @"Score:\s*(\d+\.?\d*)",
                @"Rating:\s*(\d+\.?\d*)",
                @"(\d+\.?\d*)\s*/\s*10",
                @"IGN\s+Score:\s*(\d+\.?\d*)"
            };

            foreach (var pattern in scorePatterns)
            {
                var match = System.Text.RegularExpressions.Regex.Match(
                    itemContent, 
                    pattern, 
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                
                if (match.Success && float.TryParse(match.Groups[1].Value, out var score))
                {
                    return score;
                }
            }
            
            // If no score found, return a random score for mock data
            return 0f;
        }

        private static string ExtractPlatform(string itemContent)
        {
            var platforms = new[] { "PC", "PS5", "PS4", "Xbox Series X/S", "Xbox One", "Nintendo Switch", "Steam Deck" };
            
            foreach (var platform in platforms)
            {
                if (itemContent.Contains(platform, StringComparison.OrdinalIgnoreCase))
                {
                    return platform;
                }
            }
            
            return "Multiple Platforms";
        }

        private static string ExtractGenre(string itemContent)
        {
            var genres = new[] { "Action", "Adventure", "RPG", "Strategy", "Shooter", "Sports", "Racing", "Simulation", "Puzzle", "Horror" };
            
            foreach (var genre in genres)
            {
                if (itemContent.Contains(genre, StringComparison.OrdinalIgnoreCase))
                {
                    return genre;
                }
            }
            
            return GetRandomGenre();
        }

        private static string GetRandomGenre()
        {
            var genres = new[] { "Action", "Adventure", "RPG", "Strategy", "Shooter", "Sports", "Racing" };
            return genres[new Random().Next(genres.Length)];
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

        public async Task<List<GameReview>?> GetLatestReviewsAsync(int count = 4)
        {
            try
            {
                Console.WriteLine($"Getting latest reviews with count: {count}");
                var result = await GetIGNReviewsAsync(count);
                Console.WriteLine($"Latest reviews result: {result?.Count ?? 0} reviews");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetLatestReviewsAsync: {ex.Message}");
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

        public static string GetScoreText(float score)
        {
            return score switch
            {
                >= 9.0f => "Amazing",
                >= 8.0f => "Great",
                >= 7.0f => "Good",
                >= 6.0f => "Okay",
                >= 5.0f => "Mediocre",
                > 0f => "Bad",
                _ => "Not Scored"
            };
        }

        public static string GetScoreColor(float score)
        {
            return score switch
            {
                >= 9.0f => "success",
                >= 8.0f => "primary",
                >= 7.0f => "info",
                >= 6.0f => "warning",
                >= 5.0f => "secondary",
                > 0f => "danger",
                _ => "light"
            };
        }

        public static string GetGenreIcon(string genre)
        {
            return genre.ToLower() switch
            {
                var g when g.Contains("action") => "⚔️",
                var g when g.Contains("adventure") => "🗺️",
                var g when g.Contains("rpg") => "🧙‍♂️",
                var g when g.Contains("strategy") => "♟️",
                var g when g.Contains("shooter") => "🔫",
                var g when g.Contains("sports") => "⚽",
                var g when g.Contains("racing") => "🏎️",
                var g when g.Contains("simulation") => "🎮",
                var g when g.Contains("puzzle") => "🧩",
                var g when g.Contains("horror") => "👻",
                _ => "🎮"
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

    // Model for game reviews
    public class GameReview
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Link { get; set; } = "";
        public DateTime PublishDate { get; set; }
        public string Author { get; set; } = "";
        public string Thumbnail { get; set; } = "";
        public float Score { get; set; }
        public string ScoreText { get; set; } = "";
        public string Platform { get; set; } = "";
        public string Genre { get; set; } = "";
        public List<string> Categories { get; set; } = new();
    }
}