namespace WebPageDownloader.Core.Interfaces;

public interface IFileSystemAccessor
{
    void CreateFolder(string folderPath);
    Task SaveFileAsync(string filePath, byte[] content);
    Stream GetFileWriteStream(string filePath);
}
