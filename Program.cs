using LogBlazorWebApp.Components;
using LogBlazorWebApp.Configurations;
using LogBlazorWebApp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.IdentityModel.Tokens;
using System.Text;

// Creaci�n de la aplicaci�n
var builder = WebApplication.CreateBuilder(args);

// Configuraci�n de servicios
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Configuraci�n de la base de datos SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null)));

// Configuraci�n de Identity con ApplicationUser y pol�ticas de contrase�a
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true; // Requiere al menos un d�gito
    options.Password.RequiredLength = 6; // Longitud m�nima de 6 caracteres
    options.Password.RequireNonAlphanumeric = false; // No requiere caracteres no alfanum�ricos
    options.Password.RequireUppercase = true; // Requiere al menos una letra may�scula
    options.Password.RequireLowercase = true; // Requiere al menos una letra min�scula
    options.Password.RequiredUniqueChars = 1; // Requiere al menos un car�cter �nico
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Registro de servicios personalizados
builder.Services.AddScoped<TokenService>();

// Configuración de JWTSettings
var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services.Configure<JwtSettings>(jwtSection);

// Configuracion de autenticacion con JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        var jwtSettings = jwtSection.Get<JwtSettings>();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Secret"]))
        };
    });

// Add services to the container
builder.Services.AddAuthorization();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// Agregar soporte para controladores
builder.Services.AddControllers();

// Configuraci�n de FluentUI
builder.Services.AddFluentUIComponents(options =>
{
    options.ValidateClassNames = false;
});

// Registro de HttpClient
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7211") });

// Configuracion de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Construccion de la aplicacion
var app = builder.Build();

// Configuraci�n del Middleware
app.UseCors("AllowAll");
app.UseAuthentication(); // Habilitar autenticaci�n
app.UseAuthorization();  // Habilitar autorizaci�n
app.MapControllers(); // Configurar endpoints de controladores

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Configuraci�n de la aplicaci�n
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// Mapeo de componentes Razor
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(LogBlazorWebApp.Client._Imports).Assembly);

// Ejecucion de la aplicacion
app.Run();
