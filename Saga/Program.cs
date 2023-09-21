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
    cfg.AddSagaStateMachine<SagaStateMachine, SagaStateData>()
        .MongoDbRepository(r =>
        {
            r.Connection = "mongodb://root:admin@localhost:27017";
            r.DatabaseName = "saga";
        });
    
    cfg.UsingRabbitMq((context, configurator) => {
        configurator.Host(new Uri("rabbitmq://localhost:5672"), h =>
        {
            h.Password("guest");
            h.Username("guest");
        });
    
        configurator.ReceiveEndpoint("saga-custom-queue", e =>
        {
            e.ConfigureSaga<SagaStateData>(context);
            e.SetQuorumQueue();
            e.UseInMemoryOutbox();
        });
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