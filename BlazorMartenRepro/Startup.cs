using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BlazorMartenRepro.Data;
using Marten;
using Microsoft.AspNetCore.Http;
using Weasel.Postgresql;

namespace BlazorMartenRepro
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
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddSingleton<WeatherForecastService>();
            
            services.AddMarten(options =>
            {
                options.Connection(this.Configuration.GetConnectionString("Marten"));
                options.AutoCreateSchemaObjects = AutoCreate.All;
            });
        }
        
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }
            
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.Map("/test", async context =>
                {
                    var store = context.RequestServices.GetRequiredService<IDocumentStore>();
                    
                    using (var session = store.LightweightSession())
                    {
                        var user = new User { FirstName = "Han", LastName = "Solo" };
                        
                        session.Store(user);

                        await session.SaveChangesAsync();
                    }
                    
                    using (var session = store.QuerySession())
                    {
                        var existing = await session
                            .Query<User>()
                            .SingleAsync(x => x.FirstName == "Han" && x.LastName == "Solo");

                        await context.Response.WriteAsJsonAsync(existing);
                    }
                });
                
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}