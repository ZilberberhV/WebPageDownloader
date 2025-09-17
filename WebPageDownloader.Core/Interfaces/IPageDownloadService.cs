using WebPageDownloader.Code.Models;

namespace WebPageDownloader.Core.Interfaces;

public interface IPageDownloadService
{
    Task<IEnumerable<SavedPage>> DownloadAllPagesAsync(IEnumerable<string> urls, string downloadsRootFolder, CancellationToken cancellationToken = default);
    IAsyncEnumerable<SavedPage> DownloadPagesAsync(IEnumerable<string> urls, string downloadsRootFolder, CancellationToken cancellationToken = default);
}
