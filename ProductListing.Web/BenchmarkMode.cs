namespace ProductListing.Web;

/// <summary>Represents the benchmark’s serialization mode.</summary>
public enum BenchmarkMode
{
  /// <summary>Represents the JSON mode.</summary>
  Json,

  /// <summary>Represents the gRPC-Web mode.</summary>
  GrpcWeb,

  /// <summary>Represents the gRPC-Web-Text mode.</summary>
  GrpcWebText
}
