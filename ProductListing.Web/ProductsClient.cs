using Grpc.Core;
using ProductListing.Protos;

namespace ProductListing.Web;

/// <summary>Represents the <see cref="Products.ProductsClient" />.</summary>
/// <param name="invoker">The <see cref="CallInvoker" /> to use.</param>
public class ProductsClient(CallInvoker invoker) : Products.ProductsClient(invoker)
{
  /// <inheritdoc />
  public override AsyncServerStreamingCall<Product> StreamProducts(
    ProductsRequest request,
    Metadata headers = null!,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default) =>
      base.StreamProducts(request, headers, deadline, cancellationToken);

  /// <inheritdoc />
  public override AsyncServerStreamingCall<Product> StreamProducts2(
    ProductsRequest request,
    Metadata headers = null!,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default) =>
      base.StreamProducts2(request, headers, deadline, cancellationToken);

  /// <summary>Streams products, up to given <paramref name="quantity" /> inclusively.</summary>
  /// <param name="quantity">The <see langword="int" /> quantity to limit to.</param>
  /// <param name="token">The <see cref="CancellationToken" /> to use.</param>
  /// <returns>An <see langword="await" /> <see cref="IAsyncEnumerable{T}" /> for streaming products.</returns>
  public IAsyncEnumerable<Product> StreamProductsAsync(int quantity, CancellationToken token) =>
    StreamProducts(new ProductsRequest { Quantity = quantity }, cancellationToken: token).ResponseStream.ReadAllAsync(token);
}
