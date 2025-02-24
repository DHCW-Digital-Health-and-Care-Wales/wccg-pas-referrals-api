using Microsoft.Extensions.Options;
using WCCG.PAS.Referrals.API.Configuration;
using WCCG.PAS.Referrals.API.Configuration.OptionValidators;
using WCCG.PAS.Referrals.API.Converters;
using WCCG.PAS.Referrals.API.Extensions;
using WCCG.PAS.Referrals.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

//CosmosConfig
builder.Services.Configure<CosmosConfig>(builder.Configuration.GetSection(CosmosConfig.SectionName));
builder.Services.AddSingleton<IValidateOptions<CosmosConfig>, ValidateCosmosConfigOptions>();

//ManagedIdentityConfig
builder.Services.Configure<ManagedIdentityConfig>(builder.Configuration.GetSection(ManagedIdentityConfig.SectionName));
builder.Services.AddSingleton<IValidateOptions<ManagedIdentityConfig>, ValidateManagedIdentityConfigOptions>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new FhirBundleConverter());
        options.AllowInputFormatterExceptionMessages = false;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplicationInsights(builder.Environment.IsDevelopment(), builder.Configuration);

builder.Services.AddCosmosClient(builder.Environment.IsDevelopment());
builder.Services.AddCosmosRepositories();

builder.Services.AddServices();
builder.Services.AddValidators();


var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
