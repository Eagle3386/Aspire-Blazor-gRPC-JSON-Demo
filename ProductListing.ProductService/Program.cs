using ProductListing.ProductService;

var builder = WebApplication.CreateBuilder(args)
                            .AddServiceDefaults(true);
builder.Services.AddProblemDetails()
                .AddOpenApi()
                .AddGrpc(options => options.EnableDetailedErrors = builder.Environment.IsDevelopment()).Services
                .AddHttpClient<ProductService>(client => client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher).Services
                .AddScoped(provider =>
                  new ProductService(
                    provider.GetRequiredService<IHttpClientFactory>(),
                    builder.Configuration.GetConnectionString(nameof(ProductListing))!));
var app = builder.Build();
app.UseGrpcWeb(new() { DefaultEnabled = true });
if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
  app.UseDeveloperExceptionPage();
}

app.MapDefaultEndpoints(true)
   .MapGrpcService<ProductService>();//.WithName("StreamProducts");
await app.RunAsync();
