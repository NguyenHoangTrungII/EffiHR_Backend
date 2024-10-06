using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace EffiHR.Api.Configuration
{


    public static class SwaggerConfiguration
    {
        public static void AddSwaggerConfiguration(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "EffiHR API", Version = "v1" });

                //// Thêm phần cấu hình TenantId vào header
                //c.AddSecurityDefinition("TenantId", new OpenApiSecurityScheme
                //{
                //    In = ParameterLocation.Header,
                //    Name = "TenantId",
                //    Type = SecuritySchemeType.ApiKey,
                //    Description = "Tenant ID for multi-tenant configuration (default: developer)"
                //    Scheme = "Bearer"

                //});

                //c.SwaggerDoc("v1", new OpenApiInfo { Title = "Swagger API WEB DOTNET Solution", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\n
                      Enter 'Bearer' [space] and then your token in the text input below.
                      \r\n\r\nExample: 'Bearer 12345abcdef'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
       {
                      new OpenApiSecurityScheme
                      {
                        Reference = new OpenApiReference
                          {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                          },
                          Scheme = "oauth2",
                          Name = "Bearer",
                          In = ParameterLocation.Header,
                        },
                        new List<string>()
                      }
                    });

                c.OperationFilter<TenantIdHeaderOperationFilter>();
            });
        }
    }

}
