using System.ComponentModel.DataAnnotations;

namespace MyWebApp.Services
{
    public class CoffeeService
    {
        private List<CoffeeEntry> _coffeeEntries = new();

        public event Action? OnCoffeeListChanged;

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