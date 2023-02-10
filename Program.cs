using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Asp.Versioning.Conventions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using System.Xml.Linq;
using System.Xml.XPath;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerGenOptions>()
    // ExampleCode: Comment this out and uncomment the other example code and see that it works
    .AddTransient<IConfigureOptions<SwaggerUIOptions>, ConfigureSwaggerUIOptions>()
    ;
    
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

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

var versionSet = app.NewApiVersionSet().HasApiVersion(new ApiVersion(1.0)).HasApiVersion(new ApiVersion(2.0)).Build();
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
    .WithName("GetWeatherForecast").WithApiVersionSet(versionSet).MapToApiVersions(
    new ApiVersion[]{
    new (1.0),
    new (2.0)
}).ReportApiVersions();


app.MapGet("/{version:apiVersion}/versiondescriptor", (IApiVersionDescriptionProvider provider) =>
{
    return provider.ApiVersionDescriptions;
}).WithApiVersionSet(versionSet).MapToApiVersion(2.0);

app.MapGet("/versiondescriptor2", (IApiDescriptionGroupCollectionProvider provider) =>
{
    var descriptions = provider.ApiDescriptionGroups;
    
    // See that the descriptions include only one version
    System.Diagnostics.Debugger.Break();
    
    return "debug this for the result cause it freezes Swagger.";
}).WithApiVersionSet(versionSet).MapToApiVersion(1.0);

// See that there is only one version
var foo = app.DescribeApiVersions();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    // ExampleCode: Comment out the above line and uncomment the below line to see it work.
    // app.UseSwaggerUI(ConfigureSwaggerUI);
}

System.Diagnostics.Debugger.Break();
app.Run();

void ConfigureSwaggerUI(SwaggerUIOptions options)
{
     var descriptions = app.DescribeApiVersions();

     System.Diagnostics.Debugger.Break();
        // build a swagger endpoint for each discovered API version
    foreach (var description in descriptions)
        {
            var url = $"/swagger/{description.GroupName}/swagger.json";
            var name = description.GroupName.ToUpperInvariant();
            options.SwaggerEndpoint(url, name);
        }
}

internal record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}


/// <summary>
/// The <see cref="IConfigureOptions{SwaggerGenOptions}"/> implementation for configuring SwaggerUI
/// </summary>
internal class ConfigureSwaggerGenOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider apiVersionDescriptionProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigureSwaggerGenOptions"/> class.
    /// </summary>
    /// <param name="apiVersionDescriptionProvider">The <see cref="IApiVersionDescriptionProvider"/>.</param>
    public ConfigureSwaggerGenOptions(IApiVersionDescriptionProvider apiVersionDescriptionProvider)
    {
        this.apiVersionDescriptionProvider = apiVersionDescriptionProvider;
    }

    /// <summary>
    /// Configures the <see cref="SwaggerGenOptions"/> while the dependency is being injected.
    /// </summary>
    /// <param name="options">The <see cref="SwaggerGenOptions"/>.</param>
    public void Configure(SwaggerGenOptions options)
    {
        System.Diagnostics.Debugger.Break();
        // add a swagger document for each discovered API version
        // note: you might choose to skip or document deprecated API versions differently
        foreach (var description in this.apiVersionDescriptionProvider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion("Hello World Application", description));
        }

        var xmlDocumentation = Directory.GetFiles(AppContext.BaseDirectory, "*.xml");
        foreach (var xml in xmlDocumentation)
        {
            var doc = XDocument.Load(xml);
            options.IncludeXmlComments(() => new XPathDocument(doc.CreateReader()), true);
        }

    }

    private static OpenApiInfo CreateInfoForApiVersion(String title, ApiVersionDescription description)
    {
        var info = new OpenApiInfo()
        {
            Title = title,
            Version = description.ApiVersion.ToString(),
        };

        if (description.IsDeprecated)
        {
            info.Description += " This API version has been deprecated.";
        }

        return info;
    }
}


/// <summary>
/// The <see cref="IConfigureOptions{SwaggerGenOptions}"/> implementation for configuring SwaggerUI
/// </summary>
/// <remarks>
/// Middleware used during Dependency Injection to configure the <see cref="SwaggerUIOptions"/>.
/// </remarks>
internal class ConfigureSwaggerUIOptions : IConfigureOptions<SwaggerUIOptions>
{
    private IApiVersionDescriptionProvider provider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigureSwaggerUIOptions"/> class.
    /// </summary>
    /// <param name="provider">The <see cref="IApiVersionDescriptionProvider"/>.</param>
    public ConfigureSwaggerUIOptions(IApiVersionDescriptionProvider provider)
    {
        this.provider = provider;
    }

    /// <summary>
    /// Configures the <see cref="SwaggerUIOptions"/> while the dependency is being injected.
    /// </summary>
    /// <param name="options">The <see cref="SwaggerUIOptions"/>.</param>
    public void Configure(SwaggerUIOptions options)
    {
        System.Diagnostics.Debugger.Break();
        options.EnableTryItOutByDefault();

        // build a swagger endpoint for each discovered API version  
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                description.GroupName.ToUpperInvariant());
        }
    }
}
