using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using WCCG.PAS.Referrals.API.Configuration;
using WCCG.PAS.Referrals.API.Configuration.OptionValidators;
using WCCG.PAS.Referrals.API.Extensions;
using WCCG.PAS.Referrals.API.Middleware;
using WCCG.PAS.Referrals.API.Swagger;

var builder = WebApplication.CreateBuilder(args);

//CosmosConfig
builder.Services.AddOptions<CosmosConfig>().Bind(builder.Configuration.GetRequiredSection(CosmosConfig.SectionName));
builder.Services.AddSingleton<IValidateOptions<CosmosConfig>, ValidateCosmosConfigOptions>();

//ManagedIdentityConfig
builder.Services.AddOptions<ManagedIdentityConfig>().Bind(builder.Configuration.GetSection(ManagedIdentityConfig.SectionName));
builder.Services.AddSingleton<IValidateOptions<ManagedIdentityConfig>, ValidateManagedIdentityConfigOptions>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => { options.OperationFilter<SwaggerJsonTextRequestOperationFilter>(); });
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

builder.Services.AddApplicationInsights(builder.Environment.IsDevelopment(), builder.Configuration);

builder.Services.AddCosmosClient(builder.Environment.IsDevelopment());
builder.Services.AddCosmosRepositories();

builder.Services.AddServices();
builder.Services.AddValidators();

builder.Services.AddVersioning();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

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

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
