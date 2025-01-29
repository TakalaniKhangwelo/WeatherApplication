using System.Diagnostics;

namespace WeatherApplication
{
    public partial class MainPage : ContentPage
    {
        private readonly WeatherService _weatherService;
        private readonly IGeolocation _geolocation;

        public MainPage(WeatherService weatherService, IGeolocation geolocation)
        {
            InitializeComponent();
            _weatherService = weatherService;
            _geolocation = geolocation;
            this.Appearing += async (s, e) => await LoadWeatherDataAsync();
        }

        private async Task LoadWeatherDataAsync()
        {
            try
            {
                // Show loading state
                LocationLabel.Text = "Getting location...";
                DescriptionLabel.Text = "Loading weather...";

                var location = await GetCurrentLocation();
                if (location != null)
                {
                    var weatherData = await _weatherService.GetWeatherDataAsync(
                        location.Latitude,
                        location.Longitude
                    );
                    UpdateUI(weatherData);
                }
                else
                {
                    LocationLabel.Text = "Location unavailable";
                    DescriptionLabel.Text = "Could not load weather data";
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Could not load weather data: " + ex.Message, "OK");
                LocationLabel.Text = "Error";
                DescriptionLabel.Text = "Weather data unavailable";
            }
        }
        private async Task<Location> GetCurrentLocation()
        {
            try
            {
                // Check permissions with detailed logging
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                Debug.WriteLine($"Initial permission status: {status}");

                if (status != PermissionStatus.Granted)
                {
                    Debug.WriteLine("Permission not granted, requesting permission...");
                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                    Debug.WriteLine($"Permission request result: {status}");

                    if (status != PermissionStatus.Granted)
                    {
                        await DisplayAlert("Permission Denied",
                            "Location permission is required. Please enable it in app settings.",
                            "OK");
                        return null;
                    }
                }

                // Try to get location with timeout and accuracy settings
                var request = new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.Medium,
                    Timeout = TimeSpan.FromSeconds(5)
                };

                Debug.WriteLine("Requesting location...");
                var location = await _geolocation.GetLocationAsync(request);

                if (location == null)
                {
                    Debug.WriteLine("Location returned null");
                    throw new Exception("Could not get location");
                }

                Debug.WriteLine($"Location retrieved successfully: Lat={location.Latitude}, Lon={location.Longitude}");
                return location;
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                Debug.WriteLine($"Feature not supported: {fnsEx.Message}");
                await DisplayAlert("Error", "Location services are not supported on this device.", "OK");
                return null;
            }
            catch (FeatureNotEnabledException fneEx)
            {
                Debug.WriteLine($"Feature not enabled: {fneEx.Message}");
                await DisplayAlert("Error", "Please enable location services in your device settings.", "OK");
                return null;
            }
            catch (PermissionException pEx)
            {
                Debug.WriteLine($"Permission error: {pEx.Message}");
                await DisplayAlert("Error", "Location permission was denied.", "OK");
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"General location error: {ex.GetType().Name} - {ex.Message}");
                await DisplayAlert("Location Error",
                    "Unable to get current location. Please check if location services are enabled.",
                    "OK");
                return null;
            }
        }
        private void UpdateUI(WeatherData weatherData)
        {
            if (weatherData == null)
            {
                Debug.WriteLine("UpdateUI: weatherData is null");
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    LocationLabel.Text = "No weather data available";
                    TemperatureLabel.Text = "--°C";
                    DescriptionLabel.Text = "Weather data unavailable";
                    FeelsLikeLabel.Text = "Feels like: --°C";
                    HumidityLabel.Text = "Humidity: --%";
                });
                return;
            }

            try
            {
                Debug.WriteLine($"UpdateUI: Attempting to update with data for {weatherData.Name}");
                Debug.WriteLine($"Temperature: {weatherData.Main?.Temperature}");
                Debug.WriteLine($"Weather Array Length: {weatherData.Weather?.Length ?? 0}");
                Debug.WriteLine($"Weather Description: {weatherData.Weather?[0]?.Description}");

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        // Update location
                        LocationLabel.Text = weatherData.Name ?? "Unknown Location";

                        // Update temperature
                        if (weatherData.Main != null)
                        {
                            TemperatureLabel.Text = $"{Math.Round(weatherData.Main.Temperature)}°C";
                            FeelsLikeLabel.Text = $"Feels like: {Math.Round(weatherData.Main.FeelsLike)}°C";
                            HumidityLabel.Text = $"Humidity: {weatherData.Main.Humidity}%";
                        }
                        else
                        {
                            TemperatureLabel.Text = "--°C";
                            FeelsLikeLabel.Text = "Feels like: --°C";
                            HumidityLabel.Text = "Humidity: --%";
                        }

                        // Update description
                        if (weatherData.Weather != null && weatherData.Weather.Length > 0)
                        {
                            DescriptionLabel.Text = weatherData.Weather[0].Description ?? "No description available";
                        }
                        else
                        {
                            DescriptionLabel.Text = "Weather description unavailable";
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"UI Update Inner Error: {ex.Message}");
                        Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                        LocationLabel.Text = "Error updating display";
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UI Update Outer Error: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    LocationLabel.Text = "Error updating display";
                    TemperatureLabel.Text = "--°C";
                    DescriptionLabel.Text = "Error occurred";
                    FeelsLikeLabel.Text = "Feels like: --°C";
                    HumidityLabel.Text = "Humidity: --%";
                });
            }
        }

        private async void OnRefreshClicked(object sender, EventArgs e)
        {
            RefreshButton.IsEnabled = false;
            try
            {
                await LoadWeatherDataAsync();
            }
            finally
            {
                RefreshButton.IsEnabled = true;
            }
        }
    }
}