
using Abstracciones.Interfaces.Reglas;
using Microsoft.AspNetCore.Authentication.Cookies;
using Reglas;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddScoped<IConfiguracion, Configuracion>();

builder.Services.AddHttpClient();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Seguridad/Login";
        options.LogoutPath = "/Seguridad/Logout";
        options.AccessDeniedPath = "/Seguridad/AccesoDenegado";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(120);
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
