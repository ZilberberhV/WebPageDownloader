using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using WebPageDownloader.Code.Models;
using WebPageDownloader.Core.Interfaces;

namespace WebPageDownloader.Core.Services;

internal partial class PageDownloadService : IPageDownloadService
{
    private readonly IFileSystemAccessor _fileSystemAccessor;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;

    public PageDownloadService(
        IFileSystemAccessor fileSystemAccessor,
        IHttpClientFactory httpClientFactory,
        ILogger<PageDownloadService> logger)
    {
        _fileSystemAccessor = fileSystemAccessor;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// Downloads web pages from the specified URLs and saves them to the specified folder.
    /// </summary>
    /// <remarks>This method downloads each web page asynchronously and yields the results as they become
    /// available.  The method supports cancellation via the <paramref name="cancellationToken"/> parameter.</remarks>
    /// <param name="urls">A collection of URLs representing the web pages to download.</param>
    /// <param name="downloadsRootFolder">The root folder where the downloaded pages will be saved.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The operation is canceled if the token is triggered.</param>
    /// <returns>An asynchronous stream of <see cref="SavedPage"/> objects, each representing a successfully downloaded and saved
    /// web page.</returns>
    public async IAsyncEnumerable<SavedPage> DownloadPagesAsync(IEnumerable<string> urls, string downloadsRootFolder, [EnumeratorCancellation]CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting asynchronous downloading of web pages");

        var downloadTasks = urls.Select(url => DownloadPageAsync(url, downloadsRootFolder, cancellationToken));

        await foreach (var savedPage in Task.WhenEach(downloadTasks))
        {
            var result = await savedPage;
            if (string.IsNullOrEmpty(result.Error))
            {
                _logger.LogInformation("Successfully downloaded and saved page from {Url} to {Path}", result.OriginalUrl, result.SavedHtmlPath);
            }
            else
            {
                _logger.LogError("Failed to download page from {Url}. Error: {ErrorMessage}", result.OriginalUrl, result.Error);
            }

            yield return await savedPage;
        }
    }

    /// <summary>
    /// Downloads the content of all specified URLs and saves them as pages in the specified root folder.
    /// </summary>
    /// <remarks>Each URL in the <paramref name="urls"/> collection is downloaded concurrently. The method
    /// will save the content of each URL to a file in the specified <paramref name="downloadsRootFolder"/> 
    /// as well as all referenced resources. If the folder does not exist, it will be created.</remarks>
    /// <param name="urls">A collection of URLs to download.</param>
    /// <param name="downloadsRootFolder">The root folder where the downloaded pages will be saved.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of <see
    /// cref="SavedPage"/> objects representing the downloaded pages.</returns>
    public async Task<IEnumerable<SavedPage>> DownloadAllPagesAsync(IEnumerable<string> urls, string downloadsRootFolder, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting bulk downloading of web pages");

        var downloadTasks = urls.Select(url => DownloadPageAsync(url, downloadsRootFolder, cancellationToken));

        var results = await Task.WhenAll(downloadTasks);

        _logger.LogInformation("Completed bulk downloading of web pages");

        return results;
    }

    private async Task<SavedPage> DownloadPageAsync(string url, string downloadRootFolder, CancellationToken cancellationToken)
    {
        try
        {
            var uri = new Uri(url);
            var hostFolder = Path.Combine(downloadRootFolder, uri.Host.Replace(".", "_"));

            var httpClient = _httpClientFactory.CreateClient();
            var html = await httpClient.GetStringAsync(url, cancellationToken);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var resourceUrls = HtmlPageProcessor.ProcessHtmlPageReferences(doc, uri).ToList();

            _fileSystemAccessor.CreateFolder(hostFolder);
            var downloadTasks = resourceUrls.Select(async resourceUri =>
            {
                try
                {
                    var resourceBytes = await httpClient.GetByteArrayAsync(resourceUri, cancellationToken);
                    var fileName = resourceUri?.LocalPath.TrimStart('/');
                    if (string.IsNullOrWhiteSpace(fileName)) fileName = "resource";
                    var filePath = Path.Combine(hostFolder, fileName);
                    _fileSystemAccessor.CreateFolder(Path.GetDirectoryName(filePath)!);
                    await _fileSystemAccessor.SaveFileAsync(filePath, resourceBytes);
                }
                catch (Exception)
                { }
            });

            await Task.WhenAll(downloadTasks);

            var htmlPath = Path.Combine(hostFolder, "index.html");

            using var outputStream = _fileSystemAccessor.GetFileWriteStream(htmlPath);
            doc.Save(outputStream);

            return new SavedPage(url, htmlPath, null);
        }
        catch (Exception ex)
        {
            return new SavedPage(url, null, ex.Message);
        }
    }
}
