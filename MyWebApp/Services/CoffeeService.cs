using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;

namespace MyWebApp.Services
{
    public class CoffeeService
    {
        private readonly HttpClient _httpClient;
        private List<CoffeeEntry> _coffeeEntries = new();

        // 🆕 JSONBin Configuration - loaded from environment variables
        private readonly string _binId;
        private readonly string _apiKey;
        private readonly string _jsonBinUrl;

        public event Action? OnCoffeeListChanged;

        // 🆕 Add HttpClient injection
        public CoffeeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            
            // Get values from environment variables (set in Codespace Secrets)
            _binId = Environment.GetEnvironmentVariable("JSONBIN_BIN_ID") 
                     ?? throw new InvalidOperationException("JSONBIN_BIN_ID environment variable not set");
            _apiKey = Environment.GetEnvironmentVariable("JSONBIN_API_KEY") 
                      ?? throw new InvalidOperationException("JSONBIN_API_KEY environment variable not set");
            
            _jsonBinUrl = $"https://api.jsonbin.io/v3/b/{_binId}";
            
            // Set up JSONBin headers
            _httpClient.DefaultRequestHeaders.Add("X-Master-Key", _apiKey);
            _httpClient.DefaultRequestHeaders.Add("X-Bin-Meta", "false");
        }

        public void AddCoffee(CoffeeEntry entry)
        {
            _coffeeEntries.Add(entry);
            OnCoffeeListChanged?.Invoke();
        }

        public void RemoveCoffee(CoffeeEntry entry)
        {
            _coffeeEntries.Remove(entry);
            OnCoffeeListChanged?.Invoke();
        }

        public List<CoffeeEntry> GetAllCoffees()
        {
            return _coffeeEntries.ToList();
        }

        public List<CoffeeEntry> GetTodaysCoffees()
        {
            var today = DateTime.Today;
            return _coffeeEntries.Where(c => c.DateTime.Date == today).ToList();
        }

        public int GetTotalEntries()
        {
            return _coffeeEntries.Count;
        }

        public int GetTotalVolume()
        {
            return _coffeeEntries.Sum(c => c.Volume);
        }

        public double GetAverageRating()
        {
            return _coffeeEntries.Count > 0 ? _coffeeEntries.Average(c => c.Rating) : 0;
        }

        // 🆕 NEW METHOD: Save to JSONBin
        public async Task<bool> SaveToCloudAsync()
        {
            try
            {
                var json = JsonSerializer.Serialize(_coffeeEntries, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync(_jsonBinUrl, content);
                
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving to cloud: {ex.Message}");
                return false;
            }
        }

        // 🆕 NEW METHOD: Load from JSONBin
        public async Task<bool> LoadFromCloudAsync()
        {
            try
            {
                var response = await _httpClient.GetStringAsync($"{_jsonBinUrl}/latest");
                var entries = JsonSerializer.Deserialize<List<CoffeeEntry>>(response);
                
                if (entries != null)
                {
                    _coffeeEntries = entries;
                    OnCoffeeListChanged?.Invoke();
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading from cloud: {ex.Message}");
                return false;
            }
        }
    }

    public class CoffeeEntry
    {
        public Guid Id { get; set; }
        
        [Required(ErrorMessage = "Coffee name is required")]
        public string CoffeeName { get; set; } = "";
        
        [Required]
        [Range(1, 100, ErrorMessage = "Temperature must be between 1 and 100°C")]
        public int Temperature { get; set; }
        
        [Required]
        [Range(1, 1000, ErrorMessage = "Volume must be between 1 and 1000ml")]
        public int Volume { get; set; }
        
        [Required(ErrorMessage = "Please select a brew type")]
        public string BrewType { get; set; } = "";
        
        [Required]
        [Range(1, 5, ErrorMessage = "Please rate the coffee from 1 to 5 stars")]
        public int Rating { get; set; }
        
        [Required]
        [Range(8, 24, ErrorMessage = "Grind setting must be between 8 and 24")]
        public int GrindSetting { get; set; }
        
        [Required]
        [Range(0, 59, ErrorMessage = "Brew time minutes must be between 0 and 59")]
        public int BrewTimeMinutes { get; set; }
        
        [Required]
        [Range(0, 59, ErrorMessage = "Brew time seconds must be between 0 and 59")]
        public int BrewTimeSeconds { get; set; }
        
        public DateTime DateTime { get; set; }
        
        // Helper property to get total brew time in seconds
        public int TotalBrewTimeSeconds => BrewTimeMinutes * 60 + BrewTimeSeconds;
        
        // Helper property to format brew time as string
        public string FormattedBrewTime => $"{BrewTimeMinutes}:{BrewTimeSeconds:D2}";
    }
}