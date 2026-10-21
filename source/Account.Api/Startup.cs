using Account.Data.Loan;
using Account.Data.Providers;
using Account.Data.Queries;
using Account.Data.Repositories;
using Account.Data.Stores;
using Account.Data.UnitOfWork;
using Account.Domain.Commands.Biometrics;
using Account.Domain.Framework.Layers.Data.Repositories;
using Account.Domain.Framework.Layers.Data.UnitOfWork;
using Account.Domain.Framework.Layers.Domain.Command;
using Account.Domain.Framework.Layers.Domain.Validations;
using Account.Domain.Providers;
using Account.Domain.Queries.Biometrics;
using Account.Domain.Responses;
using Account.Domain.Services;
using Account.Tasks.Jobs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

namespace Account.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddSingleton<BiometricVerificationStore>();
            services.AddScoped<IUnitOfWork, InMemoryUnitOfWork>();
            services.AddScoped<IRepository, Repository>();
            services.AddScoped<IBiometricVerificationQuery, BiometricVerificationQuery>();
            services.AddScoped<IBiometricProvider, FakeBiometricProvider>();
            services.AddScoped<ILoanResources, InMemoryLoanResources>();
            services.AddScoped<IMediator, Mediator>();
            services.AddScoped<IRequestHandler<ProcessBiometricVerificationCommand, BiometricVerificationResponse>, ProcessBiometricVerificationCommandHandler>();
            // Removido: AddSingleton<IScheduledJob, SyncBiometricVerificationsJob>()
            services.AddScoped<SyncBiometricVerificationsJob>();
            services.AddHostedService<ScheduledJobHostedService>();
            }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.Use(async (context, next) =>
            {
                try
                {
                    await next();
                }
                catch (BusinessException exception)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(JsonSerializer.Serialize(new { message = exception.Message }));
                }
            });

            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
