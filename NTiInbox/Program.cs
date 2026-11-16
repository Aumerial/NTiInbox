using Microsoft.AspNetCore.DataProtection;
using NTiInbox.Components;
using NTiInbox.Connection;
using NTiInbox.Localization;
using NTiInbox.Messages;
using NTiInbox.Storage;
using NTiInbox.UserInterface;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Keys live next to the other local data, so that a restarted container keeps reading the
// cookies it issued before. Without this, a browser holding an old cookie gets an error page.
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "App_Data", "keys")))
    .SetApplicationName("NTiInbox");

builder.Services.Configure<IbmiOptions>(builder.Configuration.GetSection(IbmiOptions.Section));

// Local data: one JSON file per user. There is no authentication in this POC, so the
// current user is a constant; replace ICurrentUser before any shared deployment.
builder.Services.AddSingleton<JsonUserStore>();
builder.Services.AddSingleton<ICurrentUser, LocalCurrentUser>();

// Session services: one per Blazor circuit, hence one per open tab.
builder.Services.AddScoped<Localizer>();
builder.Services.AddScoped<IbmiSession>();
builder.Services.AddScoped<ErrorText>();
builder.Services.AddScoped<ConnectionProfileStore>();
builder.Services.AddScoped<InboxState>();
builder.Services.AddScoped<ToastService>();
builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<BrowserNotifier>();
builder.Services.AddScoped<ThemeService>();

// Business logic: a single facade over the connection of the current circuit.
builder.Services.AddScoped<MessageQueueService>();

var app = builder.Build();

// A missing translation is caught at startup in development, not by a user.
if (app.Environment.IsDevelopment() && Strings.FindMismatches() is { Count: > 0 } mismatches)
{
    throw new InvalidOperationException($"Translation keys differ:{Environment.NewLine}{string.Join(Environment.NewLine, mismatches)}");
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

// First visit: browser language. Then the culture cookie written by the language toggle.
string[] cultures = [.. Localizer.SupportedLanguages];
app.UseRequestLocalization(options => options
    .SetDefaultCulture(Localizer.DefaultLanguage)
    .AddSupportedCultures(cultures)
    .AddSupportedUICultures(cultures));

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
