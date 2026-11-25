using Abstracciones.Interfaces.Reglas;
using Microsoft.AspNetCore.Authentication.Cookies;
using Web.Servicios;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");

    options.Conventions.AllowAnonymousToPage("/Seguridad/Login");
    options.Conventions.AllowAnonymousToPage("/Seguridad/Logout");
    options.Conventions.AllowAnonymousToPage("/Seguridad/Registro_aseisa");
    options.Conventions.AllowAnonymousToPage("/Seguridad/Cambiar_Contrasena");
    options.Conventions.AllowAnonymousToPage("/Seguridad/Restablecer_Contrasena");
    options.Conventions.AllowAnonymousToPage("/Error");
});

builder.Services.AddScoped<IConfiguracion, Configuracion>();

builder.Services.AddHttpClient();
builder.Services.AddScoped<SesionCookieEvents>();
builder.Services.AddHttpClient("SesionSeguridad",c=>c.Timeout=TimeSpan.FromSeconds(10))
    .ConfigurePrimaryHttpMessageHandler(()=>new HttpClientHandler {AllowAutoRedirect=false});
builder.Services.AddHttpClient<UsuariosApiService>()
    .ConfigurePrimaryHttpMessageHandler(()=>new HttpClientHandler {AllowAutoRedirect=false});
builder.Services.AddHttpClient<ContactoApiService>();
builder.Services.AddHttpClient<TerminoPagoApiService>();
builder.Services.AddHttpClient<MetodoPagoApiService>();
builder.Services.AddHttpClient<DistribucionAnaliticaApiService>();
builder.Services.AddHttpClient<CuentaContableApiService>();
builder.Services.AddHttpClient<ProductoApiService>();
builder.Services.AddHttpClient<FacturaApiService>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Seguridad/Login";
        options.LogoutPath = "/Seguridad/Logout";
        options.AccessDeniedPath = "/Seguridad/AccesoDenegado";

        options.ExpireTimeSpan = TimeSpan.FromMinutes(120);
        options.SlidingExpiration = false;

        options.EventsType = typeof(SesionCookieEvents);
    });

var app = builder.Build();

// Configuración del entorno
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.Use(async(context,next)=>
{
    if(context.Items.ContainsKey(SesionCookieEvents.NoDisponible))
    {
        context.Response.StatusCode=503;context.Response.Headers.CacheControl="no-store";
        await context.Response.WriteAsync("No fue posible verificar la sesion. Intente nuevamente mas tarde.");return;
    }
    await next(context);
});
app.UseAuthorization();

app.MapRazorPages();

app.Run();
