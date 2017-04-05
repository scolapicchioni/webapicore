using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MarketPlaceService.Data;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Swagger;

namespace MarketPlaceService
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // requires using Microsoft.EntityFrameworkCore;
            // and using MarketPlaceService.Data;
            services.AddDbContext<MarketPlaceContext>(opt => opt.UseInMemoryDatabase());

            services.AddCors(options =>
                options.AddPolicy("default", policy =>
                    policy.WithOrigins("http://localhost:5001")
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                )
            );

            // Add framework services.
            services.AddMvc();

            // requires using MarketPlaceService.Models;
            services.AddSingleton<IProductsRepository, ProductsRepository>();

            ///requires using Swashbuckle.AspNetCore.Swagger;
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "MarketPlace APIs", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseCors("default");

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "MarketPlace API V1");
            });

            app.UseIdentityServerAuthentication(new IdentityServerAuthenticationOptions {
                Authority = "http://localhost:5002",
                RequireHttpsMetadata = false,

                ApiName = "MarketplaceService"
            });

            app.UseMvc();
        }
    }
}
