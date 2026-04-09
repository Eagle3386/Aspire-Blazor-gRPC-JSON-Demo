using Grpc.Net.Client;

namespace ProductListing.Web;

public class BenchmarkClient(HttpClient client)
{
  //private Weather.WeatherForecasts.WeatherForecastsClient grpcWebClient = CreateClient(GrpcWebMode.GrpcWeb);
  //private Weather.WeatherForecasts.WeatherForecastsClient grpcWebTextClient = CreateClient(GrpcWebMode.GrpcWebText);

  public async Task<WeatherForecast[]> GetWeatherAsync(int limit, CancellationToken token = default)
  {
    List<WeatherForecast> forecasts = [];

    await foreach (var forecast in client.GetFromJsonAsAsyncEnumerable<WeatherForecast>($"/forecast/{limit}", token))
    {
      if (forecasts.Count >= limit)
      {
        break;
      }

      if (forecast is not null)
      {
        forecasts.Add(forecast);
      }
    }

    return [.. forecasts];
  }
}

public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
  public int TemperatureF { get; } = GetFahrenheit(TemperatureC);

  public static int GetFahrenheit(int degreeCelcius) => 32 + (int)(degreeCelcius / 0.5556);
}


//  private Weather.WeatherForecasts.WeatherForecastsClient CreateClient(GrpcWebMode grpcWebMode)
//  {
//    var channel = GrpcChannel.ForAddress(Env.BaseAddress, new GrpcChannelOptions
//    {
//      HttpHandler = new GrpcWebHandler(grpcWebMode, httpHandler),
//      MaxReceiveMessageSize = null
//    });

//    return new Weather.WeatherForecasts.WeatherForecastsClient(channel);
//  }

//  private async Task Run()
//  {
//    if (returnCount is < 0 or > 999999)
//    {
//      error = "Return count must be between 0 and 999999";

//      return;
//    }

//    try
//    {
//      error = null;
//      grpcForecasts = null;
//      jsonForecasts = null;
//      virtualizeUi = shouldVirtualizeUi;
//      retrievalStopwatch.Start();
//      switch (benchmarkType)
//      {
//        case BenchmarkType.Json:
//          jsonForecasts = await WeatherApi.GetWeatherAsync(returnCount);
//          break;

//        case BenchmarkType.GrpcWeb:
//          grpcForecasts = (await grpcWebClient.GetWeatherForecastsAsync(new Weather.GetWeatherForecastsRequest { ReturnCount = returnCount })).Forecasts;
//          break;

//        case BenchmarkType.GrpcWebText:
//          grpcForecasts = (await grpcWebTextClient.GetWeatherForecastsAsync(new Weather.GetWeatherForecastsRequest { ReturnCount = returnCount })).Forecasts;
//          break;

//        default:
//          throw new ArgumentOutOfRangeException();
//      }

//      bytesRead = httpHandler.BytesRead;
//      retrievalSeconds = httpHandler.RetrievalElapsed.TotalSeconds;
//      totalSeconds = retrievalStopwatch.Elapsed.TotalSeconds;
//    }
//    catch (Exception exception)
//    {
//      error = $"Error fetching data: {exception.GetBaseException().ToString()}";
//    }
//    finally
//    {
//      retrievalStopwatch.Reset();
//    }

//    renderingStopwatch.Reset();
//    renderingStopwatch.Start();
//  }


public class BenchmarkClient2(HttpClient client)
{
  //private Weather.WeatherForecasts.WeatherForecastsClient grpcWebClient = CreateClient(GrpcWebMode.GrpcWeb);
  //private Weather.WeatherForecasts.WeatherForecastsClient grpcWebTextClient = CreateClient(GrpcWebMode.GrpcWebText);

  public async Task<WeatherForecast[]> GetWeatherAsync(int limit, CancellationToken token = default)
  {
    List<WeatherForecast> forecasts = [];

    await foreach (var forecast in client.GetFromJsonAsAsyncEnumerable<WeatherForecast>($"/forecast/{limit}", token))
    {
      if (forecasts.Count >= limit)
      {
        break;
      }

      if (forecast is not null)
      {
        forecasts.Add(forecast);
      }
    }

    return [.. forecasts];
  }
}