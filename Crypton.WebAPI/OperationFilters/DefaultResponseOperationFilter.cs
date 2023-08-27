using Crypton.Infrastructure.Diamond;
using Crypton.Infrastructure.Idempotency;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Crypton.WebAPI.OperationFilters;

public sealed class DefaultResponseOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Responses.Remove("200");

        if (HasAttribute<RequireIdempotencyAttribute>(context))
            operation.Responses.TryAdd("409", new OpenApiResponse { Description = "Conflicting Idempotency Key" });

        if (!HasAttribute<AllowAnonymousAttribute>(context))
        {
            operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });
            operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Forbidden" });
        }

        if (!HasAttribute<IgnoreDigitalSignatureAttribute>(context))
            operation.Responses.TryAdd("400", new OpenApiResponse { Description = "Digital Signature rules issue" });

        if (HasAttribute<EnableRateLimitingAttribute>(context) && !HasAttribute<DisableRateLimitingAttribute>(context))
            operation.Responses.TryAdd("429", new OpenApiResponse { Description = "Rate limit" });

        operation.Responses.TryAdd("500", new OpenApiResponse { Description = "Internal Server Error" });
    }

    private static bool HasAttribute<TAttribute>(OperationFilterContext context)
        where TAttribute : Attribute
    {
        return context
            .ApiDescription
            .ActionDescriptor
            .EndpointMetadata
            .OfType<TAttribute>()
            .Any();
    }
}