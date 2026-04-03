using Microsoft.Extensions.Logging;
using project_Telephone_directory.Database;
using project_Telephone_directory.Services;
using project_Telephone_directory.ViewModels;
using SkiaSharp.Views.Maui.Controls.Hosting;
#if ANDROID
using project_Telephone_directory.Platforms.Android.Services;
#endif

namespace project_Telephone_directory;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseSkiaSharp()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton<ContactDatabase>();
        builder.Services.AddSingleton<ICryptoService, CryptoService>();
        builder.Services.AddSingleton<IContactRepository, ContactRepository>();
        builder.Services.AddSingleton<IContactImportExportService, ContactImportExportService>();

#if ANDROID
        builder.Services.AddSingleton<ISystemContactsService, AndroidContactsService>();
#elif IOS || MACCATALYST
        builder.Services.AddSingleton<ISystemContactsService, AppleContactsService>();
#else
        builder.Services.AddSingleton<ISystemContactsService, DefaultSystemContactsService>();
#endif

        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<ContactsListViewModel>();
        builder.Services.AddTransient<SearchViewModel>();
        builder.Services.AddTransient<ContactDetailViewModel>();
        builder.Services.AddTransient<ContactEditViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();

        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<ContactsListPage>();
        builder.Services.AddTransient<SearchPage>();
        builder.Services.AddTransient<ContactDetailPage>();
        builder.Services.AddTransient<ContactEditPage>();
        builder.Services.AddTransient<SettingsPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();
        App.Services = app.Services;
        return app;
    }
}
