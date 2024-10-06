using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class TenantIdHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters == null)
            operation.Parameters = new List<OpenApiParameter>();

        // Thêm header TenantId vào tất cả các API call
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "TenantId",
            In = ParameterLocation.Header,
            Required = true, // TenantId là bắt buộc
            Schema = new OpenApiSchema
            {
                Type = "string",
                Default = new Microsoft.OpenApi.Any.OpenApiString("Developer") // Giá trị mặc định là 'developer'
            }
        });
    }
}
