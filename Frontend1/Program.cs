using Autofac;
using Autofac.Extensions.DependencyInjection;
using Business.Abstract;
using Business.Concrete;
using Business.DependecyResolvers.Autofac;
using Core.DependencyResolvers;
using Core.Extensions;
using Core.Utilities.IoC;
using Core.Utilities.Security.Encryption;
using Core.Utilities.Security.JWT;
using DataAccess.Abstract;
using DataAccess.Concrete.EntityFramework;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

builder.Services.AddDependencyResolvers(new ICoreModule[]
{
    new CoreModule()
});

var tokenOptions = builder.Configuration.GetSection("TokenOptions").Get<TokenOptions>();

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
        };
    });

// CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

// JWT Configuration
builder.Services.Configure<TokenOptions>(builder.Configuration.GetSection("TokenOptions"));

builder.Host
    .UseServiceProviderFactory(new AutofacServiceProviderFactory())
    .ConfigureContainer<ContainerBuilder>(builder =>
    {
        builder.RegisterModule(new AutofacBusinessModule());
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCors("AllowAll");

app.UseRouting();

// Kimlik doğrulama ve yetkilendirme middleware'lerinin sırası önemli
app.UseAuthentication();
app.UseAuthorization();

// Özel middleware - Token kontrolü ve yönlendirme
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value?.ToLower();

    // Auth ile ilgili sayfalara ve statik dosyalara doğrudan erişime izin ver
    if (path != null && (
        path.StartsWith("/auth/") ||
        path.StartsWith("/lib/") ||
        path.StartsWith("/css/") ||
        path.StartsWith("/js/") ||
        path.StartsWith("/images/")))
    {
        await next();
        return;
    }

    // AuthToken cookie'sini kontrol et
    var authToken = context.Request.Cookies["AuthToken"];
    if (!string.IsNullOrEmpty(authToken))
    {
        await next();
        return;
    }

    // AJAX isteklerini kontrol et
    if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsJsonAsync(new { message = "Oturum süresi doldu. Lütfen tekrar giriş yapın." });
        return;
    }

    // Kimliği doğrulanmamış kullanıcıları login sayfasına yönlendir
    if (!path.Equals("/auth/login"))
    {
        context.Response.Redirect("/Auth/Login");
        return;
    }

    await next();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();
