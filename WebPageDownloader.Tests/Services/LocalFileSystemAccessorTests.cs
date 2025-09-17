using System.Text;
using WebPageDownloader.Core.Services;

namespace WebPageDownloader.Tests.Services;

public class LocalFileSystemAccessorTests
{
    [Fact]
    public void CreateFolder_CreatesDirectory()
    {
        // Arrange
        var accessor = new LocalFileSystemAccessor();
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        try
        {
            // Act
            accessor.CreateFolder(tempDir);

            // Assert
            Assert.True(Directory.Exists(tempDir));
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task SaveFileAsync_WritesBytesToFile()
    {
        // Arrange
        var accessor = new LocalFileSystemAccessor();
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var filePath = Path.Combine(tempDir, "testfile.txt");
        var content = Encoding.UTF8.GetBytes("Hello World");

        try
        {
            // Act
            await accessor.SaveFileAsync(filePath, content);

            // Assert
            Assert.True(File.Exists(filePath));
            var fileContent = await File.ReadAllBytesAsync(filePath);
            Assert.Equal(content, fileContent);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void GetFileWriteStream_CreatesFileAndWritesContent()
    {
        // Arrange
        var accessor = new LocalFileSystemAccessor();
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var filePath = Path.Combine(tempDir, "streamfile.txt");
        var content = "Stream Content";

        try
        {
            // Act
            using (var stream = accessor.GetFileWriteStream(filePath))
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(content);
            }

            // Assert
            Assert.True(File.Exists(filePath));
            var fileContent = File.ReadAllText(filePath);
            Assert.Equal(content, fileContent);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }
}
