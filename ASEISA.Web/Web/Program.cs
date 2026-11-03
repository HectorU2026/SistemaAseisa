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
    options.Conventions.AllowAnonymousToPage("/Seguridad/Registro_aseisa");
    options.Conventions.AllowAnonymousToPage("/Seguridad/Restablecer_Contrasena");
    options.Conventions.AllowAnonymousToPage("/Error");
});

builder.Services.AddScoped<IConfiguracion, Configuracion>();

builder.Services.AddHttpClient();
builder.Services.AddHttpClient<ContactoApiService>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Seguridad/Login";
        options.LogoutPath = "/Seguridad/Logout";
        options.AccessDeniedPath = "/Seguridad/AccesoDenegado";

        options.ExpireTimeSpan = TimeSpan.FromMinutes(120);
        options.SlidingExpiration = false;

        options.Events.OnValidatePrincipal = async context =>
        {
            var token = context.Principal?
                .FindFirst("Token")?.Value;

            if (string.IsNullOrWhiteSpace(token))
            {
                await RechazarSesion(context);
                return;
            }

            var handler = new JwtSecurityTokenHandler();

            if (!handler.CanReadToken(token))
            {
                await RechazarSesion(context);
                return;
            }

            try
            {
                var jwt = handler.ReadJwtToken(token);

                if (jwt.ValidTo <= DateTime.UtcNow)
                {
                    await RechazarSesion(context);
                }
            }
            catch
            {
                await RechazarSesion(context);
            }
        };
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
app.UseAuthorization();

app.MapRazorPages();

app.Run();

static async Task RechazarSesion(
    CookieValidatePrincipalContext context)
{
    context.RejectPrincipal();

    await context.HttpContext.SignOutAsync(
        CookieAuthenticationDefaults.AuthenticationScheme);
}
