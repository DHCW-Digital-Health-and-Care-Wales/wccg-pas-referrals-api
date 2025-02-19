using WCCG.PAS.Referrals.API.Configuration;
using WCCG.PAS.Referrals.API.Extensions;
using WCCG.PAS.Referrals.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

//IOptions
builder.Services.Configure<CosmosConfig>(builder.Configuration.GetSection("Cosmos"));
builder.Services.Configure<ManagedIdentityConfig>(builder.Configuration.GetSection("ManagedIdentity"));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var clientId = builder.Configuration.GetValue<string>("ManagedIdentity:ClientId")!;
builder.Services.AddApplicationInsights(builder.Environment, clientId);

builder.Services.AddCosmosClient(builder.Environment);
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
