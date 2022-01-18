using GreenPipes;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace MassTransitDeadLetterDemo
{
    public static class MassTransitSetup
    {
        public static IServiceCollection AddMassTransit(this IServiceCollection services)
        {

            services.AddMassTransit(x =>
            {
                x.UsingAmazonSqs((context, cfg) =>
                {
                    cfg.Host(new Uri($"amazonsqs://eu-central-1"), h => 
                    {
                        h.Config(new Amazon.SQS.AmazonSQSConfig() { RegionEndpoint = Amazon.RegionEndpoint.EUCentral1 });
                    });

                    cfg.ConfigureJsonSerializer(settings =>
                    {
                        settings.Formatting = Newtonsoft.Json.Formatting.None;
                        return settings;
                    });

                    cfg.ReceiveEndpoint("mass-transit-dead-letter-demo", x =>
                    {
                        x.Durable = true;
                        x.Consumer<Consumer>(context);
                        x.ConfigureMessageTopology<Message>(false);
                        x.WaitTimeSeconds = 20;

                        //It prevents from creating topics for dead letter queues
                        x.PublishFaults = false;

                        x.UseRetry(configurator => configurator.Immediate(3));
                    });

                    cfg.SendTopology.ConfigureErrorSettings = configurator => configurator.QueueAttributes.Add(
                        "MessageRetentionPeriod",
                        TimeSpan.FromDays(10).TotalSeconds);

                    cfg.SendTopology.ConfigureDeadLetterSettings = configurator => configurator.QueueAttributes.Add(
                        "MessageRetentionPeriod",
                        TimeSpan.FromDays(10).TotalSeconds);
                });
            });

            services.AddScoped<Consumer>();
            services.AddMassTransitHostedService();
            return services;
        }
    }
}
