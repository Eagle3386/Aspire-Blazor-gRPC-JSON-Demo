using Microsoft.Extensions.Logging;

namespace ProductListing.Tests;

public class WebTests
{
  private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

  [Fact]
  public async Task GetApiResourceRootReturnsOkStatusCode()
  {
    // Arrange
    var cancellationToken = TestContext.Current.CancellationToken;

    var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.ProductListing_AppHost>(cancellationToken);
    appHost.Services.AddLogging(logging =>
    {
      logging.SetMinimumLevel(LogLevel.Debug);
      logging.AddFilter(appHost.Environment.ApplicationName, LogLevel.Debug);
      logging.AddFilter("Aspire.", LogLevel.Debug);
      // Output logs to xUnit.net's ITestOutputHelper by adding a package from https://www.nuget.org/packages?q=xunit+logging
    });
    appHost.Services.ConfigureHttpClientDefaults(builder => builder.AddStandardResilienceHandler());

    await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
    await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

    // Act
    var client = app.CreateHttpClient("api");
    await app.ResourceNotifications.WaitForResourceHealthyAsync("api", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
    var response = await client.GetAsync("/", cancellationToken);

    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }

  [Fact]
  public async Task GetWebResourceRootReturnsOkStatusCode()
  {
    // Arrange
    var cancellationToken = TestContext.Current.CancellationToken;

    var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.ProductListing_AppHost>(cancellationToken);
    appHost.Services.AddLogging(logging =>
    {
      logging.SetMinimumLevel(LogLevel.Debug);
      logging.AddFilter(appHost.Environment.ApplicationName, LogLevel.Debug);
      logging.AddFilter("Aspire.", LogLevel.Debug);
      // Output logs to xUnit.net's ITestOutputHelper by adding a package from https://www.nuget.org/packages?q=xunit+logging
    });
    appHost.Services.ConfigureHttpClientDefaults(builder => builder.AddStandardResilienceHandler());

    await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
    await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

    // Act
    var client = app.CreateHttpClient("web");
    await app.ResourceNotifications.WaitForResourceHealthyAsync("web", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
    var response = await client.GetAsync("/", cancellationToken);

    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }
}
