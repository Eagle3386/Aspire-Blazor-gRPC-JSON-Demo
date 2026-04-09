using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using MudBlazor.Services;
using ProductListing.Protos;
using ProductListing.Web;

var builder = WebApplication.CreateBuilder(args).AddServiceDefaults();
builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents().Services
                .AddOutputCache()
                .AddHttpClient<BenchmarkClient>(client => client.BaseAddress = new("https://benchmark")).Services
                .AddSingleton(
                  GrpcChannel.ForAddress(
                    builder.Configuration.GetConnectionString(nameof(Products))!, // TODO: Check why "https://products" doesn't work as usual - maybe contact Aspire team, if it’s a bug.
                    new() { HttpHandler = new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler()) }))
                .AddGrpcClient<ProductListing.Protos.Products.ProductsClient>(nameof(ProductsClient), options => options.Address = new("https://products"))
                .ConfigurePrimaryHttpMessageHandler(() => new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler())).Services
                .AddScoped<ProductsClient>(provider => new(provider.GetRequiredService<GrpcChannel>().CreateCallInvoker()))
                .AddScoped<Products.ProductsClient>(provider => provider.GetRequiredService<ProductsClient>())
                .AddSingleton(new BenchmarkHttpHandler(new HttpClientHandler()))
                .AddMudServices();
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
  app.UseDeveloperExceptionPage();
}
else
{
  app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseHttpsRedirection()
   .UseAntiforgery()
   .UseOutputCache();

app.MapStaticAssets();
app.MapRazorComponents<ProductListing.Web.Components.App>()
   .AddInteractiveServerRenderMode();
app.MapDefaultEndpoints();

await app.RunAsync();
