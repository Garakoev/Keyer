using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;
using Keyer.Data;
using Keyer.Pages;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;

namespace Keyer
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddMudServices();
            builder.Services.AddSingleton<IFileSaver>(FileSaver.Default);
            builder.Services.AddTransient<PicturesKeyer>();
#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            builder.Services.AddSingleton<WeatherForecastService>();

            return builder.Build();
        }
    }
}