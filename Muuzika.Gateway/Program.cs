using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Muuzika.Gateway.Authentication;
using Muuzika.Gateway.Database;
using Muuzika.Gateway.Repositories;
using Muuzika.Gateway.Repositories.Interfaces;
using Muuzika.Gateway.Services;
using Muuzika.Gateway.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<MuuzikaDbContext>(options =>
    options
        .UseNpgsql(builder.Configuration.GetConnectionString("MuuzikaDbContext"))
        .UseSnakeCaseNamingConvention()
);

#region DI
// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IServerRepository, ServerRepository>();

// Services
builder.Services.AddSingleton<IHashService, HashService>();
builder.Services.AddSingleton<IJwtService, JwtService>();
builder.Services.AddScoped<IUserService, UserService>();
#endregion

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = "CombinedAuthentication";
        options.DefaultChallengeScheme = "CombinedAuthentication";
    })
    .AddScheme<AuthenticationSchemeOptions, ServerAuthenticationHandler>("ServerAuthentication", null)
    .AddScheme<AuthenticationSchemeOptions, UserAuthenticationHandler>("UserAuthentication", null)
    .AddPolicyScheme("CombinedAuthentication", "Authorization Server or User", options =>
    {
        options.ForwardDefaultSelector = context =>
        {
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

            return authHeader?.StartsWith("Basic") == true ? "ServerAuthentication" : "UserAuthentication";
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();