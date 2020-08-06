using System;
using MassTransit;
using MassTransit.Definition;
using MassTransit.MongoDbIntegration.MessageData;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Sample.Components.Consumers;
using Sample.Contracts.Order;

namespace Sample.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.TryAddSingleton(KebabCaseEndpointNameFormatter.Instance);
            services.AddMassTransit(configurator =>
            {
                configurator.UsingRabbitMq((context, cfg) =>
                {
                    MessageDataDefaults.ExtraTimeToLive = TimeSpan.FromDays(1);
                    MessageDataDefaults.TimeToLive = TimeSpan.FromDays(7);
                    MessageDataDefaults.Threshold = 2000; //8000
                    MessageDataDefaults.AlwaysWriteToRepository = false;

                    cfg.UseMessageData(new MongoDbMessageDataRepository("mongodb://127.0.0.1", "attachments"));
                });

                configurator.AddRequestClient<SubmitOrder>(
                    new Uri($"queue:{KebabCaseEndpointNameFormatter.Instance.Consumer<SubmitOrderConsumer>()}")
                );

                configurator.AddRequestClient<CheckOrder>();
            });

            services.AddMassTransitHostedService();

            services.AddOpenApiDocument(cfg => cfg.PostProcess = d => d.Info.Title = "Sample");

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseOpenApi();
            app.UseSwaggerUi3();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}