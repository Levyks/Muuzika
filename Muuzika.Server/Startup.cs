using System.IdentityModel.Tokens.Jwt;
using Muuzika.Server.Providers;
using Muuzika.Server.Providers.Interfaces;
using Muuzika.Server.Repositories;
using Muuzika.Server.Repositories.Interfaces;
using Muuzika.Server.Services;
using Muuzika.Server.Services.Interfaces;

namespace Muuzika.Server;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // Add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddSingleton(() => new Random());
        services.AddSingleton(new JwtSecurityTokenHandler());
        
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        
        services.AddSingleton<IRoomRepository, RoomRepository>();
        
        services.AddSingleton<IRoomService, RoomService>();
        services.AddSingleton<IJwtService, JwtService>();
        // TODO: Replace with real captcha service
        services.AddSingleton<ICaptchaService, NoOpCaptchaService>();
    }

    // Configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}