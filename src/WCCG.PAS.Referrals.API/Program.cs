using System.Text.Json;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using WCCG.PAS.Referrals.API.Configuration;
using WCCG.PAS.Referrals.API.Configuration.OptionValidators;
using WCCG.PAS.Referrals.API.Extensions;
using WCCG.PAS.Referrals.API.Middleware;
using WCCG.PAS.Referrals.API.Swagger;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationInsights(builder.Environment.IsDevelopment(), builder.Configuration);

//CosmosConfig
builder.Services.AddOptions<CosmosConfig>().Bind(builder.Configuration.GetRequiredSection(CosmosConfig.SectionName));
builder.Services.AddSingleton<IValidateOptions<CosmosConfig>, ValidateCosmosConfigOptions>();

//ManagedIdentityConfig
builder.Services.AddOptions<ManagedIdentityConfig>().Bind(builder.Configuration.GetSection(ManagedIdentityConfig.SectionName));
builder.Services.AddSingleton<IValidateOptions<ManagedIdentityConfig>, ValidateManagedIdentityConfigOptions>();

//BundleCreationConfig
builder.Services.AddOptions<BundleCreationConfig>().Bind(builder.Configuration.GetSection(BundleCreationConfig.SectionName));
builder.Services.AddSingleton<IValidateOptions<BundleCreationConfig>, ValidateBundleCreationConfigOptions>();

builder.Services.AddSingleton(new JsonSerializerOptions().ForFhirExtended());

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options => { options.OperationFilter<SwaggerOperationFilter>(); });
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

builder.Services.AddCosmosClient(builder.Environment.IsDevelopment(), builder.Configuration);
builder.Services.AddCosmosRepositories();

builder.Services.AddServices();
builder.Services.AddValidators();

builder.Services.AddVersioning();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseMiddleware<ResponseMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var descriptions = app.DescribeApiVersions();
        foreach (var description in descriptions)
        {
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
        }
    });
}

app.UseHealthChecks("/health");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
