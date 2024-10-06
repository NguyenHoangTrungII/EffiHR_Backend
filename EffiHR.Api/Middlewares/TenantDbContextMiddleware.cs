using EffiHR.Infrastructure.Services;

public class TenantDbContextMiddleware
{
    private readonly RequestDelegate _next;
    private readonly TenantService _tenantService;

    public TenantDbContextMiddleware(RequestDelegate next, TenantService tenantService)
    {
        _next = next;
        _tenantService = tenantService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Lấy TenantId từ HTTP header
        var tenantId = context.Request.Headers["TenantId"].ToString();

        if (!string.IsNullOrEmpty(tenantId))
        {
            // Lấy ConnectionString từ TenantService dựa trên TenantId
            var connectionString = _tenantService.GetConnectionString(tenantId);

            // Thiết lập ConnectionString trong HttpContext để các thành phần khác sử dụng
            if (!string.IsNullOrEmpty(connectionString))
            {
                context.Items["TenantConnectionString"] = connectionString;
            }
            else
            {
                throw new Exception($"Connection string for tenant '{tenantId}' not found.");
            }
        }
        else
        {
            throw new Exception("TenantId is missing from the request headers.");
        }

        // Tiếp tục chuyển yêu cầu tới các middleware tiếp theo
        await _next(context);
    }
}
