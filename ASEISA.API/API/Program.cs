using Abstracciones.Interfaces.DA;
using Abstracciones.Interfaces.Flujo;
using DA;
using DA.Repositorio;
using Flujo;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Pegue solo el accessToken, sin el prefijo Bearer."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

var issuer = builder.Configuration["Token:Issuer"];
var audience = builder.Configuration["Token:Audience"];
var key = builder.Configuration["Token:key"];
if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience)
    || string.IsNullOrWhiteSpace(key) || Encoding.UTF8.GetByteCount(key) < 32)
    throw new InvalidOperationException("Configure Token:Issuer, Token:Audience y Token:key (minimo 32 bytes), iguales a Seguridad.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            RequireSignedTokens = true,
            RequireExpirationTime = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role,
            ClockSkew = TimeSpan.Zero
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var valor = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(valor, out var idUsuario) || idUsuario <= 0)
                    context.Fail("El token no contiene un IdUsuario valido.");
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddScoped<IRepositorioDapper, RepositorioDapper>();
builder.Services.AddScoped<IUsuarioDA, UsuarioDA>();
builder.Services.AddScoped<IUsuarioFlujo, UsuarioFlujo>();
builder.Services.AddScoped<IBitacoraDA, BitacoraDA>();
builder.Services.AddScoped<IBitacoraFlujo, BitacoraFlujo>();
builder.Services.AddScoped<IBodegaDA, BodegaDA>();
builder.Services.AddScoped<IBodegaFlujo, BodegaFlujo>();
builder.Services.AddScoped<IEmpresaDA, EmpresaDA>();
builder.Services.AddScoped<IEmpresaFlujo, EmpresaFlujo>();
builder.Services.AddScoped<IContactoDA, ContactoDA>();
builder.Services.AddScoped<IContactoFlujo, ContactoFlujo>();
builder.Services.AddScoped<IProductoDA, ProductoDA>();
builder.Services.AddScoped<IProductoFlujo, ProductoFlujo>();
builder.Services.AddScoped<ICompraDA, CompraDA>();
builder.Services.AddScoped<ICompraFlujo, CompraFlujo>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.MapGet("/", () => Results.Redirect("/swagger"));
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
