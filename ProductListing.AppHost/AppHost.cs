var builder = DistributedApplication.CreateBuilder(args);
#pragma warning disable ASPIRECERTIFICATES001
var products = builder.AddProject<Projects.ProductListing_ProductService>("products")
                      .AsHttp2Service()
                      .WithDeveloperCertificateTrust(true)
                      .WithHttpsDeveloperCertificate()
                      .WithHttpHealthCheck("/health");
var benchmark = builder.AddProject<Projects.ProductListing_BenchmarkService>("benchmark")
                       .AsHttp2Service()
                       .WithDeveloperCertificateTrust(true)
                       .WithHttpsDeveloperCertificate()
                       .WithHttpHealthCheck("/health");
builder.AddProject<Projects.ProductListing_Web>("web")
       .AsHttp2Service()
       .WithDeveloperCertificateTrust(true)
       .WithHttpsDeveloperCertificate()
       .WithExternalHttpEndpoints()
       .WithHttpHealthCheck("/health")
       .WithReference(products)
       .WithReference(benchmark)
       .WaitFor(products);
#pragma warning restore ASPIRECERTIFICATES001
builder.Build().Run();
