using MassTransit;
using Microsoft.EntityFrameworkCore;
using ProcessaPedido.Infrastructure.Messaging;
using ProcessaPedido.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System;
using SQLitePCL;
using MassTransit.RabbitMqTransport;
using Microsoft.OpenApi.Models;

SQLitePCL.Batteries_V2.Init();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ProcessaPedido API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Apenas cole o token JWT gerado pelo login (Auth).",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite("Data Source=entregas.db"));

var jwtConfig = builder.Configuration.GetSection("Jwt");
var key = jwtConfig["Key"];
var issuer = jwtConfig["Issuer"];
var audience = jwtConfig["Audience"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
    };
});

// Configuração do MassTransit usando fila em memória (ideal para testes e desenvolvimento local)
//builder.Services.AddMassTransit(x =>
//{
//    x.AddConsumer<EntregaConsumer>();
//    x.UsingInMemory((ctx, cfg) =>
//    {
//        cfg.ConfigureEndpoints(ctx);
//    });
//});

// Configuração do MassTransit usando RabbitMQ real (ideal para produção ou integração real)
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<EntregaConsumer>();
    x.UsingRabbitMq((ctx, cfg) =>
    {
        // Usa a connection string do RabbitMQ de uma variável de ambiente ou padrão localhost (para implementação futura).
        var rabbitMqHost = Environment.GetEnvironmentVariable("RABBITMQ_CONNECTION") ?? "localhost";
        cfg.Host(rabbitMqHost, "/", h => {
            h.Username("guest");
            h.Password("guest");
        });
        cfg.ConfigureEndpoints(ctx);
    });
});

var app = builder.Build();

//Ao iniciar a aplicação (inclusive nos testes), o banco de dados será criado e migrado automaticamente.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }