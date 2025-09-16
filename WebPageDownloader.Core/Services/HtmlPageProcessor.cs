using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace WebPageDownloader.Core.Services;

internal partial class HtmlPageProcessor
{
    [GeneratedRegex(@"background-image\s*:\s*url\((['""]?)([^'""\)]+)\1\)", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex BackgroundImageRegex();

    /// <summary>
    /// Extracts and processes all resource URLs and references from an HTML document, resolving them to absolute URIs and relative with respect to the file system.
    /// </summary>
    /// <remarks>The method processes the following types of references in the HTML document: <list
    /// type="bullet"> <item> <description>Anchor tags (<c>&lt;a href="..."&gt;</c>)</description> </item> <item>
    /// <description>Image sources (<c>&lt;img src="..."&gt;</c>), script sources (<c>&lt;script src="..."&gt;</c>), and
    /// link references (<c>&lt;link href="..."&gt;</c>)</description> </item> <item> <description>Background image URLs
    /// specified in inline <c>style</c> attributes</description> </item> </list> Relative URLs are resolved using the
    /// provided <paramref name="baseUri"/>. Duplicate URLs are removed from the result.</remarks>
    /// <param name="document">The HTML document to analyze. Must not be <see langword="null"/>.</param>
    /// <param name="baseUri">The base URI used to resolve relative references. Must not be <see langword="null"/>.</param>
    /// <returns>An enumerable collection of distinct absolute URIs representing the resources referenced in the HTML document.
    /// This includes URLs from image sources, script sources, link references, and background image styles.
    /// </returns>
    internal static IEnumerable<Uri> ProcessHtmlPageReferences(HtmlDocument document, Uri baseUri)
    {
        var anchorNodes = document.DocumentNode.SelectNodes("//a[@href]") ?? new HtmlNodeCollection(null);
        foreach (var node in anchorNodes)
        {
            AnchorAbsoluteUri(node, baseUri);
        }

        var resourceNodes = document.DocumentNode
            .SelectNodes("//img[@src]|//script[@src]|//link[@href]") ?? new HtmlNodeCollection(null);

        var resourceUrls = resourceNodes
            .Select(node => GetRelativeReference(node, baseUri))
            .Where(u => u != null)
            .ToList();

        var styleNodes = document.DocumentNode.SelectNodes("//*[@style]") ?? new HtmlNodeCollection(null);
        var styleUrlRegex = BackgroundImageRegex();

        foreach (var node in styleNodes)
        {
            var resourceUri = ProcessBackgroundImageStyle(node, baseUri);

            if (resourceUri != null)
                resourceUrls.Add(resourceUri);
        }

        return resourceUrls.Distinct();
    }

    private static void AnchorAbsoluteUri(HtmlNode node, Uri baseUri)
    {
        var href = node.GetAttributeValue("href", null);
        if (!string.IsNullOrEmpty(href) && !Uri.IsWellFormedUriString(href, UriKind.Absolute))
        {
            var absoluteUri = new Uri(baseUri, href);
            node.SetAttributeValue("href", absoluteUri.ToString());
        }
    }

    private static Uri? GetRelativeReference(HtmlNode node, Uri baseUri)
    {
        string attrName = node.Name == "link" ? "href" : "src";
        var originalRef = node.GetAttributeValue(attrName, null);
        if (!string.IsNullOrEmpty(originalRef) && originalRef.StartsWith('/'))
        {
            var trimmedRef = originalRef.TrimStart('/');
            node.SetAttributeValue(attrName, trimmedRef);
            return new Uri(baseUri, originalRef);
        }
        else if (!string.IsNullOrEmpty(originalRef))
        {
            return new Uri(baseUri, originalRef);
        }
        return null;
    }

    private static Uri? ProcessBackgroundImageStyle(HtmlNode node, Uri baseUri)
    {
        var styleUrlRegex = BackgroundImageRegex();

        var style = node.GetAttributeValue("style", null);
        if (string.IsNullOrEmpty(style)) return null;

        var matches = styleUrlRegex.Matches(style);
        foreach (Match match in matches)
        {
            var originalRef = match.Groups[2].Value;
            if (!string.IsNullOrEmpty(originalRef))
            {
                var trimmedRef = originalRef.StartsWith('/') ? originalRef.TrimStart('/') : originalRef;
                style = style.Replace(originalRef, trimmedRef);
                node.SetAttributeValue("style", style);

                var resourceUri = new Uri(baseUri, originalRef);
                return resourceUri;
            }
        }

        return null;
    }
}
