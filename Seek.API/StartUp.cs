using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Seek.API.Services.System;
using Seek.Core;
using Seek.EF;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;

namespace Seek.API
{
    public class StartUp
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        public StartUp(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Initialize SQLCipher
            SQLitePCL.Batteries_V2.Init();

            var projectRoot = Directory.GetCurrentDirectory();
            var dbFolderPath = Path.Combine(projectRoot, "Database");

            if (!Directory.Exists(dbFolderPath))
            {
                Directory.CreateDirectory(dbFolderPath);
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var encryptionKey = _configuration["EncryptionKey"];
            // Register the interceptor as a singleton
            services.AddSingleton<SqliteEncryptionInterceptor>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<SqliteEncryptionInterceptor>>();
                var encryptionKey = _configuration["EncryptionKey"];
                if (string.IsNullOrWhiteSpace(encryptionKey))
                {
                    Log.Error("Missing SQLite encryption key. Set SqliteEncryptionKey as an environment variable.");
                }

                return new SqliteEncryptionInterceptor(encryptionKey, logger);
            });
            // Register the DbContext with the interceptor
            services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
            {
                var interceptor = serviceProvider.GetRequiredService<SqliteEncryptionInterceptor>();

                options.UseSqlite(connectionString, sqliteOptions =>
                {
                    sqliteOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    sqliteOptions.CommandTimeout(600);
                });

                options.AddInterceptors(interceptor);
            });

            // Add services to the container
            services.AddCors();

            // ContextAccessor :-  For Station Access Filter
            services.AddHttpContextAccessor();

            // Injection of Repositories and Interfaces
            services.AddRepositoryServices();

            // Add AutoMapper
            services.AddAutoMapper(typeof(Mappings).Assembly);

            // Register DefaultDataService as a scoped service
            services.AddScoped<DefaultDataService>();

            // Add Versioning
            services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
            });

            // SwaggerGen
            services.AddVersionedApiExplorer(c => c.GroupNameFormat = "'v'VVV");
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            services.AddSwaggerGen();

            // JWT Authentication (to be added later)

            // Add controllers
            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IApiVersionDescriptionProvider provider)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Manually apply PRAGMA key before EF migration
                var conn = dbContext.Database.GetDbConnection();
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    var key = _configuration["EncryptionKey"];
                    cmd.CommandText = $"PRAGMA key = '{key}';";
                    cmd.ExecuteNonQuery();
                }

                // NOW apply migrations
                dbContext.Database.Migrate();

                // Seed default data
                var defaultDataService = scope.ServiceProvider.GetRequiredService<DefaultDataService>();
                defaultDataService.EnsureDefaultDataAsync().GetAwaiter().GetResult();
            }

            if (_environment.IsDevelopment())
            {
                Log.Information("System : Starting in development environment");
                app.UseDeveloperExceptionPage();

                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    foreach (var desc in provider.ApiVersionDescriptions)
                    {
                        c.SwaggerEndpoint($"/swagger/{desc.GroupName}/swagger.json", desc.GroupName.ToUpperInvariant());
                    }
                });
            }
            else
            {
                Log.Information("System : Starting in Production environment");
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            // First: Authentication (if configured later)
            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
