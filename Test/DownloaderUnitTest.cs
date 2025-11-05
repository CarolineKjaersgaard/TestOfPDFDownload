using System;
using System.Data;
using PDFDownload;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;


namespace Test;

// Unit tests the downloader class' DownloadFile method 
public class DownloaderUnitTest
{

    private string _file = "test.pdf"; // The name of valid pdf files for testing purposes
    private string _rapport = "rapport.txt"; // Valid status rapport for testing purposes

    // Mimics a working http client that checks if the given url contains a reference to a pdf or zip 
    // else responds with bad request
    // Returns: mock http client
    private HttpClient GetMockClient()
    {
        var mockHandler = new Mock<HttpMessageHandler>();

        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
                {
                    if (request.RequestUri.AbsoluteUri.Contains("pdf"))
                    {
                        return new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.OK,
                            Content = new ByteArrayContent(new byte[] { 1, 2, 3 })
                            {
                                Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf") }
                            }
                        };
                    }
                    else if (request.RequestUri.AbsoluteUri.Contains("zip"))
                    {
                        return new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.OK,
                            Content = new ByteArrayContent(new byte[] { 1, 2, 3 })
                            {
                                Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/zip") }
                            }
                        };
                    }
                    else
                    {
                        return new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.BadRequest,
                            Content = new StringContent("Bad Request")
                        };
                    }
                });

        return new HttpClient(mockHandler.Object);
    }

    // Mimics a http client with no internet connection
    // Returns: mock http client
    private HttpClient GetFailedConnectionMockClient()
    {
        var mockHandler = new Mock<HttpMessageHandler>();

        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new HttpRequestException("No internet connection"));

        return new HttpClient(mockHandler.Object);
    }

    // Cleans up test envirorment 
    private void Cleanup(string downloadPath, string fileName)
    {
        string filePath = Path.Combine(downloadPath, fileName);
        if (File.Exists(filePath))
            File.Delete(filePath);
    }

    // Creates a temporary folder for downloaded files and calls cleanup method
    // Returns: path to temporary folder
    private string MakeDownloadPath()
    {
        string downloadPath = Path.GetTempPath();
        Cleanup(downloadPath, _file);
        Cleanup(downloadPath, _rapport);
        return downloadPath;
    }

    // Tests if download file method writes a pdf file based on valid url containing pdf
    [Fact]
    public async Task DownloadFile_ValidPdfInFirstUrl_WritesFile()
    {
        Downloader downloader = new Downloader();
        string downloadPath = MakeDownloadPath();
        string rapportPath = Path.Combine(downloadPath, _rapport);
        await downloader.DownloadFile(GetMockClient(), "http://example.com/test.pdf", downloadPath, rapportPath, _file);

        Assert.True(File.Exists(Path.Combine(downloadPath, _file)));
    }

    // Tests if download file method writes a sucessful entry in status rapport given valid url containing pdf
    [Fact]
    public async Task DownloadFile_ValidPdfInFirstUrl_WritesReport()
    {
        Downloader downloader = new Downloader();
        string downloadPath = MakeDownloadPath();
        string rapportPath = Path.Combine(downloadPath, _rapport);
        await downloader.DownloadFile(GetMockClient(), "http://example.com/test.pdf", downloadPath, rapportPath, _file);

        Assert.Contains("Successfully downloaded", File.ReadAllText(rapportPath));
    }

    // Tests if download file method adds a pdf to download folder given invalid first url and valid second url
    [Fact]
    public async Task DownloadFile_OnlyValidPdfInSecondUrl_WritesFile()
    {
        Downloader downloader = new Downloader();
        string downloadPath = MakeDownloadPath();
        string rapportPath = Path.Combine(downloadPath, _rapport);
        await downloader.DownloadFile(GetMockClient(), "http://example.com/test", downloadPath, rapportPath, _file, "http://example.com/test.pdf");

        Assert.True(File.Exists(Path.Combine(downloadPath, "test.pdf")));
    }
    // Tests if download file method writes a sucessful status rapport update given invalid first url and valid second url
    [Fact]
    public async Task DownloadFile_OnlyValidPdfInSecondUrl_WritesRepport()
    {
        Downloader downloader = new Downloader();
        string downloadPath = MakeDownloadPath();
        string rapportPath = Path.Combine(downloadPath, _rapport);
        await downloader.DownloadFile(GetMockClient(), "http://example.com/test", downloadPath, rapportPath, _file, "http://example.com/test.pdf");

        Assert.Contains("Successfully downloaded", File.ReadAllText(rapportPath));
    }

    // Tests if download file method does not add a pdf to download folder given both urls are invalid
    [Fact]
    public async Task DownloadFile_InvalidPdfInBothUrls_WritesNoFile()
    {
        Downloader downloader = new Downloader();
        string downloadPath = MakeDownloadPath();
        string rapportPath = Path.Combine(downloadPath, _rapport);
        await downloader.DownloadFile(GetMockClient(), "http://example.com/test", downloadPath, rapportPath, _file, "http://example.com/test");

        Assert.False(File.Exists(Path.Combine(downloadPath, _file)));
    }

    // Tests if download file method writes a failed status rapport given both urls are invalid
    [Fact]
    public async Task DownloadFile_InvalidPdfInBothUrls_WritesNoRepport()
    {
        Downloader downloader = new Downloader();
        string downloadPath = MakeDownloadPath();
        string rapportPath = Path.Combine(downloadPath, _rapport);
        await downloader.DownloadFile(GetMockClient(), "http://example.com/test", downloadPath, rapportPath, _file, "http://example.com/test");

        Assert.Contains(_file + ": Failed to download.", File.ReadAllText(rapportPath));
    }

    // Tests if download file method does not add a file to download folder given url containing zip
    [Fact]
    public async Task DownloadFile_ZipInUrl_WritesNoFile()
    {
        Downloader downloader = new Downloader();
        string downloadPath = MakeDownloadPath();
        string rapportPath = Path.Combine(downloadPath, _rapport);
        await downloader.DownloadFile(GetMockClient(), "http://example.com/test.zip", downloadPath, rapportPath, _file);

        Assert.False(File.Exists(Path.Combine(downloadPath, _file)));
    }

    // Tests if download file method writes a not sucessful entry in status rapport given url containing zip
    [Fact]
    public async Task DownloadFile_ZipPdfInUrl_WritesNoReport()
    {
        Downloader downloader = new Downloader();
        string downloadPath = MakeDownloadPath();
        string rapportPath = Path.Combine(downloadPath, _rapport);
        await downloader.DownloadFile(GetMockClient(), "http://example.com/test.zip", downloadPath, rapportPath, _file);

        Assert.Contains(_file + ": Failed to download.", File.ReadAllText(rapportPath));
    }

    // Tests if download file method are able to handle missing internet connection
    [Fact]
    public async Task DownloadFile_NoInternet_Handles()
    {
        Downloader downloader = new Downloader();
        string downloadPath = MakeDownloadPath();
        string rapportPath = Path.Combine(downloadPath, _rapport);
        await downloader.DownloadFile(GetFailedConnectionMockClient(), "http://example.com/test.pdf", downloadPath, rapportPath, _file);

        Assert.False(File.Exists(Path.Combine(downloadPath, _file)));
    }

}