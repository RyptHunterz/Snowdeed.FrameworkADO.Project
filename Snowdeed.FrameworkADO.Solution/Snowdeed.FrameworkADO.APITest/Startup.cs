using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Snowdeed.FrameworkADO.APITest
{
    using System;
    using Snowdeed.FrameworkADO.APITest.Core;
    using Snowdeed.FrameworkADO.APITest.DbModels;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddCors();

            services.AddOpenApiDocument();

            this.ConfigureDependencyInjection(services);
        }

        private void ConfigureDependencyInjection(IServiceCollection services)
        {
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseCors(c =>
                {
                    c.AllowAnyHeader();
                    c.AllowAnyMethod();
                    c.AllowAnyOrigin();
                });
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseOpenApi();
            app.UseSwaggerUi3();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            using (MovieDbContext context = new MovieDbContext(Configuration.GetConnectionString("DefaultConnection")))
            {
                context.CreateDatabase();
                context.CreateTable();

                if (env.IsDevelopment())
                {
                    context.MovieGenre.Add(new MovieGenre() { ID = 1, Genre_Name = "Action", Create_At = DateTime.Now, Create_By = "Admin" });
                    context.MovieGenre.Add(new MovieGenre() { ID = 2, Genre_Name = "Aventure", Create_At = DateTime.Now, Create_By = "Admin" });
                    context.MovieGenre.Add(new MovieGenre() { ID = 3, Genre_Name = "Comédie", Create_At = DateTime.Now, Create_By = "Admin" });
                    context.MovieGenre.Add(new MovieGenre() { ID = 4, Genre_Name = "Documentaire", Create_At = DateTime.Now, Create_By = "Admin" });
                    context.MovieGenre.Add(new MovieGenre() { ID = 5, Genre_Name = "Drame", Create_At = DateTime.Now, Create_By = "Admin" });
                    context.MovieGenre.Add(new MovieGenre() { ID = 6, Genre_Name = "Fantastique", Create_At = DateTime.Now, Create_By = "Admin" });
                    context.MovieGenre.Add(new MovieGenre() { ID = 7, Genre_Name = "Science-fiction", Create_At = DateTime.Now, Create_By = "Admin" });

                    context.Movie.Add(new Movie() { ID = Guid.NewGuid(), Title = "Les Gardiens de la Galaxie", TitleOriginal = "Guardians of the Galaxy", TheatricalRelease = new DateTime(2014,08,13), CommercialRelease = new DateTime(2014,12,30), Origin = "Etats-Unis", GenreID = 7, Create_At = DateTime.Now, Create_By = "Admin" });
                }
            }
        }
    }
}