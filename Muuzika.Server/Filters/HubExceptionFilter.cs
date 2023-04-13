using Microsoft.AspNetCore.SignalR;
using Muuzika.Server.Exceptions;
using Muuzika.Server.Mappers.Interfaces;
using Muuzika.Server.Models;
using ILogger = Serilog.ILogger;

namespace Muuzika.Server.Filters;

public class HubExceptionFilter : IHubFilter
{
    private readonly IExceptionMapper _exceptionMapper;
    
    public HubExceptionFilter(IExceptionMapper exceptionMapper)
    {
        _exceptionMapper = exceptionMapper;
    }
    
    public async ValueTask<object?> InvokeMethodAsync(HubInvocationContext invocationContext, Func<HubInvocationContext, ValueTask<object?>> next)
    {
        try
        {
            return await next(invocationContext);
        } 
        catch (Exception ex)
        {
            var baseException = ex as BaseException ?? new UnknownException();
            
            if (baseException is UnknownException unknownException)
                TryLogException(invocationContext, unknownException);
            
            throw _exceptionMapper.ToHubException(ex);
        }
    }


    private static void TryLogException(HubInvocationContext invocationContext, Exception ex)
    {
        if (!invocationContext.Context.Items.TryGetValue("player", out var playerObj) || playerObj is not Player player)
            return;
        
        var logger = player.Room.ServiceProvider.GetRequiredService<ILogger>();
        logger.Error(ex, "Unknown exception while invoking hub method {MethodName}", invocationContext.HubMethodName);
    }
    
}