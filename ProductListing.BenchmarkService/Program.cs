using ProductListing.BenchmarkService;

var builder = WebApplication.CreateBuilder(args).AddServiceDefaults(true);
builder.Services.AddOpenApi();
var app = builder.Build();
app.UseGrpcWeb();
if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
  app.UseDeveloperExceptionPage().UseWebAssemblyDebugging();
}

app.MapGet("/forecast/{count}", (int count) => WeatherFactory.Create<WeatherForecast>(count)).WithName("GetForecast");
app.MapDefaultEndpoints(true)
   .MapGrpcService<WeatherService>().EnableGrpcWeb();
app.Run();
