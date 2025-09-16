using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebPageDownloader.Code.Models;

namespace WebPageDownloader.Core.Interfaces
{
    public interface IPageDownloadService
    {
        Task<IEnumerable<SavedPage>> DownloadAllPagesAsync(IEnumerable<string> urls, string downloadsRootFolder = "Downloads", CancellationToken cancellationToken = default);
        IAsyncEnumerable<SavedPage> DownloadPagesAsync(IEnumerable<string> urls, string downloadsRootFolder = "Downloads", CancellationToken cancellationToken = default);
    }
}
