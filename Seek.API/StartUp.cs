using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Seek.API.Services.System;
using Seek.Core;
using Seek.EF;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Data.Common;
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
                if (string.IsNullOrWhiteSpace(encryptionKey))
                {
                    Log.Error("Missing SQLite encryption key. Set SQLite Encryption Key as an environment variable.");
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
            services.AddHttpContextAccessor();
            services.AddRepositoryServices();
            services.AddAutoMapper(typeof(Mappings).Assembly);
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

            // Add controllers
            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IApiVersionDescriptionProvider provider)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var conn = dbContext.Database.GetDbConnection();
                conn.Open();

                // First, set the PRAGMA key for encryption
                var encryptionKey = _configuration["EncryptionKey"];
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"PRAGMA key = '{encryptionKey}';";
                    cmd.ExecuteNonQuery();
                }

                // Now check if the database is encrypted
                if (!IsDatabaseEncrypted(conn))
                {
                    Log.Warning("Database is not encrypted. Attempting to encrypt...");
                    EncryptDatabase(conn, encryptionKey);

                    // Move the current database to a backup folder with timestamp
                    var backupFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Database_Backups");
                    if (!Directory.Exists(backupFolderPath))
                    {
                        Directory.CreateDirectory(backupFolderPath);
                    }

                    var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                    var backupFilePath = Path.Combine(backupFolderPath, $"backup_{timestamp}.db");

                    var currentDbFilePath = new Uri(conn.ConnectionString).LocalPath;
                    if (File.Exists(currentDbFilePath))
                    {
                        File.Copy(currentDbFilePath, backupFilePath);
                        Log.Information($"Database backup created at: {backupFilePath}");
                    }

                    // Optionally, handle further failure here (perhaps stop execution)
                    throw new Exception("Database is not encrypted, backup created.");
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

        private bool IsDatabaseEncrypted(DbConnection connection)
        {
            using (var command = connection.CreateCommand())
            {
                // SQLite command to check if the database is encrypted
                command.CommandText = "PRAGMA cipher_version;";
                try
                {
                    var result = command.ExecuteScalar();
                    return result != null;
                }
                catch (Exception ex)
                {
                    Log.Error("Error checking database encryption: " + ex.Message);
                    return false; // If it fails, the database is not encrypted
                }
            }
        }
        private void EncryptDatabase(DbConnection connection, string key)
        {
            using var command = connection.CreateCommand();
            command.CommandText = $"PRAGMA rekey = '{key}';";
            command.ExecuteNonQuery();
            Log.Information("Database encryption completed successfully.");
        }
    }
}
