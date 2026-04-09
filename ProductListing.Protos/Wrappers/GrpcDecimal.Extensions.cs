namespace ProductListing.Protos;

// NOTE: This code is greatly inspired by .NET’s architecture eBook "gRPC for WCF developers" (p. 16/17).
//       For details, see https://docs.microsoft.com/en-us/dotnet/architecture/grpc-for-wcf-developers/

// Represents a gRPC wrapper for .NET’s "decimal" value type which separates it into an integer & the remainder into a fractional
// part to minimize rounding errors common to "float" & "double".
// Example: 12345.6789 => { integer = 12345, fractional = 678900000 }

/// <inheritdoc />
public partial class GrpcDecimal
{
  /// <summary>
  /// Initializes a new instance of the <see cref="GrpcDecimal" /> class with given <paramref name="integer" /> &amp;
  /// <paramref name="fractional" /> parts.
  /// </summary>
  /// <param name="integer">The <see langword="long" /> integer part of the <see langword="decimal" /> value.</param>
  /// <param name="fractional">The <see langword="int" /> fractional part of the <see langword="decimal" /> value.</param>
  public GrpcDecimal(long integer, int fractional)
  {
    Integer = integer;
    Fractional = fractional;
  }

  public static bool operator <(decimal left, GrpcDecimal right) =>
    right is not null && GrpcDecimalExtensions.ToDecimal(right) > left;

  public static bool operator <(GrpcDecimal left, decimal right) =>
    left is not null && GrpcDecimalExtensions.ToDecimal(left) < right;

  public static bool operator >(decimal left, GrpcDecimal right) =>
    right is not null && GrpcDecimalExtensions.ToDecimal(right) < left;

  public static bool operator >(GrpcDecimal left, decimal right) =>
    left is not null && GrpcDecimalExtensions.ToDecimal(left) > right;

  public static bool operator <=(decimal left, GrpcDecimal right) =>
    right is not null && GrpcDecimalExtensions.ToDecimal(right) <= left;

  public static bool operator <=(GrpcDecimal left, decimal right) =>
    left is not null && GrpcDecimalExtensions.ToDecimal(left) <= right;

  public static bool operator >=(decimal left, GrpcDecimal right) =>
    right is not null && GrpcDecimalExtensions.ToDecimal(right) > left;

  public static bool operator >=(GrpcDecimal left, decimal right) =>
    left is not null && GrpcDecimalExtensions.ToDecimal(left) > right;

  public static bool operator ==(decimal left, GrpcDecimal right) =>
    right is not null && GrpcDecimalExtensions.ToDecimal(right) == left;

  public static bool operator ==(GrpcDecimal left, decimal right)
    => left is not null && GrpcDecimalExtensions.ToDecimal(left) == right;

  public static bool operator !=(decimal left, GrpcDecimal right) =>
    right is not null && GrpcDecimalExtensions.ToDecimal(right) != left;

  public static bool operator !=(GrpcDecimal left, decimal right) =>
    left is not null && GrpcDecimalExtensions.ToDecimal(left) != right;

  /// <inheritdoc cref="GrpcDecimalExtensions.ToDecimal(GrpcDecimal)" />
  public static implicit operator decimal(GrpcDecimal grpcDecimal) => GrpcDecimalExtensions.ToDecimal(grpcDecimal);

  /// <inheritdoc cref="GrpcDecimalExtensions.ToGrpcDecimal(decimal)" />
  public static implicit operator GrpcDecimal(decimal @decimal) => GrpcDecimalExtensions.ToGrpcDecimal(@decimal);

  /// <inheritdoc cref="GrpcDecimalExtensions.ToString(GrpcDecimal, string?)" />
  /// <remarks>Formats given <paramref name="grpcDecimal" /> as an amount of money.</remarks>
  public static implicit operator string(GrpcDecimal grpcDecimal) => GrpcDecimalExtensions.ToString(grpcDecimal, "C2");
}

/// <summary>Represents extension methods for <see cref="GrpcDecimal" />.</summary>
public static class GrpcDecimalExtensions
{
  private const decimal ScaleFactor = 1_000_000_000;

  /// <summary>Converts given <paramref name="grpcDecimal" /> to <see langword="decimal" />.</summary>
  /// <param name="grpcDecimal">The <see cref="GrpcDecimal" /> to convert.</param>
  /// <returns>The converted <see langword="decimal" />.</returns>
  public static decimal ToDecimal(this GrpcDecimal grpcDecimal) => grpcDecimal.Integer + grpcDecimal.Fractional / ScaleFactor;

  /// <summary>Converts given <paramref name="decimal" /> to <see cref="GrpcDecimal" />.</summary>
  /// <param name="decimal">The <see langword="decimal" /> to convert.</param>
  /// <returns>The converted <see cref="GrpcDecimal" />.</returns>
  public static GrpcDecimal ToGrpcDecimal(this decimal @decimal)
  {
    var integer = decimal.ToInt64(@decimal);

    return new(integer, decimal.ToInt32((@decimal - integer) * ScaleFactor));
  }

  /// <summary>
  /// Converts given <paramref name="grpcDecimal" /> to its <see langword="string" /> representation using given
  /// <paramref name="format" />.
  /// </summary>
  /// <param name="grpcDecimal">The <see cref="GrpcDecimal" /> to convert.</param>
  /// <param name="format">
  /// <inheritdoc cref="decimal.ToString(string?)" />
  /// Defaults to <see langword="null" /> for the default format (<c>G</c>).
  /// </param>
  /// <returns>The formatted <see langword="string" /> representation or <see cref="decimal.Zero" /> upon an error.</returns>
  public static string ToString(this GrpcDecimal grpcDecimal, string? format = null) =>
    ToDecimal(grpcDecimal ?? (GrpcDecimal)decimal.Zero).ToString(format);
}
