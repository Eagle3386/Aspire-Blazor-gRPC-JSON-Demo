using static Google.Protobuf.WellKnownTypes.TimeExtensions;

namespace ProductListing.BenchmarkService;

/// <summary>Represents a factory which creates <see cref="WeatherForecast" />s.</summary>
public static class WeatherFactory
{
  private static readonly string[] Summaries =
    ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

  /// <summary>Creates an <see cref="Array" /> of <see cref="WeatherForecast" />s with random data.</summary>
  /// <param name="count">
  /// The <see langword="int" /> number of <see cref="WeatherForecast" />s to generate. Must be between 0 &amp; 999,999 inclusive.
  /// </param>
  /// <returns>
  /// An <see cref="Array" /> of <see cref="WeatherForecast" />s or <see cref="Enumerable.Empty{TResult}" />, if
  /// <paramref name="count" /> is <c>0</c>.
  /// </returns>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="count" /> is negative or exceeds 999,999.</exception>
  public static TForecast[] Create<TForecast>(int count) where TForecast : class
  {
    ArgumentOutOfRangeException.ThrowIfNegative(count);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(count, 999_999);

    return Enumerable.Range(1, count).Select(CreateForecast).ToArray();

    static TForecast CreateForecast(int index) =>
      (typeof(TForecast) == typeof(Protos.WeatherForecast)
        ? new Protos.WeatherForecast
        {
          Date = GetDate(index).ToTimestamp(),
          Summary = GetSummary(),
          TemperatureC = GetTemperature()
        } as TForecast
        : new WeatherForecast(DateOnly.FromDateTime(GetDate(index)), GetTemperature(), GetSummary()) as TForecast)!;

    static DateTime GetDate(int offset) => DateTime.Now.AddDays(offset);
    static string GetSummary() => Summaries[Random.Shared.Next(Summaries.Length)];
    static int GetTemperature() => Random.Shared.Next(-20, 55);
  }
}

/// <summary>Represents a weather forecast.</summary>
/// <param name="Date">The forecast’s <see cref="DateOnly" /> date.</param>
/// <param name="TemperatureC">The forecasted temperature in degree Celsius.</param>
/// <param name="Summary">The forecast’s summary.</param>
public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
  /// <summary>Gets the forecast’s temperature, converted to degree Fahrenheit.</summary>
  public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
