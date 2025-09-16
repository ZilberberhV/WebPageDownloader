using Microsoft.Extensions.Hosting;
using WebPageDownloader.Core.Dependencies;
using Microsoft.Extensions.DependencyInjection;
using WebPageDownloader.Core.Interfaces;

var urls = new[]
{
    "https://www.entaingroup.com/",
    "https://www.microsoft.com/en-us/",
    "https://www.github.com/",
    "https://www.wikipedia.org/",
    "https://owasp.org/",
    "https://www.opengroup.org/togaf",
};

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddPageDownloadServices();
builder.Services.AddHttpClient();

using IHost host = builder.Build();

var downloader = host.Services.GetRequiredService<IPageDownloadService>();

await foreach (var savedPage in downloader.DownloadPagesAsync(urls))
{
    Console.WriteLine($"Downloaded {savedPage.OriginalUrl} to {savedPage.SavedHtmlPath}");
}

Console.WriteLine("Downloading process is done");

await host.RunAsync();



