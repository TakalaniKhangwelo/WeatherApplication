using System.Text.Json.Serialization;

public class WeatherData
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("main")]
    public Main Main { get; set; }

    [JsonPropertyName("weather")]
    public Weather[] Weather { get; set; }
}

public class Main
{
    [JsonPropertyName("temp")]
    public double Temperature { get; set; }

    [JsonPropertyName("humidity")]
    public double Humidity { get; set; }

    [JsonPropertyName("feels_like")]
    public double FeelsLike { get; set; }
}

public class Weather
{
    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("icon")]
    public string Icon { get; set; }
}