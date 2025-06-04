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

SQLitePCL.Batteries_V2.Init();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
// Add the missing using directive for WebApplication  

builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite("Data Source=entregas.db"));

// Gera o token JWT fixo a partir do payload fornecido
var fixedJwtToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhZG1pbiIsInJvbGUiOiJBZG1pbiIsImV4cCI6MTcxNzQzMTYwMH0.2Qn6QwQw6QwQwQwQwQwQwQwQwQwQwQwQwQwQwQwQwQw"; // Substitua por seu token real

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            Console.WriteLine($"Token recebido: {token}");
            context.Token = token; // Aceita qualquer token para teste
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine("Token validado com sucesso!");
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"Falha na autenticação: {context.Exception.Message}");
            return Task.CompletedTask;
        }
    };
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateIssuerSigningKey = false,
        ValidateLifetime = false,
        //SignatureValidator = (token, parameters) => new JwtSecurityToken(token) O .NET 8 espera que o SignatureValidator retorne um JsonWebToken, não um JwtSecurityToken.
        SignatureValidator = (token, parameters) => new Microsoft.IdentityModel.JsonWebTokens.JsonWebToken(token)
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
        // Usa a connection string do RabbitMQ de uma variável de ambiente ou padrão localhost
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