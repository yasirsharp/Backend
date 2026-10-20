using Autofac.Extensions.DependencyInjection;
using Autofac;
using Core.DependencyResolvers;
using Core.Extensions;
using Core.Utilities.IoC;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Core.Utilities.Security.Encryption;
using Business.DependecyResolvers.Autofac;
using Core.Utilities.Security.JWT;
using DataAccess.Concrete.EntityFramework;
using System.Text;
using System.Threading.RateLimiting;
using API.Middleware;
using API.Startup;
using Microsoft.AspNetCore.StaticFiles;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Yapılandırma doğrulaması: gizli bilgiler appsettings.json'da tutulmaz.
// Geliştirme:  dotnet user-secrets set "TokenOptions:SecurityKey" "<en az 64 karakter rastgele değer>" --project API
// Sunucu:      TokenOptions__SecurityKey ve ConnectionStrings__DefaultConnection ortam değişkenleri
var tokenOptions = builder.Configuration.GetSection("TokenOptions").Get<TokenOptions>()
    ?? throw new InvalidOperationException("TokenOptions yapılandırması bulunamadı.");

if (string.IsNullOrWhiteSpace(tokenOptions.SecurityKey) || Encoding.UTF8.GetByteCount(tokenOptions.SecurityKey) < 32)
    throw new InvalidOperationException(
        "TokenOptions:SecurityKey tanımlı değil veya 32 bayttan kısa. " +
        "User Secrets ya da TokenOptions__SecurityKey ortam değişkeni ile güçlü bir anahtar tanımlayın.");

DuzceUniversiteContext.ConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection tanımlı değil.");

// Loglar: konsol + günlük dönen dosya (varsayılan logs/api-YYYYMMDD.log, 14 gün saklanır).
// IIS'te uygulama havuzu kullanıcısının logs klasörüne yazma izni olmalıdır.
builder.Host.UseSerilog((context, loggerConfiguration) =>
{
    var logPath = context.Configuration["Serilog:FilePath"];
    if (string.IsNullOrWhiteSpace(logPath))
        logPath = Path.Combine(context.HostingEnvironment.ContentRootPath, "logs", "api-.log");

    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File(logPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 14);
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n " +
                      "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
                      "Example: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...\""
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});


builder.Services.AddDependencyResolvers(new ICoreModule[]
{
    new CoreModule()
});

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = tokenOptions.Issuer,
            ValidAudience = tokenOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = SecurityKeyHelper.CreateSecurityKey(tokenOptions.SecurityKey),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

// [Authorize] unutulan bir endpoint varsayılan olarak açık kalmasın: [AllowAnonymous] olmayan
// her endpoint kimliği doğrulanmış kullanıcı ister.
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Login/register uç noktaları için kaba kuvvet koruması (IP başına dakikada 10 istek)
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

builder.Host
    .UseServiceProviderFactory(new AutofacServiceProviderFactory())
    .ConfigureContainer<ContainerBuilder>(builder =>
    {
        builder.RegisterModule(new AutofacBusinessModule());
    });

// CORS Policy: sadece yapılandırmada listelenen origin'ler.
// Frontend API ile aynı adresten (wwwroot) sunuluyorsa CORS gerekmez; liste boş kalabilir.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend",
        policy =>
        {
            policy
                .WithOrigins(allowedOrigins)
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

var app = builder.Build();

// Veritabanı hazırlığı (isteğe bağlı migration + ilk yönetici hesabı)
await DatabaseInitializer.InitializeAsync(app.Configuration, app.Logger);

// Configure the HTTP request pipeline.
// İstek logu en dışta: ExceptionMiddleware'in hatayı 400/500'e çevirdiği son durum kodu loglanır.
app.UseSerilogRequestLogging();
app.ConfigureCustomExceptionMiddleware();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();

// Frontend (React) derlenmiş haliyle wwwroot'tan sunulur; böylece API ve arayüz aynı adresten çalışır.
// index.html önbelleğe alınmaz (yeni sürüm hemen görünsün); dosya adları hash'li olan diğer dosyalar alınabilir.
var spaFileOptions = new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        if (ctx.File.Name == "index.html")
            ctx.Context.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
    }
};
app.UseDefaultFiles();
app.UseStaticFiles(spaFileOptions);

app.UseCors("Frontend");

app.UseRateLimiter();

app.UseAuthentication();

// Geçici parolalı kullanıcı parolasını değiştirene kadar diğer API uçlarını kullanamaz
app.UseMiddleware<PasswordChangeRequiredMiddleware>();

app.UseAuthorization();

app.MapControllers();

// Sağlık kontrolü (izleme ve kurulum sonrası test için): veritabanına bağlanabiliyor mu?
app.MapGet("/health", async () =>
{
    await using var context = new DuzceUniversiteContext();
    var canConnect = await context.Database.CanConnectAsync();
    return canConnect
        ? Results.Ok(new { status = "Healthy" })
        : Results.Json(new { status = "Unhealthy", reason = "Veritabanına bağlanılamıyor" }, statusCode: StatusCodes.Status503ServiceUnavailable);
}).AllowAnonymous();

// Bilinmeyen /api adresleri 404 döner; diğer tüm adresler SPA'nın index.html'ine gider (React Router).
app.MapFallback("/api/{**rest}", context =>
{
    context.Response.StatusCode = StatusCodes.Status404NotFound;
    return Task.CompletedTask;
}).AllowAnonymous();

if (File.Exists(Path.Combine(app.Environment.WebRootPath ?? Path.Combine(app.Environment.ContentRootPath, "wwwroot"), "index.html")))
    app.MapFallbackToFile("index.html", spaFileOptions).AllowAnonymous();

app.Run();
