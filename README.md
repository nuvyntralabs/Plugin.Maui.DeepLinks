# Plugin.Maui.DeepLinks

[![NuGet](https://img.shields.io/nuget/v/Plugin.Maui.DeepLinks.svg?label=NuGet)](https://www.nuget.org/packages/Plugin.Maui.DeepLinks)

Make deep linking actually pleasant.

A .NET MAUI plugin for **iOS** and **Android** that maps incoming URIs to handlers:

```csharp
DeepLinks.Map(
    "/orders/{id}",
    async route =>
    {
        await Shell.Current.GoToAsync(
            $"order?id={route["id"]}");
    });
```

Handles all of these the same way:

```
https://example.com/orders/123
myapp://orders/123
```

## Install

Package: [https://www.nuget.org/packages/Plugin.Maui.DeepLinks](https://www.nuget.org/packages/Plugin.Maui.DeepLinks)

```bash
dotnet add package Plugin.Maui.DeepLinks
```

Target frameworks: `net10.0`, `net10.0-android`, `net10.0-ios`.

## Quick start

```csharp
using Plugin.Maui.DeepLinks;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiDeepLinks(options =>
            {
                options.Hosts.Add("example.com");
                options.CustomSchemes.Add("myapp");
                options.IsAuthenticated = () => session.IsLoggedIn;
                options.LoginPath = "//login";
            });

        return builder.Build();
    }
}
```

Call `DeepLinks.MarkReady()` after `AppShell` is created (the sample does this in `CreateWindow`). `UseMauiDeepLinks` also marks ready on Android resume / iOS activate when `Shell.Current` exists.

```csharp
DeepLinks.Map(
    "/orders/{id}",
    async route =>
    {
        await Shell.Current.GoToAsync($"order?id={route["id"]}");
    });

DeepLinks.Map(
    "/account/{section}",
    async route =>
    {
        await Shell.Current.GoToAsync($"//account?section={route["section"]}");
    },
    requiresAuthentication: true);
```

Or map straight to a Shell path:

```csharp
DeepLinks.Map("/orders/{id}", "order?id={id}");
```

## What you get

| Capability | How |
| --- | --- |
| **Android App Links** | `https` VIEW intents from `OnCreate` / `OnNewIntent` |
| **iOS Universal Links** | `NSUserActivity` browsing-web + launch options |
| **Custom schemes** | `myapp://orders/123` (host + path become `/orders/123`) |
| **Cold start** | Queued until `MarkReady()`, optionally persisted |
| **Warm start** | Dispatched immediately when Shell is ready |
| **Authentication-required links** | Persist the original URI, open login, restore after sign-in |
| **Deferred navigation** | Not-ready queue and auth hold, including process death |
| **Navigation stack restoration** | Snapshot before dispatch; `RestoreNavigationStackAsync()` |

## Authentication-required links

```
Open link
   ↓
Not logged in
   ↓
Login
   ↓
Restore original link
   ↓
Navigate to requested page
```

```csharp
options.IsAuthenticated = () => session.IsLoggedIn;
options.LoginPath = "//login";

DeepLinks.Map("/account/{section}", handler, requiresAuthentication: true);

// After a successful login:
DeepLinks.NotifyAuthenticated();
```

The original URI is written to app data so a process death during login still restores the destination.

## Route templates

| Template | Matches |
| --- | --- |
| `/orders/{id}` | `/orders/123` → `route["id"]` |
| `/orders/{id}/items/{itemId}` | Nested parameters |
| `/orders/{id?}` | Optional segment |
| `/files/{*path}` | Catch-all remainder |
| `/orders/{id:int}` | Integer constraint |
| `/users/{id:guid}` | GUID constraint |

Query values are available through the same indexer: `https://example.com/orders/123?src=email` → `route["src"]`.

More specific templates win (`/orders/mine` over `/orders/{id}` over `/orders/{*rest}`).

## Events

```csharp
var links = DeepLinks.Current;
links.Received += (_, e) => { };
links.Navigated += (_, e) => { };
links.Deferred += (_, e) => { };
links.Unhandled += (_, e) => { };
links.Failed += (_, e) => { };
```

## Host app setup

The plugin routes URIs. The OS still needs to deliver them to your app.

### Android App Links

`LaunchMode.SingleTop` on `MainActivity`, plus an intent filter. Host `/.well-known/assetlinks.json` on the domain for verification.

```csharp
[Activity(LaunchMode = LaunchMode.SingleTop, MainLauncher = true, /* ... */)]
[IntentFilter(
    [Intent.ActionView],
    Categories = [Intent.CategoryDefault, Intent.CategoryBrowsable],
    DataScheme = "https",
    DataHost = "example.com",
    DataPathPrefix = "/",
    AutoVerify = true)]
[IntentFilter(
    [Intent.ActionView],
    Categories = [Intent.CategoryDefault, Intent.CategoryBrowsable],
    DataScheme = "myapp")]
public class MainActivity : MauiAppCompatActivity
{
}
```

You can also call `DeepLinks.HandleIntent(intent)` yourself.

### iOS Universal Links

Associated domain `applinks:example.com` and `apple-app-site-association` on the host. Custom schemes go in `Info.plist` `CFBundleURLTypes`.

```xml
<key>com.apple.developer.associated-domains</key>
<array>
    <string>applinks:example.com</string>
</array>
```

You can also call `DeepLinks.HandleUrl(url)` or `DeepLinks.HandleUserActivity(activity)`.

## Navigation stack

Before a matched link runs, the current Shell location is stored.

```csharp
await DeepLinks.RestoreNavigationStackAsync();
```

Disable per route with `new DeepLinkMapOptions { CaptureNavigationStack = false }`.

## Without the generic host

```csharp
var links = DeepLinks.Create(new DeepLinksOptions
{
    Hosts = { "example.com" },
    CustomSchemes = { "myapp" },
    IsAuthenticated = () => session.IsLoggedIn,
    LoginPath = "//login"
});

DeepLinks.SetDefault(links);
links.Map("/orders/{id}", "order?id={id}");
await links.HandleAsync("https://example.com/orders/123");
```

## Sample

`samples/Plugin.Maui.DeepLinks.Sample` walks through public links, custom schemes, login restore, and stack restoration.

```bash
dotnet build src/Plugin.Maui.DeepLinks/Plugin.Maui.DeepLinks.csproj
dotnet pack src/Plugin.Maui.DeepLinks/Plugin.Maui.DeepLinks.csproj -c Release -o artifacts
dotnet test tests/Plugin.Maui.DeepLinks.Tests/Plugin.Maui.DeepLinks.Tests.csproj
dotnet build samples/Plugin.Maui.DeepLinks.Sample/Plugin.Maui.DeepLinks.Sample.csproj -f net10.0-android
```

## Pack from source

```bash
dotnet pack src/Plugin.Maui.DeepLinks/Plugin.Maui.DeepLinks.csproj -c Release -o artifacts
```

The `.nupkg` is written to `artifacts/Plugin.Maui.DeepLinks.1.0.0.nupkg`.

## License

MIT

## Support

> If this plugin saved you a weekend of native plumbing, consider buying me a coffee.
> Your support keeps it maintained, documented, and free.

[![Buy Me A Coffee](https://img.shields.io/badge/Buy%20Me%20a%20Coffee-ffdd00?style=for-the-badge&logo=buy-me-a-coffee&logoColor=black)](https://buymeacoffee.com/npadhy)

This library stays open source. A coffee helps cover time for bug fixes, new features, and docs.
