using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using API.Seguridad;
using System.Security.Claims;
using Abstracciones.DA;
using Abstracciones.Flujo;
using Abstracciones.Modelos;
using Abstracciones.Reglas;
using BC;
using DA;
using DA.Repositorios;
using Flujo;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Reglas;
using Servicios;
using Servicios.Correo;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var tokenConfiguration = builder.Configuration.GetSection("Token").Get<TokenConfiguracion>();
var jwtIssuer = tokenConfiguration.Issuer;
var jwtAudience = tokenConfiguration.Audience;
var jwtKey = tokenConfiguration.key;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(
    options => {
        options.MapInboundClaims = true;
        options.IncludeErrorDetails = false;
        options.EventsType = typeof(RevalidacionJwtEvents);
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            RequireSignedTokens = true,
            RequireExpirationTime = true,
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role,
            ClockSkew = TimeSpan.Zero,
            ValidateIssuer=true,
            ValidateAudience=true,
            ValidateLifetime=true,
            ValidateIssuerSigningKey=true,
            ValidIssuer=jwtIssuer,
            ValidAudience= jwtAudience,
            IssuerSigningKey= new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    }
    );


builder.Services.AddScoped<IContextoUsuarioDA, ContextoUsuarioDA>();
builder.Services.AddScoped<IContextoUsuarioFlujo, ContextoUsuarioFlujo>();
builder.Services.AddScoped<RevalidacionJwtEvents>();
builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(options=>
{
    options.RejectionStatusCode=429;
    options.AddPolicy("recuperacion",context=>RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "desconocido",
        _=>new FixedWindowRateLimiterOptions {PermitLimit=5,Window=TimeSpan.FromMinutes(1),QueueLimit=0}));
});
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IRepositorioDapper, RepositorioDapper>();
builder.Services.AddScoped<IUsuarioDA, UsuarioDA>();
builder.Services.AddScoped<IAuditoriaSeguridadDA, AuditoriaSeguridadDA>();
builder.Services.AddScoped<IAuditoriaSeguridad, AuditoriaSeguridad>();
builder.Services.AddScoped<ICredencialesDA, CredencialesDA>();
builder.Services.AddScoped<ICredencialesFlujo, CredencialesFlujo>();
builder.Services.AddScoped<IEntregaEnlace, EntregaEnlace>();
builder.Services.AddScoped<IProtectorEnlace, ProtectorEnlaces>();
builder.Services.AddScoped<ICorreoEnlaceDA, CorreoEnlaceDA>();

builder.Services.AddHostedService<DespachadorEnlaces>();
builder.Services.AddScoped<IUsuarioFlujo, UsuarioFlujo>();
builder.Services.AddScoped<IAutenticacionFlujo, AutenticacionFlujo>();
builder.Services.AddScoped<IAutenticacionBC, AutenticacionReglas>();
builder.Services.AddScoped<IRecuperarContrasena, RecuperarContrasena>();
builder.Services.AddScoped<IEnviarCorreo, EnviarCorreo>();
builder.Services.AddScoped<IConstructorCorreo, ConstructorCorreo>();

var politicaAcceso = "Politica de acceso";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: politicaAcceso,
                      policy =>
                      {
                          policy.WithOrigins("https://localhost:7253", "https://localhost:7280", "https://localhost:5032")
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

var app = builder.Build();
app.UseMiddleware<API.Seguridad.ErroresUsuariosMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
