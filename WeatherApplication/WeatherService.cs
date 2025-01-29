using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class WeatherService
{
    private readonly HttpClient _httpClient;
    private const string API_KEY = "501c9ee04bcc0a36d8c9202cf2e6dfd8";
    private const string BASE_URL = "https://api.openweathermap.org/data/2.5/weather";

    public WeatherService()
    {
        _httpClient = new HttpClient();
    }

    public async Task<WeatherData> GetWeatherDataAsync(double latitude, double longitude)
    {
        try
        {
            Debug.WriteLine($"Making API request for coordinates: Lat={latitude}, Lon={longitude}");

            var requestUrl = $"{BASE_URL}?lat={latitude}&lon={longitude}&appid={API_KEY}&units=metric";
            Debug.WriteLine($"Request URL: {requestUrl}");

            var response = await _httpClient.GetAsync(requestUrl);
            Debug.WriteLine($"API Response Status: {response.StatusCode}");

            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"API Response Content: {json}");

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var weatherData = JsonSerializer.Deserialize<WeatherData>(json, options);

            if (weatherData == null)
            {
                throw new Exception("Failed to deserialize weather data");
            }

            Debug.WriteLine($"Deserialized Weather Data:");
            Debug.WriteLine($"Location Name: {weatherData.Name}");
            Debug.WriteLine($"Temperature: {weatherData.Main?.Temperature}°C");
            Debug.WriteLine($"Description: {weatherData.Weather?.FirstOrDefault()?.Description}");

            return weatherData;
        }
        catch (HttpRequestException ex)
        {
            Debug.WriteLine($"HTTP Request Error: {ex.Message}");
            throw;
        }
        catch (JsonException ex)
        {
            Debug.WriteLine($"JSON Parsing Error: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"General Error in GetWeatherDataAsync: {ex.Message}");
            throw;
        }
    }
}
