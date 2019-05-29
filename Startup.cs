using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Protacon.NetCore.WebApi.ApiKeyAuth;
using Protacon.NetCore.WebApi.Util.ModelValidation;
using Updater.Domain;

namespace Updater
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration config)
        {
            Configuration = config;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IK8sApi, K8sApi>();
            services.AddTransient<ImageUpdater>();
            services.Configure<AppSettings>(Configuration);

            services.AddMvc(options => options.Filters.Add(new ValidateModelAttribute()));

            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            services
                .AddAuthentication()
                .AddApiKeyAuth(options =>
                {
                    if (Configuration.GetChildren().All(x => x.Key != "apiKeys"))
                        throw new InvalidOperationException($"Expected 'apiKeys' section.");

                    var keys = Configuration.GetSection("apiKeys")
                        .AsEnumerable()
                        .Where(x => x.Value != null)
                        .Select(x => x.Value);

                    options.ValidApiKeys = keys;
                });

            services.AddSwaggerGen(c =>
            {
                var basePath = System.AppContext.BaseDirectory;

                c.SwaggerDoc("v1",
                    new OpenApiInfo
                    {
                        Title = "Kubernetes image updater",
                        Version = "v1",
                        Description = File.ReadAllText(Path.Combine(basePath, "README.md"))
                    });
                c.AddSecurityDefinition("ApiKey", ApiKey.OpenApiSecurityScheme);
                c.AddSecurityRequirement(ApiKey.OpenApiSecurityRequirement("ApiKey"));
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseAuthentication();

            app.UseMvc();

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Monitoring API");
                c.RoutePrefix = "doc";
            });
        }
    }
}
