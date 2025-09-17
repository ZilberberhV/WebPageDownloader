# WebPageDownloader Solution

This solution is centered around a class library that encapsulates the logic for downloading web pages, with a console application provided as a supporting tool to run the process from the command line.

## Projects

### 1. WebPageDownloader.Core (Class Library)
- **Purpose:**
  - Implements the core logic for downloading web pages, handling HTTP requests, and saving content.
  - Designed for reuse, extensibility, and easy testing.
- **Usage:**
  - Reference this library from other .NET projects to utilize its web downloading functionality.
  - Contains all business logic and is independent of any user interface.

### 2. WebPageDownloader (Console Application)
- **Purpose:**
  - Provides a command-line interface to invoke the functionality of the core library.
  - Acts as a supporting application to demonstrate and run the download process interactively.
- **Usage:**
  - Run the application from the command line, providing optionally directory to download to and the list of URLs of the web pages you wish to download.
  - Example:
    ```sh
    dotnet run --project WebPageDownloader -- DownloadFolder https://example.com
    ```

### 3. WebPageDownloader.Tests (Test Project)
- **Purpose:**
  - Contains unit tests for the core library to ensure reliability and correctness.

## Requirements
- .NET 9 SDK

## Getting Started
1. Clone the repository.
2. Restore dependencies:
   ```sh
   dotnet restore
   ```
3. Build the solution:
   ```sh
   dotnet build
   ```
4. Run the console app as shown above.
