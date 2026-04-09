using Grpc.Core;
using ProductListing.Protos;

namespace ProductListing.ProductService;

/// <summary>Represents the product service for data retrieval from a remote API.</summary>
/// <param name="factory">The <see cref="IHttpClientFactory" /> to use.</param>
/// <param name="apiUrl">The <see langword="string" /> URL of the remote API to use.</param>
public class ProductService(IHttpClientFactory factory, string apiUrl) : Products.ProductsBase
{
  /// <inheritdoc />
  public override async Task StreamProducts(ProductsRequest request, IServerStreamWriter<Product> responseStream, ServerCallContext context)
  {
    await foreach (var product in StreamAsync(request.Quantity, context.CancellationToken).ConfigureAwait(false))
    {
      await responseStream.WriteAsync(product).ConfigureAwait(false);
    }
  }

  /// <inheritdoc />
  public override async Task StreamProducts2(ProductsRequest request, IServerStreamWriter<Product> responseStream, ServerCallContext context)
  {
    List<Product> products = [];
    await foreach (var product in factory.CreateClient()
                                         .GetFromJsonAsAsyncEnumerable<ApiProduct>(apiUrl, context.CancellationToken)
                                         .Take(request.Quantity)
                                         .WithCancellation(context.CancellationToken)
                                         .ConfigureAwait(false))
    {
      foreach(var article in product!.Articles)
      {
        products.Add(new Product
        {
          Id = article.Id,
          ImageUrl = article.Image,
          Name = product.Name,
          Price = (decimal)article.Price,
          Unit = article.ShortDescription
        });
      }
    }

    await foreach (var product in products.ToAsyncEnumerable().WithCancellation(context.CancellationToken).ConfigureAwait(false))
    {
      await responseStream.WriteAsync(product, context.CancellationToken).ConfigureAwait(false);
    }
  }

  private async IAsyncEnumerable<Product> StreamAsync(int quantity, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken token)
  {
    var channel = System.Threading.Channels.Channel.CreateUnbounded<Product>();
    using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token);
    var readApiTask = Task.Run(async () =>
    {
      try
      {
        List<Product> products = [];
        await foreach (var product in factory.CreateClient()
                                             .GetFromJsonAsAsyncEnumerable<ApiProduct>(apiUrl, linkedCts.Token)
                                             .Take(quantity)
                                             .WithCancellation(linkedCts.Token)
                                             .ConfigureAwait(false))
        {
          foreach (var article in product!.Articles)
          {
            products.Add(new Product
            {
              Id = article.Id,
              ImageUrl = article.Image,
              Name = product.Name,
              Price = (decimal)article.Price,
              Unit = article.ShortDescription
            });
          }
        }

        await foreach (var product in products.ToAsyncEnumerable().WithCancellation(linkedCts.Token).ConfigureAwait(false))
        {
          await channel.Writer.WriteAsync(product, linkedCts.Token).ConfigureAwait(false);
        }
      }
      finally
      {
        channel.Writer.Complete();
      }
    }, linkedCts.Token);

    try
    {
      await foreach (var product in channel.Reader.ReadAllAsync(linkedCts.Token).ConfigureAwait(false))
      {
        yield return product;
      }

      await readApiTask.ConfigureAwait(false);
    }
    finally
    {
      if (!readApiTask.IsCompleted)
      {
        await Task.WhenAll(linkedCts.CancelAsync(), readApiTask).ConfigureAwait(false);
      }
    }
  }

  private class ApiProduct
  {
    public ICollection<ApiArticle> Articles { get; set; } = [];
    public string BrandName { get; set; } = string.Empty;
    public string DescriptionText { get; set; } = string.Empty;
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
  }

  private class ApiArticle
  {
    public int Id { get; set; }
    public string Image { get; set; } = string.Empty;
    public float Price { get; set; }
    public string PricePerUnitText { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
  }
}
