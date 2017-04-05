using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using IdentityServer.Data;
using IdentityServer.Models;
using IdentityServer.Services;
using System.Reflection;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using IdentityModel;

namespace IdentityServer
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see https://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets<Startup>();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddMvc();

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();

            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            //requires using System.Reflection;
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            // configure Identity Server with EF stores for users, clients and resources
            services.AddIdentityServer()
                .AddTemporarySigningCredential()
                .AddAspNetIdentity<ApplicationUser>()
                .AddConfigurationStore(builder =>
                    builder.UseSqlServer(connectionString, options =>
                        options.MigrationsAssembly(migrationsAssembly)))
                .AddOperationalStore(builder =>
                    builder.UseSqlServer(connectionString, options =>
                        options.MigrationsAssembly(migrationsAssembly)));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            // this will do the initial DB population
            InitializeDatabase(app);

            app.UseIdentity();

            // Add external authentication middleware below. To configure them please see https://go.microsoft.com/fwlink/?LinkID=532715
            app.UseIdentityServer();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private void InitializeDatabase(IApplicationBuilder app) {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope()) {
                //let's first ensure that the DB schema matches our DbContexts
                serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.Migrate();
                //requires using IdentityServer4.EntityFramework.DbContexts;
                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                context.Database.Migrate();

                //now let's add the Clients
                foreach (var client in Config.GetClients()) {
                    var existingClient = context.Clients.FirstOrDefault(c => c.ClientId == client.ClientId);
                    if (existingClient != null)
                        context.Clients.Remove(existingClient);
                    //requires using IdentityServer4.EntityFramework.Mappers;
                    context.Clients.Add(client.ToEntity());
                }
                context.SaveChanges();

                //let's add the IdentityResources
                foreach (var resource in Config.GetIdentityResources()) {
                    var existingIdentityResource = context.IdentityResources.FirstOrDefault(c => c.Name == resource.Name);
                    if (existingIdentityResource != null)
                        context.IdentityResources.Remove(existingIdentityResource);
                    context.IdentityResources.Add(resource.ToEntity());
                }
                context.SaveChanges();

                //let's add the ApiResources
                foreach (var resource in Config.GetApiResources()) {
                    var existingApiResource = context.ApiResources.FirstOrDefault(c => c.Name == resource.Name);
                    if (existingApiResource != null)
                        context.ApiResources.Remove(existingApiResource);
                    context.ApiResources.Add(resource.ToEntity());
                }
                context.SaveChanges();

                //let's use Microsoft Identity Framework to add a couple of users
                //requires using Microsoft.AspNetCore.Identity;
                var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                foreach (var user in Config.GetUsers()) {
                    userManager.CreateAsync(user, "Pa$$w0rd").Wait();
                    //requires using System.Security.Claims;
                    //requires using IdentityModel;
                    userManager.AddClaimAsync(user, new Claim(JwtClaimTypes.Name, user.Email, JwtClaimTypes.Name)).Wait();
                }
            }
        }
    }
}
