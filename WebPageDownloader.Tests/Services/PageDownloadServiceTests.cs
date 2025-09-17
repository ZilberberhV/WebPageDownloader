using Microsoft.Extensions.Logging;
using Moq;
using WebPageDownloader.Code.Models;
using WebPageDownloader.Core.Interfaces;
using WebPageDownloader.Core.Services;

namespace WebPageDownloader.Tests.Services;

public class PageDownloadServiceTests
{
    private readonly Mock<IFileSystemAccessor> _fileSystemAccessorMock = new();
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();
    private readonly Mock<ILogger<PageDownloadService>> _loggerMock = new();

    private PageDownloadService CreateService(HttpClient? httpClient = null)
    {
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient ?? new HttpClient(new MockHttpMessageHandler()));
        return new PageDownloadService(_fileSystemAccessorMock.Object, _httpClientFactoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task DownloadAllPagesAsync_ReturnsSavedPages_ForValidUrls()
    {
        // Arrange
        var urls = new[] { "http://test.com" };
        var downloadsRootFolder = "downloads";
        var html = "<html><body>Hello</body></html>";
        var httpClient = new HttpClient(new MockHttpMessageHandler(html));
        var service = CreateService(httpClient);

        _fileSystemAccessorMock.Setup(f => f.CreateFolder(It.IsAny<string>()));
        _fileSystemAccessorMock.Setup(f => f.GetFileWriteStream(It.IsAny<string>())).Returns(new MemoryStream());
        _fileSystemAccessorMock.Setup(f => f.SaveFileAsync(It.IsAny<string>(), It.IsAny<byte[]>())).Returns(Task.CompletedTask);

        // Act
        var result = await service.DownloadAllPagesAsync(urls, downloadsRootFolder);

        // Assert
        var savedPage = Assert.Single(result);
        Assert.Equal("http://test.com", savedPage.OriginalUrl);
        Assert.NotNull(savedPage.SavedHtmlPath);
        Assert.Equal("downloads\\test_com\\index.html", savedPage.SavedHtmlPath);
        Assert.Null(savedPage.Error);
    }

    [Fact]
    public async Task DownloadAllPagesAsync_ReturnsError_WhenHttpFails()
    {
        // Arrange
        var urls = new[] { "http://fail.com" };
        var downloadsRootFolder = "downloads";
        var httpClient = new HttpClient(new MockHttpMessageHandler(throwOnGet:true));
        var service = CreateService(httpClient);

        // Act
        var result = await service.DownloadAllPagesAsync(urls, downloadsRootFolder);

        // Assert
        var savedPage = Assert.Single(result);
        Assert.Equal("http://fail.com", savedPage.OriginalUrl);
        Assert.Null(savedPage.SavedHtmlPath);
        Assert.NotNull(savedPage.Error);
    }

    [Fact]
    public async Task DownloadPagesAsync_YieldsSavedPages()
    {
        // Arrange
        var urls = new[] { "http://test.com" };
        var downloadsRootFolder = "downloads";
        var html = "<html><body>Hello</body></html>";
        var httpClient = new HttpClient(new MockHttpMessageHandler(html));
        var service = CreateService(httpClient);

        _fileSystemAccessorMock.Setup(f => f.CreateFolder(It.IsAny<string>()));
        _fileSystemAccessorMock.Setup(f => f.GetFileWriteStream(It.IsAny<string>())).Returns(new MemoryStream());
        _fileSystemAccessorMock.Setup(f => f.SaveFileAsync(It.IsAny<string>(), It.IsAny<byte[]>())).Returns(Task.CompletedTask);

        // Act
        var results = new List<SavedPage>();
        await foreach (var page in service.DownloadPagesAsync(urls, downloadsRootFolder))
        {
            results.Add(page);
        }

        // Assert
        var savedPage = Assert.Single(results);
        Assert.Equal("http://test.com", savedPage.OriginalUrl);
        Assert.NotNull(savedPage.SavedHtmlPath);
        Assert.Equal("downloads\\test_com\\index.html", savedPage.SavedHtmlPath);
        Assert.Null(savedPage.Error);
    }

    private class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _html;
        private readonly bool _throwOnGet;
        public MockHttpMessageHandler(string html = "<html></html>", bool throwOnGet = false)
        {
            _html = html;
            _throwOnGet = throwOnGet;
        }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_throwOnGet)
                throw new HttpRequestException("Mocked failure");
            if (request.Method == HttpMethod.Get)
            {
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(_html)
                });
            }
            throw new NotImplementedException();
        }
    }
}
