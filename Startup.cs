using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
        public IConfiguration Configuration { get; private set; }

        public Startup(IConfiguration config)
        {
            Configuration = config;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<UpdaterDbContext>(options => options.UseInMemoryDatabase(databaseName: "db"));
            services.AddTransient<ImageUpdater>();
            services.Configure<AppSettings>(Configuration);

            services.AddMvc(options =>
            {
                options.Filters.Add(new ValidateModelAttribute());
            });

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
                    if(Configuration.GetChildren().All(x => x.Key != "apiKeys"))
                        throw new InvalidOperationException($"Expected 'apiKeys' section.");

                    var keys = Configuration.GetSection("apiKeys")
                        .AsEnumerable()
                        .Where(x => x.Value != null)
                        .Select(x => x.Value);

                    options.Keys = keys;
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
