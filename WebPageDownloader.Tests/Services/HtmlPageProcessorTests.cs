using HtmlAgilityPack;
using WebPageDownloader.Core.Services;

namespace WebPageDownloader.Tests.Services;

public class HtmlPageProcessorTests
{
    [Fact]
    public void ProcessHtmlPageReferences_ExtractsImageScriptAndLinkReferences()
    {
        // Arrange
        var html = @"<html><head><link href='style.css'></head><body><img src='img.png'><script src='app.js'></script></body></html>";
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        var baseUri = new Uri("http://example.com/");

        // Act
        var uris = HtmlPageProcessor.ProcessHtmlPageReferences(doc, baseUri).ToList();

        // Assert
        Assert.Contains(new Uri("http://example.com/style.css"), uris);
        Assert.Contains(new Uri("http://example.com/img.png"), uris);
        Assert.Contains(new Uri("http://example.com/app.js"), uris);
    }

    [Fact]
    public void ProcessHtmlPageReferences_ExtractsBackgroundImageFromStyle()
    {
        // Arrange
        var html = @"<div style='background-image: url(bg.jpg)'></div>";
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        var baseUri = new Uri("http://example.com/");

        // Act
        var uris = HtmlPageProcessor.ProcessHtmlPageReferences(doc, baseUri).ToList();

        // Assert
        Assert.Contains(new Uri("http://example.com/bg.jpg"), uris);
    }

    [Fact]
    public void ProcessHtmlPageReferences_ResolvesRelativeAndAbsoluteUrls()
    {
        // Arrange
        var html = @"<img src='/img1.png'><img src='img2.png'><img src='http://other.com/img3.png'>";
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        var baseUri = new Uri("http://example.com/sub/");

        // Act
        var uris = HtmlPageProcessor.ProcessHtmlPageReferences(doc, baseUri).ToList();

        // Assert
        Assert.Contains(new Uri("http://example.com/img1.png"), uris); // leading slash
        Assert.Contains(new Uri("http://example.com/sub/img2.png"), uris); // relative
        Assert.Contains(new Uri("http://other.com/img3.png"), uris); // absolute
    }

    [Fact]
    public void ProcessHtmlPageReferences_AnchorTagsAreUpdatedToAbsolute()
    {
        // Arrange
        var html = @"<a href='page.html'>link</a>";
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        var baseUri = new Uri("http://example.com/");

        // Act
        HtmlPageProcessor.ProcessHtmlPageReferences(doc, baseUri).ToList();
        var anchor = doc.DocumentNode.SelectSingleNode("//a");
        var href = anchor.GetAttributeValue("href", null);

        // Assert
        Assert.Equal("http://example.com/page.html", href);
    }
}
