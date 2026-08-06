using Microsoft.Extensions.Logging;

namespace InternshipMpbile
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

#if ANDROID
            // Android'in giriş alanlarına eklediği varsayılan alt çizgiyi kaldırır,
            // böylece alanlar tasarımdaki gibi yalnızca kendi çerçevelerini gösterir.
            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("NoUnderline",
                (handler, view) => handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent));
            Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("NoUnderline",
                (handler, view) => handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent));
            Microsoft.Maui.Handlers.DatePickerHandler.Mapper.AppendToMapping("NoUnderline",
                (handler, view) => handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent));
#endif

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
