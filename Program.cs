using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Asp.Versioning.Conventions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApiVersioning().EnableApiVersionBinding()
    .AddApiExplorer(
    options =>
    {
        // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service  
        // note: the specified format code will format the version as "'v'major[.minor][-status]"  
        options.GroupNameFormat = "'v'VVV";

        // note: this option is only necessary when versioning by url segment. the SubstitutionFormat  
        // can also be used to control the format of the API version in route templates  
        options.SubstituteApiVersionInUrl = true;
    });
builder.Services.AddSwaggerGen();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

var versionSet = app.NewApiVersionSet().HasApiVersion(new ApiVersion(1.0)).Build();
var versionSet2 = app.NewApiVersionSet().HasApiVersion(new ApiVersion(2.0)).Build();
app.MapGet("/{version:apiVersion}/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateTime.Now.AddDays(index),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
    .WithName("GetWeatherForecast").WithApiVersionSet(versionSet).WithApiVersionSet(versionSet2).MapToApiVersions(
    new ApiVersion[]{
    new (1.0),
    new (2.0)
}).ReportApiVersions();


app.MapGet("/{version:apiVersion}/versiondescriptor", (IApiVersionDescriptionProvider provider) =>
{
    return provider.ApiVersionDescriptions;
}).WithApiVersionSet(versionSet);

app.MapGet("/versiondescriptor2", (IApiDescriptionGroupCollectionProvider provider) =>
{
    var descriptions = provider.ApiDescriptionGroups;
    
    // See that the descriptions include only one version
    System.Diagnostics.Debugger.Break();
    
    return "debug this for the result cause it freezes Swagger.";
}).WithApiVersionSet(versionSet);

// See that there is only one version
var foo = app.DescribeApiVersions();
System.Diagnostics.Debugger.Break();
app.Run();


internal record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}