


using HangfireBasicAuthenticationFilter;

var builder = WebApplication.CreateBuilder(args);


// we will use hybrid cache strategy, we will use in-memory cache for the most frequently accessed data
// and distributed cache for the less frequently accessed data, and we will use Redis as a distributed cache provider
// we will install Microsoft.Extensions.Caching.Hybrid package to
// use the hybrid cache strategy and we will configure it in the startup class



//builder.Logging.AddConsole();

builder.Services.AddDependencies(builder.Configuration);
builder.Host.UseSerilog((context, configuration) =>

    // Read Serilog configuration from appsettings.json
    configuration.ReadFrom.Configuration(context.Configuration)
);

// Identity API Endpoints registration
//builder.Services.AddIdentityApiEndpoints<ApplicationUser>()
//    .AddEntityFrameworkStores<ApplicationDbContext>();

var app = builder.Build();

#region Pipeline

if (app.Environment.IsDevelopment())
{

    app.MapOpenApi();
    app.UseSwaggerUI(options=>options.SwaggerEndpoint("/openapi/v1.json","v1"));
    app.UseHangfireDashboard();

}
app.UseSerilogRequestLogging();

app.UseHttpsRedirection();


app.UseHangfireDashboard("/jobs", new DashboardOptions
{
    Authorization =
    [
        new HangfireCustomBasicAuthenticationFilter
        {
            User = app.Configuration.GetValue<string>("HangfireSettings:Username"),
            Pass = app.Configuration.GetValue<string>("HangfireSettings:Password")
        }
    ],
    DashboardTitle = "Survey Basket Dashboard",
    //IsReadOnlyFunc = (DashboardContext conext) => true
});

var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
using var scope = scopeFactory.CreateScope();
var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

RecurringJob.AddOrUpdate("SendNewPollNotification", () => notificationService.SendNewPollNotification(null), Cron.Daily);




// Enable CORS
app.UseCors();
//app.UseCors("AllowAll");
app.UseAuthorization();

//app.MapIdentityApi<ApplicationUser>();

app.MapControllers();
app.UseExceptionHandler();

app.Run();

#endregion