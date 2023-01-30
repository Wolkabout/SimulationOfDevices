using System.Data.SqlClient;
using System.IO;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.MySql;
using HangfireBasicAuthenticationFilter;
using Microsoft.Extensions.FileProviders;
using SendDataToPlatfomAPI.Extensions;
using Serilog;
using SimulationOfDevices.API.Middleware;
using SimulationOfDevices.DAL;
using SimulationOfDevices.Services;
using SimulationOfDevices.Services.Common;


try
{
	var logger = new LoggerConfiguration().CreateLogger();

	var builder = WebApplication.CreateBuilder(args);

	builder.Host.UseSerilog((ctx, lc) => lc
		.WriteTo.Console()
		.ReadFrom.Configuration(builder.Configuration)
		.Enrich.FromLogContext());

	Log.Information("Starting up");

	builder.Services.AddHttpClient();
	builder.Services.AddServiceLayer(builder.Configuration);
    builder.Services.AddPersistanceLayer(builder.Configuration);
    builder.Services.AddAuth(builder.Configuration);    

    builder.Services.AddSwaggerExtension();
	builder.Services.AddControllersExtension();

	//Fluent validation register
	builder.Services.AddFluentValidationAutoValidation();
	builder.Services.AddValidatorsFromAssemblyContaining<IServiceMarker>();

	// CORS
	builder.Services.AddCorsExtension();
	//builder.Services.AddHealthChecks();

	#region Api Versioning
	//API version
	builder.Services.AddApiVersioningExtension();
	// API explorer
	builder.Services.AddMvcCore().AddApiExplorer();
	// API explorer version
	builder.Services.AddVersionedApiExplorerExtension();
	#endregion

	//API hosting json config
	builder.Host.ConfigureAppConfiguration((hostContext, config) =>
	{
#if DEBUG
		//config.AddJsonFile($"hosting.json", optional: true, reloadOnChange: true);
		config.AddJsonFile($"appsettings.Development.json", optional: true, reloadOnChange: true);
#else
		config.AddJsonFile($"hosting.Release.json", optional: true, reloadOnChange: true);
		config.AddJsonFile($"appsettings.Release.json", optional: true, reloadOnChange: true);
#endif
	});

	var connectionStringForHangFire = builder.Configuration.GetConnectionString("HangFireConnection");
	builder.Services.AddHangfire(configuration => configuration
		.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
		.UseSimpleAssemblyNameTypeSerializer()
		.UseRecommendedSerializerSettings()
		   .UseStorage(
				   new MySqlStorage(
					   connectionStringForHangFire,
					   new MySqlStorageOptions
					   {
						   QueuePollInterval = TimeSpan.FromSeconds(10),
						   JobExpirationCheckInterval = TimeSpan.FromHours(1),
						   CountersAggregateInterval = TimeSpan.FromMinutes(5),
						   PrepareSchemaIfNecessary = true,
						   DashboardJobListLimit = 25000,
						   TransactionTimeout = TimeSpan.FromMinutes(1),
						   TablesPrefix = "Hangfire",
					   }
				   )
			   ));

    builder.Services.AddHangfireServer(x =>
	{
		//Additional config for server
		//x.StopTimeout = TimeSpan.FromSeconds(2);		
		x.CancellationCheckInterval = TimeSpan.FromSeconds(1); //default is 5

	});

    //APPLICATION PART
    var app = builder.Build();

    app.CheckDbConnectionAndCreateTables();
    app.UseMiddleware<OperationCanceledMiddleware>();

	app.UseExceptionHandler("/error");

	//app.UseHttpsRedirection();
	app.UseRouting();
	//Enable CORS
	app.UseCors("AllowAll");
	//app.UseAuthentication();
	//app.UseAuthorization();
	app.UseSwaggerExtension();

	app.UseHangfireDashboard("/hangfire", new DashboardOptions
	{
		DashboardTitle = "Simulation devices",
		Authorization = new[]
		{
				new HangfireCustomBasicAuthenticationFilter{
					User = builder.Configuration.GetSection("HangfireSettings:UserName").Value,
					Pass = builder.Configuration.GetSection("HangfireSettings:Password").Value
				}
			}
	});

	//app.UseHangfireDashboard();
	
	app.UseEndpoints(endpoints =>
	{
		endpoints.MapControllers();
		endpoints.MapHangfireDashboard();
	});
   
    app.Run();
}
catch (SqlException)
{
    Log.Fatal("SQL Connection FAILED, Check Connection string!!");
}
catch (Exception ex)
{
	Log.Fatal(ex, "Unhandled exception");
}
finally
{
	Log.Information("Shut down complete");
	Log.CloseAndFlush();
}