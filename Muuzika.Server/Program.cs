using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.SignalR;
using Muuzika.Server.Auth;
using Muuzika.Server.Filters;
using Muuzika.Server.Hubs;
using Muuzika.Server.Mappers;
using Muuzika.Server.Mappers.Interfaces;
using Muuzika.Server.Providers;
using Muuzika.Server.Providers.Interfaces;
using Muuzika.Server.Repositories;
using Muuzika.Server.Repositories.Interfaces;
using Muuzika.Server.Services;
using Muuzika.Server.Services.Interfaces;
using Muuzika.Server.Services.Playlist;
using Muuzika.Server.Services.Playlist.Interfaces;
using Muuzika.Server.Services.Room;
using Muuzika.Server.Services.Room.Interfaces;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Templates;
using ILogger = Serilog.ILogger;

namespace Muuzika.Server;

internal class Program
{
    private static readonly Logger Logger = new LoggerConfiguration()
        .WriteTo.Console(new ExpressionTemplate(
            "[{@t:HH:mm:ss} {@l:u3}" +
            "{#if SourceContext is not null} {Substring(SourceContext, LastIndexOf(SourceContext, '.') + 1)}{#end}"+ 
            "{#if AdditionalSourceIdentifier is not null}:{AdditionalSourceIdentifier}{#end}" +
            "] {@m}\n{@x}"))
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .CreateLogger();
    
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };
    
    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers(options =>
                {
                    options.Filters.Add(new BaseExceptionFilter());
                })
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonSerializerOptions.PropertyNamingPolicy;
                    JsonSerializerOptions.Converters.ToList().ForEach(options.JsonSerializerOptions.Converters.Add);
                });
            
        services.AddSignalR(options =>
                {
                    options.AddFilter<HubExceptionFilter>();    
                })
                .AddJsonProtocol(options =>
                {
                    options.PayloadSerializerOptions = JsonSerializerOptions;
                });

        services.AddCors(options =>
            {
                options.AddDefaultPolicy(corsPolicyBuilder =>
                {
                    corsPolicyBuilder.AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        //.AllowCredentials()
                        .AllowAnyOrigin(); //.WithOrigins("http://127.0.0.1:5173");
                });
            });
    
        services.AddAuthentication(options =>
                {
                    options.DefaultScheme = "MuuzikaAuthentication";
                })
                .AddScheme<AuthenticationSchemeOptions, MuuzikaAuthenticationHandler>("MuuzikaAuthentication", null);

        services.AddAuthorization(LeaderOnlyPolicy.Register);
        
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        
        RegisterDependencies(services);
    }
    
    private static void RegisterDependencies(IServiceCollection services)
    {
        services.AddTransient<IHttpService, HttpService>();
        
        services.AddSingleton(new JwtSecurityTokenHandler());
        services.AddSingleton(JsonSerializerOptions);
            
        services.AddSingleton<ILogger>(Logger);

        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<IConfigProvider, ConfigProvider>();
        services.AddSingleton<IRandomProvider, RandomProvider>();

        services.AddSingleton<IPlayerMapper, PlayerMapper>();
        services.AddSingleton<IRoomMapper, RoomMapper>();
        services.AddSingleton<IExceptionMapper, ExceptionMapper>();
        services.AddSingleton<ISpotifyMapper, SpotifyMapper>();
        services.AddSingleton<IPlaylistMapper, PlaylistMapper>();

        services.AddSingleton<IRoomRepository, RoomRepository>();
    
        services.AddSingleton<IRoomJoinerService, RoomJoinerService>();
        services.AddSingleton<IJwtService, JwtService>();
        services.AddSingleton<IPlaylistFetcherService, SpotifyPlaylistFetcherService>();
        // TODO: Replace with real captcha service
        services.AddSingleton<ICaptchaService, NoOpCaptchaService>();

        services.AddSingleton(services);
    }

    private static void ConfigureApp(WebApplication app)
    {
        app.UseSerilogRequestLogging();
        
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthorization();
        app.MapControllers();
        app.MapHub<RoomHub>("/hub");
        app.UseCors();
    }
    
    public static void Main(string[] args)
    {
        Log.Logger = Logger;

        try
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.UseSerilog();
            ConfigureServices(builder.Services);
            var app = builder.Build();
            ConfigureApp(app);
            app.Run();
        }
        catch (Exception e)
        {
            Log.Fatal(e, "Host terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
