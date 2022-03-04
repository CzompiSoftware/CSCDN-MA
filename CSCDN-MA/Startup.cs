using CSCDNMA.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CSCDNMA
{
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            //var db = args.Any() && args.ContainsName("connectionString") ? args.WithName("connectionString") : builder.Configuration["CzSoftDatabase"];

            services.AddDbContext<CzSoftCDNDatabaseContext>(options =>
            {
                //options.UseSqlServer(db);
                options.UseSqlServer("Data Source=srv;Initial Catalog=dbname;Persist Security Info=True;User ID=userid;Password=securepass");
            }, contextLifetime: ServiceLifetime.Transient, optionsLifetime: ServiceLifetime.Singleton);

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyOrigin();
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseCors();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
