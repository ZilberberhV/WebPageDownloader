using Microsoft.Extensions.Hosting;
using WebPageDownloader.Core.Dependencies;
using Microsoft.Extensions.DependencyInjection;
using WebPageDownloader.Core.Interfaces;

var defaultUrls = new[]
{
    "https://www.entaingroup.com/",
    "https://www.microsoft.com/en-us/",
    "https://www.github.com/",
    "https://www.wikipedia.org/",
    "https://owasp.org/",
    "https://www.opengroup.org/togaf",
};

string? downloadsRootFolder = "Downloads";
string[] urls = defaultUrls;

if (args.Length > 0)
{
    if (IsValidUrl(args[0], out _))
    {
        // No downloadsRootFolder provided, all args are URLs
        urls = args;
    }
    else
    {
        downloadsRootFolder = args[0];
        if (args.Length > 1)
        {
            urls = [.. args.Skip(1)];
        }
        else
        {
            // No URLs provided, fallback to default
            urls = defaultUrls;
        }
    }
}

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddPageDownloadServices();
builder.Services.AddLocalFileSystemAccessor();
builder.Services.AddHttpClient();

using IHost host = builder.Build();

var downloader = host.Services.GetRequiredService<IPageDownloadService>();

await foreach (var savedPage in downloader.DownloadPagesAsync(urls, downloadsRootFolder))
{
    Console.WriteLine($"Downloaded {savedPage.OriginalUrl} to {savedPage.SavedHtmlPath}");
}

Console.WriteLine("Downloading process is done");

await host.RunAsync();

static bool IsValidUrl(string url, out Uri? uri)
{
    if (Uri.TryCreate(url, UriKind.Absolute, out uri))
    {
        return uri.Scheme == Uri.UriSchemeHttp ||
               uri.Scheme == Uri.UriSchemeHttps;
    }
    return false;
}

