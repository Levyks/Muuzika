using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Serialization;
using Muuzika.Server.Hubs;
using Muuzika.Server.Mappers;
using Muuzika.Server.Mappers.Interfaces;
using Muuzika.Server.Providers;
using Muuzika.Server.Providers.Interfaces;
using Muuzika.Server.Repositories;
using Muuzika.Server.Repositories.Interfaces;
using Muuzika.Server.Services;
using Muuzika.Server.Services.Interfaces;
using Serilog;
using Serilog.Events;
using Serilog.Templates;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new ExpressionTemplate(
        "[{@t:HH:mm:ss} {@l:u3}" +
        "{#if SourceContext is not null} {Substring(SourceContext, LastIndexOf(SourceContext, '.') + 1)}{#end}"+ 
        "{#if AdditionalSourceIdentifier is not null}:{AdditionalSourceIdentifier}{#end}" +
        "] {@m}\n{@x}"))
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();
    
    var jsonStringEnumConverter = new JsonStringEnumConverter();
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(jsonStringEnumConverter);
        });
    builder.Services.AddSignalR()
        .AddJsonProtocol(options =>
        {
            options.PayloadSerializerOptions.Converters.Add(jsonStringEnumConverter);
        });
    
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(corsPolicyBuilder =>
        {
            corsPolicyBuilder.AllowAnyMethod()
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .WithOrigins("http://127.0.0.1:5173");
        });
    });
        
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    
    builder.Services.AddSingleton(() => new Random());
    builder.Services.AddSingleton(new JwtSecurityTokenHandler());

    builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
    builder.Services.AddSingleton<IConfigProvider, ConfigProvider>();
    
    builder.Services.AddSingleton<IPlayerMapper, PlayerMapper>();
    builder.Services.AddSingleton<IRoomMapper, RoomMapper>();
    builder.Services.AddSingleton<IExceptionMapper, ExceptionMapper>();
    
    builder.Services.AddSingleton<IRoomRepository, RoomRepository>();
        
    builder.Services.AddSingleton<IRoomService, RoomService>();
    builder.Services.AddSingleton<IJwtService, JwtService>();
    // TODO: Replace with real captcha service
    builder.Services.AddSingleton<ICaptchaService, NoOpCaptchaService>();
    
    var app = builder.Build();     
    
    app.UseSerilogRequestLogging();
        
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseRouting();
    app.MapControllers();
    app.MapHub<RoomHub>("/hub");
    app.UseCors();

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
