using System.Text.Json;
using System.Text.Json.Serialization;

namespace MyWebApp.Services
{
    public class RugbyCalendarService
    {
        private readonly HttpClient _httpClient;
        
        // API endpoints for live rugby data
        private const string SportsApiUrl = "https://api.sportradar.com/rugby/trial/v3/en"; // Sports Radar API
        private const string BackupApiUrl = "https://rugby-live-data.p.rapidapi.com"; // RapidAPI backup
        private const string FreeApiUrl = "https://api.sportmonks.com/v3/rugby"; // SportMonks
        
        // API Keys (you'll need to register for these free APIs)
        private const string ApiKey = "YOUR_API_KEY_HERE"; // Replace with actual API key
        
        public RugbyCalendarService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "MyWebApp Rugby Calendar v1.0");
        }

        /// <summary>
        /// Gets rugby matches from the last 10 days and next 12 months
        /// </summary>
        public async Task<List<RugbyMatch>> GetDynamicRugbyCalendarAsync()
        {
            var matches = new List<RugbyMatch>();
            
            try
            {
                Console.WriteLine("🏉 Fetching dynamic rugby calendar...");
                
                // Try to fetch from real API first
                var apiMatches = await FetchFromRugbyApiAsync();
                if (apiMatches?.Any() == true)
                {
                    matches.AddRange(apiMatches);
                    Console.WriteLine($"✅ Fetched {apiMatches.Count} matches from live API");
                }
                else
                {
                    Console.WriteLine("⚠️ API unavailable, using dynamic mock data");
                    matches.AddRange(CreateDynamicMockMatches());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error fetching rugby data: {ex.Message}");
                Console.WriteLine("📅 Falling back to dynamic mock data");
                matches.AddRange(CreateDynamicMockMatches());
            }
            
            return FilterMatchesByTimeWindow(matches);
        }

        /// <summary>
        /// Filters matches to show last 10 days + next 12 months
        /// </summary>
        private List<RugbyMatch> FilterMatchesByTimeWindow(List<RugbyMatch> allMatches)
        {
            var now = DateTime.Now;
            var startDate = now.AddDays(-10);
            var endDate = now.AddMonths(12);
            
            return allMatches
                .Where(m => m.DateTime >= startDate && m.DateTime <= endDate)
                .OrderBy(m => m.DateTime)
                .ToList();
        }

        /// <summary>
        /// Attempts to fetch live rugby data from multiple APIs
        /// </summary>
        private async Task<List<RugbyMatch>?> FetchFromRugbyApiAsync()
        {
            // Try SportRadar API first
            try
            {
                var sportRadarMatches = await FetchFromSportRadarAsync();
                if (sportRadarMatches?.Any() == true)
                    return sportRadarMatches;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SportRadar API failed: {ex.Message}");
            }

            // Try backup APIs
            try
            {
                var backupMatches = await FetchFromBackupApiAsync();
                if (backupMatches?.Any() == true)
                    return backupMatches;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Backup API failed: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Fetches from SportRadar API (requires API key)
        /// </summary>
        private async Task<List<RugbyMatch>?> FetchFromSportRadarAsync()
        {
            if (string.IsNullOrEmpty(ApiKey) || ApiKey == "YOUR_API_KEY_HERE")
            {
                Console.WriteLine("📝 No SportRadar API key configured");
                return null;
            }

            var matches = new List<RugbyMatch>();
            var competitions = new[] { "six-nations", "rugby-championship", "world-cup" };

            foreach (var competition in competitions)
            {
                try
                {
                    var url = $"{SportsApiUrl}/competitions/{competition}/fixtures?api_key={ApiKey}";
                    var response = await _httpClient.GetStringAsync(url);
                    var apiData = JsonSerializer.Deserialize<SportRadarResponse>(response);
                    
                    if (apiData?.Fixtures != null)
                    {
                        matches.AddRange(apiData.Fixtures.Select(f => new RugbyMatch
                        {
                            Team1 = f.HomeTeam?.Name ?? "TBD",
                            Team2 = f.AwayTeam?.Name ?? "TBD",
                            Competition = GetCompetitionDisplayName(competition),
                            Venue = f.Venue?.Name ?? "TBD",
                            DateTime = f.StartTime,
                            Score = f.Status == "closed" ? $"{f.HomeScore}-{f.AwayScore}" : "",
                            Description = f.Stage ?? ""
                        }));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching {competition}: {ex.Message}");
                }
            }

            return matches;
        }

        /// <summary>
        /// Fetches from backup rugby APIs
        /// </summary>
        private async Task<List<RugbyMatch>?> FetchFromBackupApiAsync()
        {
            try
            {
                // Try a free rugby API or scraping approach
                // This would need to be implemented based on available free APIs
                Console.WriteLine("🔄 Attempting backup API...");
                
                // Placeholder for backup API implementation
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Backup API error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Creates realistic mock data that updates dynamically based on current date
        /// </summary>
        private List<RugbyMatch> CreateDynamicMockMatches()
        {
            var matches = new List<RugbyMatch>();
            var now = DateTime.Now;
            var currentYear = now.Year;
            
            // Add recent matches (last 10 days) with scores
            matches.AddRange(CreateRecentMatches(now));
            
            // Add upcoming matches based on current date and rugby calendar
            matches.AddRange(CreateUpcomingMatches(now, currentYear));
            
            return matches;
        }

        private List<RugbyMatch> CreateRecentMatches(DateTime now)
        {
            var recentMatches = new List<RugbyMatch>();
            var random = new Random();
            
            // Generate matches for the last 10 days
            for (int i = 1; i <= 10; i++)
            {
                var matchDate = now.AddDays(-i);
                
                // Only add matches if they would realistically happen (weekends mostly)
                if (matchDate.DayOfWeek == DayOfWeek.Saturday || 
                    matchDate.DayOfWeek == DayOfWeek.Sunday ||
                    random.Next(1, 5) == 1) // 25% chance for weekday matches
                {
                    var match = CreateRandomMatch(matchDate, true);
                    if (match != null)
                        recentMatches.Add(match);
                }
            }
            
            return recentMatches;
        }

        private List<RugbyMatch> CreateUpcomingMatches(DateTime now, int currentYear)
        {
            var upcomingMatches = new List<RugbyMatch>();
            
            // Six Nations (February-March)
            if (now.Month <= 3 || now.Month >= 11)
            {
                var sixNationsYear = now.Month >= 11 ? currentYear + 1 : currentYear;
                upcomingMatches.AddRange(GenerateSixNationsFixtures(sixNationsYear, now));
            }
            
            // Champions Cup (April-May)
            if (now.Month <= 5 || now.Month >= 10)
            {
                var championsYear = now.Month >= 10 ? currentYear + 1 : currentYear;
                upcomingMatches.AddRange(GenerateChampionsCupFixtures(championsYear, now));
            }
            
            // Rugby Championship (July-September)
            if (now.Month <= 9 || now.Month >= 5)
            {
                upcomingMatches.AddRange(GenerateRugbyChampionshipFixtures(currentYear, now));
            }
            
            // Autumn Internationals (November)
            if (now.Month <= 11 || now.Month >= 8)
            {
                var autumnYear = now.Month >= 8 ? currentYear : currentYear + 1;
                upcomingMatches.AddRange(GenerateAutumnInternationalsFixtures(autumnYear, now));
            }
            
            return upcomingMatches.Where(m => m.DateTime > now).ToList();
        }

        private List<RugbyMatch> GenerateSixNationsFixtures(int year, DateTime now)
        {
            var fixtures = new List<RugbyMatch>();
            var teams = new[] { "France", "England", "Ireland", "Scotland", "Wales", "Italy" };
            var venues = new Dictionary<string, string>
            {
                ["France"] = "Stade de France, Paris",
                ["England"] = "Twickenham, London",
                ["Ireland"] = "Aviva Stadium, Dublin",
                ["Scotland"] = "Murrayfield, Edinburgh",
                ["Wales"] = "Principality Stadium, Cardiff",
                ["Italy"] = "Stadio Olimpico, Rome"
            };

            var round1Date = new DateTime(year, 2, 1, 15, 0, 0);
            var matchDates = new[]
            {
                round1Date,
                round1Date.AddDays(1),
                round1Date.AddDays(7),
                round1Date.AddDays(14),
                round1Date.AddDays(21),
                round1Date.AddDays(28),
                round1Date.AddDays(35)
            };

            var matchups = new[]
            {
                ("France", "Italy"), ("England", "Ireland"), ("Scotland", "Wales"),
                ("Ireland", "France"), ("Wales", "England"), ("Italy", "Scotland"),
                ("France", "Scotland"), ("England", "Wales"), ("Ireland", "Italy")
            };

            for (int i = 0; i < Math.Min(matchDates.Length, matchups.Length); i++)
            {
                if (matchDates[i] > now.AddDays(-10))
                {
                    var (team1, team2) = matchups[i];
                    fixtures.Add(new RugbyMatch
                    {
                        Team1 = team1,
                        Team2 = team2,
                        Competition = $"Six Nations {year}",
                        Venue = venues[team1],
                        DateTime = matchDates[i],
                        Description = $"Round {(i / 3) + 1}",
                        Score = matchDates[i] < now ? GenerateRandomScore() : ""
                    });
                }
            }

            return fixtures;
        }

        private List<RugbyMatch> GenerateRugbyChampionshipFixtures(int year, DateTime now)
        {
            var fixtures = new List<RugbyMatch>();
            var teams = new[] { "South Africa", "New Zealand", "Australia", "Argentina" };
            var venues = new Dictionary<string, string>
            {
                ["South Africa"] = "Ellis Park, Johannesburg",
                ["New Zealand"] = "Eden Park, Auckland",
                ["Australia"] = "Suncorp Stadium, Brisbane",
                ["Argentina"] = "Estadio Madre de Ciudades, Santiago"
            };

            var startDate = new DateTime(year, 7, 6, 17, 0, 0);
            var weeklyInterval = 7;

            var matchups = new[]
            {
                ("South Africa", "New Zealand"), ("Australia", "Argentina"),
                ("New Zealand", "Argentina"), ("South Africa", "Australia"),
                ("Argentina", "South Africa"), ("New Zealand", "Australia")
            };

            for (int i = 0; i < matchups.Length; i++)
            {
                var matchDate = startDate.AddDays(i * weeklyInterval);
                if (matchDate > now.AddDays(-10) && matchDate <= now.AddMonths(12))
                {
                    var (team1, team2) = matchups[i];
                    fixtures.Add(new RugbyMatch
                    {
                        Team1 = team1,
                        Team2 = team2,
                        Competition = $"Rugby Championship {year}",
                        Venue = venues[team1],
                        DateTime = matchDate,
                        Description = $"Round {i + 1}",
                        Score = matchDate < now ? GenerateRandomScore() : ""
                    });
                }
            }

            return fixtures;
        }

        private List<RugbyMatch> GenerateChampionsCupFixtures(int year, DateTime now)
        {
            var fixtures = new List<RugbyMatch>();
            
            var semiDate1 = new DateTime(year, 4, 26, 15, 0, 0);
            var semiDate2 = new DateTime(year, 4, 27, 15, 0, 0);
            var finalDate = new DateTime(year, 5, 25, 17, 45, 0);

            if (semiDate1 > now.AddDays(-10))
            {
                fixtures.Add(new RugbyMatch
                {
                    Team1 = "Toulouse",
                    Team2 = "Leinster",
                    Competition = "Champions Cup Semi-Final",
                    Venue = "Stade Ernest-Wallon, Toulouse",
                    DateTime = semiDate1,
                    Description = "Semi-final 1",
                    Score = semiDate1 < now ? GenerateRandomScore() : ""
                });
            }

            if (semiDate2 > now.AddDays(-10))
            {
                fixtures.Add(new RugbyMatch
                {
                    Team1 = "La Rochelle",
                    Team2 = "Leicester",
                    Competition = "Champions Cup Semi-Final",
                    Venue = "Stade Marcel-Deflandre, La Rochelle",
                    DateTime = semiDate2,
                    Description = "Semi-final 2",
                    Score = semiDate2 < now ? GenerateRandomScore() : ""
                });
            }

            if (finalDate > now.AddDays(-10))
            {
                fixtures.Add(new RugbyMatch
                {
                    Team1 = "TBD",
                    Team2 = "TBD",
                    Competition = "Champions Cup Final",
                    Venue = "Tottenham Hotspur Stadium, London",
                    DateTime = finalDate,
                    Description = "Final",
                    Score = finalDate < now ? GenerateRandomScore() : ""
                });
            }

            return fixtures;
        }

        private List<RugbyMatch> GenerateAutumnInternationalsFixtures(int year, DateTime now)
        {
            var fixtures = new List<RugbyMatch>();
            var novemberDates = new[]
            {
                new DateTime(year, 11, 2, 17, 30, 0),
                new DateTime(year, 11, 9, 15, 0, 0),
                new DateTime(year, 11, 16, 17, 30, 0),
                new DateTime(year, 11, 23, 20, 0, 0)
            };

            var matchups = new[]
            {
                ("France", "Japan", "U Arena, Paris"),
                ("England", "South Africa", "Twickenham, London"),
                ("Ireland", "New Zealand", "Aviva Stadium, Dublin"),
                ("Wales", "Australia", "Principality Stadium, Cardiff")
            };

            for (int i = 0; i < Math.Min(novemberDates.Length, matchups.Length); i++)
            {
                if (novemberDates[i] > now.AddDays(-10))
                {
                    var (team1, team2, venue) = matchups[i];
                    fixtures.Add(new RugbyMatch
                    {
                        Team1 = team1,
                        Team2 = team2,
                        Competition = "Autumn International",
                        Venue = venue,
                        DateTime = novemberDates[i],
                        Description = $"Autumn International {year}",
                        Score = novemberDates[i] < now ? GenerateRandomScore() : ""
                    });
                }
            }

            return fixtures;
        }

        private RugbyMatch? CreateRandomMatch(DateTime matchDate, bool withScore)
        {
            var random = new Random();
            var teams = new[] { "France", "England", "Ireland", "Scotland", "Wales", "Italy", "South Africa", "New Zealand", "Australia", "Argentina" };
            var competitions = new[] { "Test Match", "Six Nations", "Rugby Championship", "World Cup Qualifier" };
            
            var team1 = teams[random.Next(teams.Length)];
            var team2 = teams[random.Next(teams.Length)];
            
            if (team1 == team2) return null;

            return new RugbyMatch
            {
                Team1 = team1,
                Team2 = team2,
                Competition = competitions[random.Next(competitions.Length)],
                Venue = "Various Stadium",
                DateTime = matchDate,
                Description = "International Match",
                Score = withScore ? GenerateRandomScore() : ""
            };
        }

        private string GenerateRandomScore()
        {
            var random = new Random();
            var score1 = random.Next(10, 45);
            var score2 = random.Next(10, 45);
            return $"{score1}-{score2}";
        }

        private string GetCompetitionDisplayName(string apiCompetition)
        {
            return apiCompetition switch
            {
                "six-nations" => "Six Nations",
                "rugby-championship" => "Rugby Championship",
                "world-cup" => "World Cup",
                _ => apiCompetition
            };
        }
    }

    // API Response Models for SportRadar
    public class SportRadarResponse
    {
        [JsonPropertyName("fixtures")]
        public List<SportRadarFixture>? Fixtures { get; set; }
    }

    public class SportRadarFixture
    {
        [JsonPropertyName("start_time")]
        public DateTime StartTime { get; set; }
        
        [JsonPropertyName("status")]
        public string Status { get; set; } = "";
        
        [JsonPropertyName("home_team")]
        public SportRadarTeam? HomeTeam { get; set; }
        
        [JsonPropertyName("away_team")]
        public SportRadarTeam? AwayTeam { get; set; }
        
        [JsonPropertyName("venue")]
        public SportRadarVenue? Venue { get; set; }
        
        [JsonPropertyName("stage")]
        public string? Stage { get; set; }
        
        [JsonPropertyName("home_score")]
        public int HomeScore { get; set; }
        
        [JsonPropertyName("away_score")]
        public int AwayScore { get; set; }
    }

    public class SportRadarTeam
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";
    }

    public class SportRadarVenue
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";
    }

    // Existing RugbyMatch model (keeping it compatible)
    public class RugbyMatch
    {
        public string Team1 { get; set; } = "";
        public string Team2 { get; set; } = "";
        public string Competition { get; set; } = "";
        public string Venue { get; set; } = "";
        public DateTime DateTime { get; set; }
        public string Score { get; set; } = "";
        public string Description { get; set; } = "";
    }
}