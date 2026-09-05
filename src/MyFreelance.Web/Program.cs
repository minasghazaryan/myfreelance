using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using MyFreelance.Domain.Constants;
using MyFreelance.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using MyFreelance.Infrastructure;
using MyFreelance.Infrastructure.Logging;
using MyFreelance.Infrastructure.Persistence;
using MyFreelance.Web.Filters;
using MyFreelance.Web.Hubs;
using MyFreelance.Web.Services;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var databaseLogging = builder.Configuration.GetSection("DatabaseLogging");
var databaseLoggingEnabled = databaseLogging.GetValue("Enabled", true);
var databaseMinimumLevel = Enum.TryParse<LogEventLevel>(
    databaseLogging["MinimumLevel"],
    ignoreCase: true,
    out var parsedLevel)
    ? parsedLevel
    : LogEventLevel.Warning;

var loggerConfiguration = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console();

if (databaseLoggingEnabled)
{
    loggerConfiguration = loggerConfiguration.WriteTo.Sink(
        new DatabaseLogSink(connectionString ?? string.Empty),
        restrictedToMinimumLevel: databaseMinimumLevel);
}

Log.Logger = loggerConfiguration.CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

var cookieSecurePolicy = ResolveCookieSecurePolicy(builder.Configuration, builder.Environment);

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = cookieSecurePolicy;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
});

var jwtKey = builder.Configuration["Jwt:Key"] ?? "AurumWealth-Super-Secret-Key-Min-32-Chars!!";
builder.Services.AddAuthentication()
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "AurumWealth",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "AurumWealth.Api",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminArea", policy => policy.RequireRole(AppRoles.Admin, AppRoles.AdminReadOnly));
    options.AddPolicy("AdminWrite", policy => policy.RequireRole(AppRoles.Admin));
    options.AddPolicy("AdminOnly", policy => policy.RequireRole(AppRoles.Admin));
    options.AddPolicy("InvestorOnly", policy => policy.RequireRole(AppRoles.Investor));
    options.AddPolicy("KycApproved", policy => policy.RequireAssertion(ctx =>
        ctx.User.HasClaim("KycApproved", "true")
        || ctx.User.IsInRole(AppRoles.Admin)
        || ctx.User.IsInRole(AppRoles.AdminReadOnly)));
});

builder.Services.AddScoped<AdminWriteAuthorizationFilter>();
builder.Services.AddScoped<SuspendedAccountFilter>();
builder.Services.AddScoped<IClientLocationService, ClientLocationService>();
builder.Services.AddHttpClient("IpLookup", client =>
{
    client.Timeout = TimeSpan.FromSeconds(2);
});
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeAreaFolder("Admin", "/", "AdminArea");
    options.Conventions.AddAreaFolderApplicationModelConvention("Admin", "/", model =>
    {
        model.Filters.Add(new ServiceFilterAttribute(typeof(AdminWriteAuthorizationFilter)));
    });
    options.Conventions.AuthorizeFolder("/Dashboard", "InvestorOnly");
    options.Conventions.AddFolderApplicationModelConvention("/Dashboard", model =>
    {
        model.Filters.Add(new ServiceFilterAttribute(typeof(SuspendedAccountFilter)));
    });
});

builder.Services.AddSignalR();
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.SecurePolicy = cookieSecurePolicy;
});

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions { PermitLimit = 100, Window = TimeSpan.FromMinutes(1) }));
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

builder.Services.AddHttpContextAccessor();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor
        | ForwardedHeaders.XForwardedProto
        | ForwardedHeaders.XForwardedHost;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();
var useHttpsRedirection = builder.Configuration.GetValue("App:UseHttpsRedirection", !app.Environment.IsDevelopment());

using (var scope = app.Services.CreateScope())
{
    await DatabaseSeeder.SeedAsync(scope.ServiceProvider);
}

if (!app.Environment.IsDevelopment())
{
    app.UseForwardedHeaders();
    app.UseExceptionHandler("/Error");
    if (useHttpsRedirection)
        app.UseHsts();
}

if (useHttpsRedirection)
    app.UseHttpsRedirection();

var uploadsPath = Path.GetFullPath(
    Path.Combine(app.Environment.ContentRootPath,
        builder.Configuration["FileStorage:Path"] ?? "uploads"));

if (Directory.Exists(uploadsPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
        RequestPath = "/uploads"
    });
}

app.UseStaticFiles();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapRazorPages();
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();

static CookieSecurePolicy ResolveCookieSecurePolicy(IConfiguration configuration, IWebHostEnvironment environment)
{
    var configured = configuration["Auth:CookieSecurePolicy"];
    if (!string.IsNullOrWhiteSpace(configured)
        && Enum.TryParse<CookieSecurePolicy>(configured, ignoreCase: true, out var policy))
    {
        return policy;
    }

    return environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
}
