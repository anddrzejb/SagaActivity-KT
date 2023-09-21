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

var app = builder.Build();
app.UseSwagger();

app.MapGet("/", () => "Hello World!");

app.UseSwaggerUI();
app.Run();

Log.CloseAndFlush();