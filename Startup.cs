using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Protacon.NetCore.WebApi.ApiKeyAuth;
using Swashbuckle.AspNetCore.Swagger;
using Updater.Domain;

namespace Updater
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<UpdaterDbContext>(options => options.UseInMemoryDatabase(databaseName: "db"));
            services.AddTransient<ImageUpdater>();
            services.AddMvc();

            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            if (isWindows)
            {
                services.AddTransient<ICommandLine, CommandLineWindows>();
            }
            else
            {
                services.AddTransient<ICommandLine, CommandLineBashLinux>();
            }

            services
                .AddAuthentication()
                .AddApiKeyAuth(options =>
                {
                    options.Keys = new List<string>{"apikeyfortesting"};
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

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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
