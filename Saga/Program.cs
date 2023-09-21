using MassTransit;
using Saga;
using Saga.Configuration;
using Saga.Events;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.ConfigureLogging((hostingContext, logging) =>
{
    logging.AddSerilog(dispose: true);
    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(cfg =>
{
    cfg.AddSagaStateMachine<SagaStateMachine, SagaStateData>().InMemoryRepository();
    
    cfg.UsingInMemory((context, opt) =>
    {
        opt.ConfigureEndpoints(context);
    });
});

builder.Services.AddHostedService<MassTransitHostedService>();

var app = builder.Build();
app.UseSwagger();

app.MapGet("/saga", async (IPublishEndpoint publishEndpoint) =>
{
    await publishEndpoint.Publish(new InitSagaEvent()
    {
        CorrelationId = Guid.NewGuid()
    });
    return Results.Ok();
});

app.UseSwaggerUI();
app.Run();

Log.CloseAndFlush();