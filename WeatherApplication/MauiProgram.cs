using Microsoft.Extensions.Logging;

namespace WeatherApplication
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder

                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Request required permissions
            builder.Services.AddSingleton<IGeolocation>( Geolocation.Default);
            builder.Services.AddSingleton <WeatherService>();
            builder.Services.AddSingleton<MainPage>();
            ;


            return builder.Build();
        }
    }
}
