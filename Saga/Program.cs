using Hangfire;
using MassTransit;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Saga;
using Saga.Configuration;
using Saga.Consumers;
using Saga.Events;
using Serilog;
using Serilog.Events;

SetMongoConventions();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.ConfigureLogging((hostingContext, logging) =>
{
    logging.AddSerilog(dispose: true);
    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHangfire(h =>
{
    h.UseSimpleAssemblyNameTypeSerializer(); //this is probably a default 
    h.UseRecommendedSerializerSettings();
    h.UseSqlServerStorage("Server=.;Database=HangfireApplication;Trusted_Connection=True;");
});

builder.Services.AddMassTransit(cfg =>
{
    cfg.AddConsumer<SideEffectConsumer>();
    
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
        
        configurator.UseHangfireScheduler();

        configurator.ReceiveEndpoint("saga-custom-queue", e =>
        {
            e.ConfigureSaga<SagaStateData>(context);
            e.SetQuorumQueue();
            e.UseInMemoryOutbox();
            
            e.UseConsumeFilter(typeof(RetryLoggerFilter<>), context);
            e.UseMessageRetry(r =>
            {
                r.SetRetryPolicy(x => x.Incremental(3, TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(250)));
            });
        });
        
        configurator.ReceiveEndpoint("saga-side-effect", e =>
        {
            e.Consumer<SideEffectConsumer>(context);
            e.SetQuorumQueue();
        });
    });
});

builder.Services.AddHostedService<MassTransitHostedService>();

var app = builder.Build();
app.UseSwagger();

app.MapGet("/saga/{initializeWithAlternativeStep:bool}/{forceSchedule:bool}/{retryCount:int}",
    async (IPublishEndpoint publishEndpoint, bool initializeWithAlternativeStep = false, bool forceSchedule = false, int retryCount = 0) =>
    {

    SagaStateMachine.RetryCount = retryCount;
    await publishEndpoint.Publish(new InitSagaEvent()
    {
        CorrelationId = Guid.NewGuid(),
        InitializeWithAlternativeStep = initializeWithAlternativeStep,
        ForceSchedule = forceSchedule
    });
    return Results.Ok();
});

app.UseSwaggerUI();
app.UseHangfireDashboard();

app.Run();

Log.CloseAndFlush();

//Setting these mongo conventions will help identifying hangfire schedules in db 
void SetMongoConventions()
{
    ConventionRegistry.Register("camelCaseConvention",
        new ConventionPack
        {
            new CamelCaseElementNameConvention(),
        },
        t => true);
    ConventionRegistry.Register("ignoreExtraElementsConvention",
        new ConventionPack
        {
            new IgnoreExtraElementsConvention(true),
        },
        t => true);
#pragma warning disable CS0618 // It might be obsolete but without it driver doesnt work well while querying. The solution from documentation doesnt work properly
    MongoDefaults.GuidRepresentation = GuidRepresentation.Standard;
#pragma warning restore CS0618 // It might be obsolete but without it driver doesnt work well while querying. The solution from documentation doesnt work properly
}