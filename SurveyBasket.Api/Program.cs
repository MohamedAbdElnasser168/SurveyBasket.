


using Microsoft.Extensions.Configuration;
using SurveyBasket.Api.Persistence;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDependencies(builder.Configuration);
// Identity API Endpoints registration
//builder.Services.AddIdentityApiEndpoints<ApplicationUser>()
//    .AddEntityFrameworkStores<ApplicationDbContext>();

var app = builder.Build();

#region Pipeline
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options=>options.SwaggerEndpoint("/openapi/v1.json","v1"));
}

app.UseHttpsRedirection();

// Enable CORS
app.UseCors();
//app.UseCors("AllowAll");
app.UseAuthorization();

//app.MapIdentityApi<ApplicationUser>();

app.MapControllers();

app.Run();

#endregion