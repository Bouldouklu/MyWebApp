using System.Text.Json;

namespace MyWebApp.Services
{
    public class WeatherService
    {
        private readonly HttpClient _httpClient;
        
        // Coordinates for Obertrum am See, Austria
        private const double DefaultLatitude = 47.8333;
        private const double DefaultLongitude = 13.1667;

        public WeatherService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<CurrentWeather?> GetCurrentWeatherAsync(double? latitude = null, double? longitude = null)
        {
            try
            {
                var lat = latitude ?? DefaultLatitude;
                var lon = longitude ?? DefaultLongitude;
                
                var url = $"https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}&current=temperature_2m,relative_humidity_2m,weather_code,wind_speed_10m&timezone=auto";
                
                var response = await _httpClient.GetStringAsync(url);
                var weatherData = JsonSerializer.Deserialize<OpenMeteoResponse>(response);
                
                if (weatherData?.Current != null)
                {
                    return new CurrentWeather
                    {
                        Temperature = weatherData.Current.Temperature2m,
                        Humidity = weatherData.Current.RelativeHumidity2m,
                        WindSpeed = weatherData.Current.WindSpeed10m,
                        WeatherCode = weatherData.Current.WeatherCode,
                        WeatherDescription = GetWeatherDescription(weatherData.Current.WeatherCode),
                        Location = await GetLocationNameAsync(lat, lon)
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching current weather: {ex.Message}");
            }
            
            return null;
        }

        public async Task<WeeklyForecast?> GetWeeklyForecastAsync(double? latitude = null, double? longitude = null)
        {
            try
            {
                var lat = latitude ?? DefaultLatitude;
                var lon = longitude ?? DefaultLongitude;
                
                var url = $"https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}&daily=temperature_2m_max,temperature_2m_min,weather_code,precipitation_sum,wind_speed_10m_max&timezone=auto&forecast_days=7";
                
                var response = await _httpClient.GetStringAsync(url);
                var weatherData = JsonSerializer.Deserialize<OpenMeteoResponse>(response);
                
                if (weatherData?.Daily != null)
                {
                    var dailyForecasts = new List<DailyForecast>();
                    
                    for (int i = 0; i < weatherData.Daily.Time.Count && i < 7; i++)
                    {
                        dailyForecasts.Add(new DailyForecast
                        {
                            Date = DateTime.Parse(weatherData.Daily.Time[i]),
                            MaxTemperature = weatherData.Daily.Temperature2mMax[i],
                            MinTemperature = weatherData.Daily.Temperature2mMin[i],
                            WeatherCode = weatherData.Daily.WeatherCode[i],
                            WeatherDescription = GetWeatherDescription(weatherData.Daily.WeatherCode[i]),
                            PrecipitationSum = weatherData.Daily.PrecipitationSum[i],
                            MaxWindSpeed = weatherData.Daily.WindSpeed10mMax[i]
                        });
                    }
                    
                    return new WeeklyForecast
                    {
                        Location = await GetLocationNameAsync(lat, lon),
                        DailyForecasts = dailyForecasts
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching weekly forecast: {ex.Message}");
            }
            
            return null;
        }

        public async Task<(double latitude, double longitude)?> GetCurrentLocationAsync()
        {
            // This would require JavaScript geolocation API - will be handled on the client side
            // For now, return default location (Obertrum am See)
            await Task.CompletedTask;
            return (DefaultLatitude, DefaultLongitude);
        }

        private async Task<string> GetLocationNameAsync(double latitude, double longitude)
        {
            try
            {
                var url = $"https://geocoding-api.open-meteo.com/v1/search?latitude={latitude}&longitude={longitude}&count=1";
                var response = await _httpClient.GetStringAsync(url);
                var geocodingData = JsonSerializer.Deserialize<GeocodingResponse>(response);
                
                if (geocodingData?.Results?.Any() == true)
                {
                    var result = geocodingData.Results.First();
                    return $"{result.Name}, {result.Country}";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching location name: {ex.Message}");
            }
            
            return "Obertrum am See, Austria";
        }

        private static string GetWeatherDescription(int weatherCode)
        {
            return weatherCode switch
            {
                0 => "Clear sky",
                1 => "Mainly clear",
                2 => "Partly cloudy",
                3 => "Overcast",
                45 => "Fog",
                48 => "Depositing rime fog",
                51 => "Light drizzle",
                53 => "Moderate drizzle", 
                55 => "Dense drizzle",
                56 => "Light freezing drizzle",
                57 => "Dense freezing drizzle",
                61 => "Slight rain",
                63 => "Moderate rain",
                65 => "Heavy rain",
                66 => "Light freezing rain",
                67 => "Heavy freezing rain",
                71 => "Slight snow fall",
                73 => "Moderate snow fall",
                75 => "Heavy snow fall",
                77 => "Snow grains",
                80 => "Slight rain showers",
                81 => "Moderate rain showers",
                82 => "Violent rain showers",
                85 => "Slight snow showers",
                86 => "Heavy snow showers",
                95 => "Thunderstorm",
                96 => "Thunderstorm with slight hail",
                99 => "Thunderstorm with heavy hail",
                _ => "Unknown"
            };
        }

        public static string GetWeatherIcon(int weatherCode)
        {
            return weatherCode switch
            {
                0 or 1 => "☀️",
                2 => "⛅",
                3 => "☁️",
                45 or 48 => "🌫️",
                51 or 53 or 55 or 56 or 57 => "🌦️",
                61 or 63 or 65 or 66 or 67 or 80 or 81 or 82 => "🌧️",
                71 or 73 or 75 or 77 or 85 or 86 => "🌨️",
                95 or 96 or 99 => "⛈️",
                _ => "🌤️"
            };
        }
    }

    // Data models for Open-Meteo API response
    public class OpenMeteoResponse
    {
        public CurrentWeatherData? Current { get; set; }
        public DailyWeatherData? Daily { get; set; }
    }

    public class CurrentWeatherData
    {
        public double Temperature2m { get; set; }
        public int RelativeHumidity2m { get; set; }
        public double WindSpeed10m { get; set; }
        public int WeatherCode { get; set; }
    }

    public class DailyWeatherData
    {
        public List<string> Time { get; set; } = new();
        public List<double> Temperature2mMax { get; set; } = new();
        public List<double> Temperature2mMin { get; set; } = new();
        public List<int> WeatherCode { get; set; } = new();
        public List<double> PrecipitationSum { get; set; } = new();
        public List<double> WindSpeed10mMax { get; set; } = new();
    }

    public class GeocodingResponse
    {
        public List<GeocodingResult>? Results { get; set; }
    }

    public class GeocodingResult
    {
        public string Name { get; set; } = "";
        public string Country { get; set; } = "";
    }

    // Models for our application
    public class CurrentWeather
    {
        public double Temperature { get; set; }
        public int Humidity { get; set; }
        public double WindSpeed { get; set; }
        public int WeatherCode { get; set; }
        public string WeatherDescription { get; set; } = "";
        public string Location { get; set; } = "";
    }

    public class WeeklyForecast
    {
        public string Location { get; set; } = "";
        public List<DailyForecast> DailyForecasts { get; set; } = new();
    }

    public class DailyForecast
    {
        public DateTime Date { get; set; }
        public double MaxTemperature { get; set; }
        public double MinTemperature { get; set; }
        public int WeatherCode { get; set; }
        public string WeatherDescription { get; set; } = "";
        public double PrecipitationSum { get; set; }
        public double MaxWindSpeed { get; set; }
    }
}