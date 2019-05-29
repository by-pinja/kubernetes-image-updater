using System;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Protacon.NetCore.WebApi.ApiKeyAuth;
using Protacon.NetCore.WebApi.Util.ModelValidation;
using Swashbuckle.AspNetCore.Swagger;
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
                    if(Configuration.GetChildren().All(x => x.Key != "apiKeys"))
                        throw new InvalidOperationException($"Expected 'apiKeys' section.");

                    var keys = Configuration.GetSection("apiKeys")
                        .AsEnumerable()
                        .Where(x => x.Value != null)
                        .Select(x => x.Value);

                    options.ValidApiKeys = keys;
                });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1",
                    new Info
                    {
                        Title = "Kubernetes image updater",
                        Version = "v1"
                    });
                c.OperationFilter<ApplyApiKeySecurityToDocument>();
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
