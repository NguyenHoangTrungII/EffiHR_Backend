using EffiHR.Application.Data;
using EffiHR.Application.Interfaces;
using EffiHR.Application.Services;
using EffiHR.Domain.Interfaces;
using EffiHR.Application.Wrappers;
using EffiHR.Infrastructure.Data;
using EffiHR.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.AspNetCore.Authentication.Google;
using Azure;
using Newtonsoft.Json;

using EffiHR.Infrastructure.Configuration;
using EffiHR.Api.Configuration;
using EffiHR.Core.DTOs.Auth;
using StackExchange.Redis;
using EffiHR.Infrastructure.Messaging;
using Microsoft.Extensions.Configuration;




var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthenticationServices(builder.Configuration);
builder.Services.AddSwaggerConfiguration(); // Thêm cấu hình Swagger


// Đăng ký các dịch vụ
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddSingleton<TenantService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITokenService, TokenService>();

// Đăng ký các dịch vụ
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddScoped<ISessionService, SessionService>();

var redisConnectionString = builder.Configuration.GetConnectionString("RedisConnection");

// Cấu hình Redis connection
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = "RoomApp"; // Tên instance
});

// Thêm RedisCacheService vào DI container
builder.Services.AddScoped<ICacheService, RedisCacheService>();

//Mutil
builder.Services.AddScoped<IMultiProcess, MultiProcess>();

//Mainteance
builder.Services.AddScoped<RabbitMQProducer>();
builder.Services.AddScoped<IMaintenanceQueue, RabbitMQProducer>();
builder.Services.AddScoped<IMaintenanceService, MaintenanceService>();

//GenericRepository
builder.Services.AddScoped(typeof(IGenericRepository), typeof(GenericRepository));
//builder.Services.AddScoped<IGenericRepository, GenericRepository>(); // Đảm bảo đúng định dạng



//Room
builder.Services.AddScoped<IRoomService, RoomService>();


//builder.Services.AddSingleton<IMaintenanceRequestConsumer, MaintenanceRequestConsumer>();
//builder.Services.AddHostedService<RabbitMqConsumerHostedService>();

builder.Services.AddScoped<IMaintenanceRequestConsumer, MaintenanceRequestConsumer>();
builder.Services.AddSingleton<IHostedService, RabbitMqConsumerHostedService>();

builder.Services.AddHostedService<RabbitMQConsumer>();  // Đăng ký RabbitMQ Consumer
builder.Services.AddScoped<MaintenanceService>();       // Đăng ký MaintenanceService


// Đăng ký các service cần thiết cho RabbitMQ
builder.Services.AddScoped<IMaintenanceRequestConsumer, MaintenanceRequestConsumer>();

// Đăng ký RabbitMqConsumerHostedService
builder.Services.AddHostedService<RabbitMqConsumerHostedService>();


builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddMemoryCache();


// Cấu hình Redis
//builder.Services.AddStackExchangeRedisCache(options =>
//{
//    options.Configuration = "localhost:6379";
//    options.InstanceName = "RedisSessionStore";
//});

// Đăng ký IConnectionMultiplexer
//var redisConnectionString = builder.Configuration.GetConnectionString("RedisConnection"); 
//builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));

builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.Configure<MailSetting>(builder.Configuration.GetSection("EmailSettings"));


// Đọc chuỗi kết nối từ appsettings.json
var tenantConnectionStrings = builder.Configuration.GetSection("TenantConnectionStrings").Get<Dictionary<string, string>>();

// Cấu hình DbContext sẽ sử dụng Tenant ConnectionString
builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
{
    var env = serviceProvider.GetRequiredService<IWebHostEnvironment>();

    // Kiểm tra nếu đang chạy trong môi trường phát triển hoặc đang tạo migration
    if (env.IsDevelopment() || Environment.GetEnvironmentVariable("DOTNET_RUNNING_MIGRATION") == "true")
    {
        // Sử dụng connection string mặc định cho việc tạo migration
        options.UseSqlServer("Data Source=NHTRUNG;Initial Catalog=EffiHR_DevelopmentDb;Integrated Security=True;Encrypt=True;TrustServerCertificate=True");
    }
    else
    {
        var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
        var httpContext = httpContextAccessor.HttpContext;

        var connectionString = httpContext?.Items["TenantConnectionString"]?.ToString();
        if (!string.IsNullOrEmpty(connectionString))
        {
            options.UseSqlServer(connectionString);
        }
        else
        {
            throw new Exception("Tenant connection string not found.");
        }
    }
});


//builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
//    .AddEntityFrameworkStores<ApplicationDbContext>()
//    .AddDefaultTokenProviders();

//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//})
//.AddJwtBearer(options =>
//{
//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuer = true,
//        ValidateAudience = true,
//        ValidateLifetime = true,
//        ValidateIssuerSigningKey = true,
//        ValidIssuer = builder.Configuration["Jwt:Issuer"],
//        ValidAudience = builder.Configuration["Jwt:Audience"],
//        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]))
//    };

//    options.Events = new JwtBearerEvents()
//    {
//        OnAuthenticationFailed = c =>
//        {
//            c.NoResult();
//            c.Response.StatusCode = 500;
//            c.Response.ContentType = "text/plain";
//            return c.Response.WriteAsync(c.Exception.ToString());
//        },
//        OnChallenge = context =>
//        {
//            context.HandleResponse();
//            context.Response.StatusCode = 401;
//            context.Response.ContentType = "application/json";
//            var result = JsonConvert.SerializeObject(new ApiResponse<string>("You are not Authorized"));
//            return context.Response.WriteAsync(result);
//        },
//        OnForbidden = context =>
//        {
//            context.Response.StatusCode = 403;
//            context.Response.ContentType = "application/json";
//            var result = JsonConvert.SerializeObject(new ApiResponse<string>("You are not authorized to access this resource"));
//            return context.Response.WriteAsync(result);
//        },
//    };
//}

//);
// Cấu hình Authorization
//builder.Services.AddAuthorization(options =>
//{
//    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
//    options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
//});

// Cấu hình Google Authentication
//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
//})
//.AddCookie()
//.AddGoogle(options =>
//{
//    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
//    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
//});




builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 10;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequiredUniqueChars = 6;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
    options.Lockout.MaxFailedAccessAttempts = 10;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
});




builder.Services.AddAuthorization();

// Thêm dịch vụ CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        policy =>
        {
            policy.WithOrigins("https://localhost:3000") // Thay bằng URL của bạn
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});



// Đăng ký TenantMigrationService với các chuỗi kết nối của tenant
builder.Services.AddSingleton<TenantMigrationService>(sp =>
{
    return new TenantMigrationService(tenantConnectionStrings.Values.ToList());
});



builder.Services.AddControllers();

//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

//    // Thêm phần cấu hình TenantId vào header
//    c.AddSecurityDefinition("TenantId", new OpenApiSecurityScheme
//    {
//        In = ParameterLocation.Header,
//        Name = "TenantId",
//        Type = SecuritySchemeType.ApiKey,
//        Description = "Tenant ID for multi-tenant configuration (default: developer)"
//    });

//    c.OperationFilter<TenantIdHeaderOperationFilter>();
//});



var app = builder.Build();



if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1");
    });
}

app.UseMiddleware<TenantDbContextMiddleware>();



app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
