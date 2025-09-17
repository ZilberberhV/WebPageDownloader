using WebPageDownloader.Core.Interfaces;

namespace WebPageDownloader.Core.Services;

internal class LocalFileSystemAccessor : IFileSystemAccessor
{
    public void CreateFolder(string folderPath)
    {
        Directory.CreateDirectory(folderPath);
    }

    public Task SaveFileAsync(string filePath, byte[] content)
    {
        return File.WriteAllBytesAsync(filePath, content);
    }

    public Stream GetFileWriteStream(string filePath)
    {
        return new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
    }
}
