


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
}
app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

// Enable CORS
app.UseCors();
//app.UseCors("AllowAll");
app.UseAuthorization();

//app.MapIdentityApi<ApplicationUser>();

app.MapControllers();
app.UseExceptionHandler();

app.Run();

#endregion