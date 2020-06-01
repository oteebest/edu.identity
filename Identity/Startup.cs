// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Identity.Core.Models;
using Identity.Infrastructure.Entity;
using IdentityServer.Util;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace IdentityServer
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }

        public IConfiguration Configuration { get; }
        public Startup(IWebHostEnvironment environment, IConfiguration configuration )
        {
            Environment = environment;
            Configuration = configuration;
        }

        readonly string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

        public void ConfigureServices(IServiceCollection services)
        {
           
            string dbConnectionString = Configuration.GetConnectionString("DefaultConnection");

            var AllowCorsForList = Configuration.GetSection("AllowCorsForList").Value.Split(',');

            services.AddCors(options =>
            {
                options.AddPolicy(MyAllowSpecificOrigins,
                builder =>
                {
                    builder.WithOrigins(AllowCorsForList)
                                        .AllowAnyHeader()
                                        .AllowAnyMethod()
                                        .WithExposedHeaders("Content-Disposition", "Content-Length"); ;
                });
            });

            services.RegisterUtilCoreServices(Configuration);

            // uncomment, if you want to add an MVC-based UI
            services.AddControllersWithViews();

            var builder = services.AddIdentityServer()
                 //.AddInMemoryIdentityResources(Config.Ids)
                 //.AddInMemoryApiResources(Config.Apis)
                 //.AddInMemoryClients(Config.Clients)
                 .AddTestUsers(TestUsers.Users);

            // not recommended for production - you need to store your key material somewhere secure
            //builder.AddDeveloperSigningCredential();
            builder.AddSigningCredential(LoadCertificateFromStore());

            var migrationsAssembly = typeof(Startup)
                .GetTypeInfo().Assembly.GetName().Name;

            builder.AddConfigurationStore(options =>
            {
                options.ConfigureDbContext = builder =>
                    builder.UseNpgsql(dbConnectionString,
                    options => options.MigrationsAssembly(migrationsAssembly));
            });

            builder.AddOperationalStore(options =>
            {
                options.ConfigureDbContext = builder =>
                    builder.UseNpgsql(dbConnectionString,
                    options => options.MigrationsAssembly(migrationsAssembly));
            });
        }

        private X509Certificate2 LoadCertificateFromStore()
        {
            string thumbPrint = Configuration.GetSection("IdentityConfig:CertificateThumbPrint").Value;

            using (var store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
            {
                store.Open(OpenFlags.ReadOnly);
                var certCollection = store.Certificates.Find(X509FindType.FindByThumbprint,
                    thumbPrint, true);
                if (certCollection.Count == 0)
                {
                    throw new Exception("The specified certificate wasn't found.");
                }
                return certCollection[0];
            }
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                IdentityModelEventSource.ShowPII = true;
            }

            InitializeDatabase(app);

            // uncomment if you want to add MVC

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseCors(MyAllowSpecificOrigins);

            app.UseIdentityServer();

            // uncomment, if you want to add MVC
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }

        private void InitializeDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices
                .GetService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider
                    .GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

                var context = serviceScope.ServiceProvider
                    .GetRequiredService<ConfigurationDbContext>();
                context.Database.Migrate();
                if (!context.Clients.Any())
                {
                    foreach (var client in Config.Clients)
                    {
                        context.Clients.Add(client.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.IdentityResources.Any())
                {
                    foreach (var resource in Config.Ids)
                    {
                        context.IdentityResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.ApiResources.Any())
                {                 
                    foreach (var resource in Config.Apis)
                    {
                        context.ApiResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }
            }
        }

    }
}
