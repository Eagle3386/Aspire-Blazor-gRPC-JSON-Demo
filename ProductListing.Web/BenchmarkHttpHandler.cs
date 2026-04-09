namespace ProductListing.Web;

/// <summary>Represents an <see cref="HttpMessageHandler" /> for benchmarking.</summary>
/// <param name="_inner">The <see cref="HttpMessageHandler" /> to benchmark.</param>
public class BenchmarkHttpHandler(HttpMessageHandler _inner) : DelegatingHandler(_inner)
{
  /// <summary>
  /// Gets the <see langword="int" /> number of bytes read from the <see cref="HttpResponseMessage" />’s
  /// <see cref="HttpResponseMessage.Content" />, if non-<see langword="null" />, otherwise <c>-1</c>.
  /// </summary>
  public int BytesRead => Content?.BytesRead ?? -1;

  /// <summary>
  /// Gets the <see cref="System.Diagnostics.Stopwatch" />’s <see cref="System.Diagnostics.Stopwatch.Elapsed" />
  /// <see cref="TimeSpan" /> for <see cref="HttpResponseMessage" />’s retrieval.
  /// </summary>
  public TimeSpan RetrievalElapsed { get; private set; }

  private static System.Diagnostics.Stopwatch Stopwatch { get; } = new();

  private CaptureResponseLengthContent? Content { get; set; }

  /// <inheritdoc />
  protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
  {
    RetrievalElapsed = TimeSpan.Zero;
    using var response = await base.SendAsync(request, cancellationToken);
    response.Content = Content = new CaptureResponseLengthContent(response.Content);
    RetrievalElapsed = Stopwatch.Elapsed;

    return response;
  }

  private class CaptureResponseLengthContent : HttpContent
  {
    private readonly HttpContent _inner;

    public int? BytesRead => InnerStream?.BytesRead;

    private CaptureResponseLengthStream? InnerStream { get; set; }

    public CaptureResponseLengthContent(HttpContent content)
    {
      _inner = content ?? throw new ArgumentNullException(nameof(content));
      foreach (var header in _inner.Headers)
      {
        _ = Headers.TryAddWithoutValidation(header.Key, header.Value);
      }
    }

    protected override async Task<Stream> CreateContentReadStreamAsync() => InnerStream = new(await _inner.ReadAsStreamAsync());

    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        _inner.Dispose();
        InnerStream?.Dispose();
      }

      base.Dispose(disposing);
    }

#pragma warning disable CS8765 // Unused in demo.
    protected override Task SerializeToStreamAsync(Stream stream, System.Net.TransportContext context) => throw new NotImplementedException();
#pragma warning restore CS8765 // Unused in demo.

    protected override bool TryComputeLength(out long length) => (length = _inner?.Headers.ContentLength ?? 0) > 0;
  }

  private class CaptureResponseLengthStream(Stream _inner) : Stream
  {
    public int BytesRead { get; private set; }

    public override bool CanRead => _inner.CanRead;

    public override bool CanSeek => _inner.CanSeek;

    public override bool CanWrite => _inner.CanWrite;

    public override long Length => _inner.Length;

    public override long Position
    {
      get => _inner.Position;
      set => _inner.Position = value;
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        _inner.Dispose();
      }
    }

    public override void Flush() => _inner.Flush();

    public override int Read(byte[] buffer, int offset, int count)
    {
      var readCount = _inner.Read(buffer, offset, count);
      BytesRead += readCount;

      return readCount;
    }

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
      var readCount = await _inner.ReadAsync(buffer.AsMemory(offset, count), cancellationToken);
      BytesRead += readCount;

      return readCount;
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
      var readCount = await _inner.ReadAsync(buffer, cancellationToken);
      BytesRead += readCount;

      return readCount;
    }

    public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);

    public override void SetLength(long value) => _inner.SetLength(value);

    public override void Write(byte[] buffer, int offset, int count) => _inner.Write(buffer, offset, count);
  }
}
