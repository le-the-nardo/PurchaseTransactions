using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PurchaseTransactions.Api.Swagger;

public class CurrencyExamplesOperationFilter : IOperationFilter
{
    private static readonly string[] Currencies =
    [
        "Canada-Dollar",
        "Euro Zone-Euro",
        "Japan-Yen",
        "United Kingdom-Pound",
        "Australia-Dollar",
        "Brazil-Real",
        "China-Renminbi",
        "Mexico-Peso",
        "Switzerland-Franc",
        "India-Rupee",
    ];

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.ApiDescription.RelativePath?.EndsWith("/converted", StringComparison.OrdinalIgnoreCase) != true)
            return;

        var param = operation.Parameters?
            .OfType<OpenApiParameter>()
            .FirstOrDefault(p => p.Name == "currency");

        if (param is null)
            return;

        param.Description = "Currency name as listed in the Treasury Reporting Rates of Exchange.";
        param.Examples = Currencies.ToDictionary(
            c => c,
            c => (IOpenApiExample)new OpenApiExample { Value = JsonValue.Create(c) }
        );
    }
}
