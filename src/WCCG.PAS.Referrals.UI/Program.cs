using WCCG.PAS.Referrals.UI.Configs;
using WCCG.PAS.Referrals.UI.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.Configure<CosmosConfig>(builder.Configuration.GetSection("Cosmos"));
builder.Services.Configure<ManagedIdentityConfig>(builder.Configuration.GetSection("ManagedIdentity"));

var clientId = builder.Configuration.GetValue<string>("ManagedIdentity:ClientId")!;
builder.Services.AddApplicationInsights(builder.Environment, clientId);

builder.Services.AddCosmosClient(builder.Environment);
builder.Services.AddCosmosRepositories();
builder.Services.AddValidators();
builder.Services.AddServices();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
