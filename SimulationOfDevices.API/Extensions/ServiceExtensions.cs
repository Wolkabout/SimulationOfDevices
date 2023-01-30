using System.Data;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using SimulationOfDevices.API.Filters;
using SimulationOfDevices.DAL;
using SimulationOfDevices.Services.Common.Authentication;
using static System.Net.Mime.MediaTypeNames;

namespace SendDataToPlatfomAPI.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddSwaggerExtension(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Simulation of devices",
                    Description = "This Api will be responsible for sending data to platform",
                });
                //c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                //{
                //    Name = "Authorization",
                //    In = ParameterLocation.Header,
                //    Type = SecuritySchemeType.ApiKey,
                //    Scheme = "Bearer",
                //    BearerFormat = "JWT",
                //    Description = "Input your Bearer token in this format - Bearer {your token here} to access this API",
                //});
                //c.AddSecurityRequirement(new OpenApiSecurityRequirement
                //{
                //    {
                //        new OpenApiSecurityScheme
                //        {
                //            Reference = new OpenApiReference
                //            {
                //                Type = ReferenceType.SecurityScheme,
                //                Id = "Bearer",
                //            },
                //            Scheme = "Bearer",
                //            Name = "Bearer",
                //            In = ParameterLocation.Header,
                //        }, new List<string>()
                //    },
                //});
            });

            return services;
        }

        public static IServiceCollection AddControllersExtension(this IServiceCollection services)
        {
            services.AddControllers(cfg =>
            {
                cfg.Filters.Add(typeof(ExceptionFilterHandler));
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            });

            return services;
        }

        public static IServiceCollection AddCorsExtension(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyHeader()
                           .AllowAnyMethod();
                });
            });

            return services;
        }


        public static IServiceCollection AddVersionedApiExplorerExtension(this IServiceCollection services)
        {
            services.AddVersionedApiExplorer(o =>
            {
                o.GroupNameFormat = "'v'VVV";
                o.SubstituteApiVersionInUrl = true;
            });

            return services;
        }

        public static IServiceCollection AddApiVersioningExtension(this IServiceCollection services)
        {
            services.AddApiVersioning(config =>
            {
                // Specify the default API Version as 1.0
                config.DefaultApiVersion = new ApiVersion(1, 0);
                // If the client hasn't specified the API version in the request, use the default API version number 
                config.AssumeDefaultVersionWhenUnspecified = true;
                // Advertise the API versions supported for the particular endpoint
                config.ReportApiVersions = true;
            });

            return services;
        }

        public static IServiceCollection AddAuth(this IServiceCollection services, ConfigurationManager configuration)
        {
            var jwtSettings = new JwtSettings();
            configuration.Bind(JwtSettings.SectionName, jwtSettings);

            services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
            services.AddSingleton(Options.Create(jwtSettings));
            services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

            services.AddAuthentication(defaultScheme: JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
                });

            return services;

        }

        public static void UseSwaggerExtension(this IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("./v1/swagger.json", "SimulationOfDevices");
            });
        }


        public static void CheckDbConnectionAndCreateTables(this IApplicationBuilder app)
        {
            try
            {
                using (var scope = app.ApplicationServices.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();

                    var command = dbContext.Database.GetDbConnection().CreateCommand();

                    command.CommandText = "SELECT 1";
                    command.CommandType = CommandType.Text;
                    dbContext.Database.OpenConnection();
                    command.ExecuteScalar();
                    Log.Information("SQL Connection successful.");

                    string sCurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    string sFile = Path.Combine(sCurrentDirectory, @"setup.sql");
                    string sFilePath = Path.GetFullPath(sFile);

                    var sqlScript = File.ReadAllText(sFilePath);
                    
                    var countRow = dbContext.Database.ExecuteSqlRaw(sqlScript);
                    if (countRow == 0)
                    {
                        Log.Information("SQL tables already exist.");
                        
                    }
                    else 
                    {
                        Log.Information("SQL tables created.");
                    }                    
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
