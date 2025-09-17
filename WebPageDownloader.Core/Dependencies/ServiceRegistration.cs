using Microsoft.Extensions.DependencyInjection;
using WebPageDownloader.Core.Interfaces;
using WebPageDownloader.Core.Services;

namespace WebPageDownloader.Core.Dependencies;

public static class ServiceRegistration
{
    public static void AddPageDownloadServices(this IServiceCollection services)
    {
        services.AddScoped<IPageDownloadService, PageDownloadService>();
    }

    public static void AddLocalFileSystemAccessor(this IServiceCollection services)
    {
        services.AddScoped<IFileSystemAccessor, LocalFileSystemAccessor>();
    }
}
