﻿using System.Security;
using System.Text.Json;
using Asp.Versioning;
using Codebelt.Bootstrapper.Web;
using Codebelt.Shared;
using Codebelt.StatusMonitor.InMemory;
using Codebelt.StatusMonitor.RestApi.Controllers.V1;
using Cuemon.Diagnostics;
using Cuemon.Extensions.Asp.Versioning;
using Cuemon.Extensions.AspNetCore.Diagnostics;
using Cuemon.Extensions.AspNetCore.Hosting;
using Cuemon.Extensions.AspNetCore.Mvc.Filters;
using Cuemon.Extensions.AspNetCore.Mvc.Filters.Diagnostics;
using Cuemon.Extensions.AspNetCore.Mvc.Formatters.Text.Json;
using Cuemon.Extensions.DependencyInjection;
using Cuemon.Extensions.Hosting;
using Cuemon.Extensions.Swashbuckle.AspNetCore;
using Cuemon.Extensions.Text.Json.Formatters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Savvyio.Extensions;
using Savvyio.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Codebelt.StatusMonitor.RestApi
{
    public class Startup : WebStartup
    {
        private ILogger _logger;

        public Startup(IConfiguration configuration, IHostEnvironment environment) : base(configuration, environment)
        {
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services
                .AddRouting(o => o.LowercaseUrls = true)
                .AddServerTiming(o => o.UseTimeMeasureProfiler = true)
                .AddControllers(o =>
                {
                    o.Filters.AddServerTiming();
                    o.Filters.AddFaultDescriptor();
                })
                .AddApplicationPart(typeof(StatusController).Assembly)
                .AddJsonFormatters()
                .AddFaultDescriptorOptions(o =>
                {
                    o.HttpFaultResolvers.AddHttpFaultResolver<SecurityException>(StatusCodes.Status401Unauthorized);
                    o.ExceptionCallback = (_, exception, descriptor) =>
                    {
                        _logger.LogError(exception, descriptor.ToString());
                    };
                });

            services.AddRestfulApiVersioning(o =>
            {
                o.Conventions.Controller<StatusController>().HasApiVersion(new ApiVersion(1, 0));
            });

            services.AddRestfulSwagger(o =>
            {
                o.OpenApiInfo.Title = "Status Monitor API";
                o.OpenApiInfo.Description = "An API tailored to support non-blocking RESTful APIs where actual work is offloaded to one or more worker services. Ideal for Microservices and the likes hereof.";
                o.Settings.UseAllOfToExtendReferenceSchemas();
                o.Settings.AddXApiKeySecurity();
                o.JsonSerializerOptionsFactory = provider => new JsonSerializerOptions(provider.GetRequiredService<IOptions<JsonFormatterOptions>>().Value.Settings);
            });

            services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService(nameof(StatusMonitor)))
                .WithTracing(tracing => tracing
                    .AddAspNetCoreInstrumentation())
                .WithMetrics(metrics => metrics
                    .AddAspNetCoreInstrumentation());

            services.AddSavvyIO(o =>
            {
                o.EnableHandlerServicesDescriptor()
                    .UseAutomaticDispatcherDiscovery(true)
                    .UseAutomaticHandlerDiscovery(true)
                    .AddMediator<Mediator>();
            });


            if (Environment.IsLocalDevelopment())
            {
                services.Add<InMemoryTenantDataStore>(o => o.Lifetime = ServiceLifetime.Singleton);
                services.Add<InMemoryStatusMonitorDataStore>(o => o.Lifetime = ServiceLifetime.Singleton);
            }
            
            services.PostConfigureAllOf<IExceptionDescriptorOptions>(o => o.SensitivityDetails = Environment.IsNonProduction() ? FaultSensitivityDetails.All : FaultSensitivityDetails.None);
        }

        public override void Configure(IApplicationBuilder app, ILogger logger)
        {
            _logger = app.ApplicationServices.GetRequiredService<ILogger<WebStartup>>();

            if (Environment.IsNonProduction())
            {
                if (Environment.IsLocalDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }
            }
            else
            {
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            app.UseHandlerServicesDescriptor();

            app.UseFaultDescriptorExceptionHandler();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHostingEnvironment();

            app.UseCors(builder =>
            {
                builder.AllowAnyHeader();
                builder.AllowAnyMethod();
                builder.AllowAnyOrigin();
            });

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
