using Grpc.Core;
using ProductListing.Protos;

namespace ProductListing.BenchmarkService;

/// <summary>Represents the gRPC implementation of the REST weather service located at <c>/forecast/{count}</c>.</summary>
public class WeatherService : WeatherForecasts.WeatherForecastsBase
{
  /// <summary>Gets the weather forecasts for given <paramref name="request" />.</summary>
  /// <param name="request">The <see cref="GetWeatherForecastsRequest" /> to serve.</param>
  /// <param name="context">The <see cref="ServerCallContext" /> to use.</param>
  /// <returns>An <see langword="await" /> <see cref="Task" /> for retrieval of weather forecasts.</returns>
  public override Task<GetWeatherForecastsResponse> GetWeatherForecasts(GetWeatherForecastsRequest request, ServerCallContext context) =>
    Task.FromResult(new GetWeatherForecastsResponse { Forecasts = { WeatherFactory.Create<Protos.WeatherForecast>(request.ReturnCount) } });
}
