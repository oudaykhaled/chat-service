using ChatService.API;
using ChatService.API.Extensions;
using ChatService.API.Jobs;
using ChatService.Application;
using ChatService.Domain;
using ChatService.Infrastructure;
using ChatService.Infrastructure.Broker;
using ChatService.Infrastructure.Models;
using ChatService.Persistence;
using Microsoft.Extensions.Http.Resilience;
using Polly;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<FirestoreOptions>(builder.Configuration.GetSection(FirestoreOptions.Firestore));
builder.Services.Configure<NatsConfiguration>(builder.Configuration.GetSection(NatsConfiguration.Nats));
builder.Services.Configure<ModerationOptions>(builder.Configuration.GetSection(ModerationOptions.Moderation));

builder.Services.ConfigureInfrastructure();
builder.Services.ConfigurePersistence();
builder.Services.ConfigureApplication();
builder.Services.ConfigureCorsPolicy();
builder.Services.ConfigureApiBehavior();
builder.Services.AddScoped<ProtobufData<MessageInfo>>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient(ClientIntegration.Integration);
    /*.RemoveAllResilienceHandlers()
    .AddResilienceHandler(ClientIntegration.IntegrationResilience, pipeline =>
    {
        pipeline.AddTimeout(TimeSpan.FromSeconds(10));

        pipeline.AddRetry(new HttpRetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
            Delay = TimeSpan.FromMilliseconds(500)
        });

        pipeline.AddTimeout(TimeSpan.FromSeconds(1));
    });*/

builder.Services.AddHostedService<ModerationCoordinatorBackgroundService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseErrorHandler();
app.UseAuthorization();
app.MapControllers();

app.Run();
