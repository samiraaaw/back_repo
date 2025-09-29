using DICREP.EcommerceSubastas.API.Attributes;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class DevelopmentDocumentFilter : IDocumentFilter
{
    private readonly IWebHostEnvironment _env;
    public DevelopmentDocumentFilter(IWebHostEnvironment env) => _env = env;

    public void Apply(OpenApiDocument doc, DocumentFilterContext ctx)
    {
        if (_env.EnvironmentName != "Development")
        {
            var rutasSandbox = ctx.ApiDescriptions
                .Where(ad => ad.ActionDescriptor.EndpointMetadata
                    .OfType<DevelopmentOnlyAttribute>().Any())
                .Select(ad => "/" + ad.RelativePath.TrimEnd('/'))
                .ToList();

            foreach (var ruta in rutasSandbox)
                doc.Paths.Remove(ruta);
        }
    }
}
