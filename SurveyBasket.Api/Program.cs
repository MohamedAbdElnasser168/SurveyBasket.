


var builder = WebApplication.CreateBuilder(args);

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