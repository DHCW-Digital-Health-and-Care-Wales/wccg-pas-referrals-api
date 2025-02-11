using WCCG.PAS.Referrals.UI.Configs;
using WCCG.PAS.Referrals.UI.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.Configure<CosmosConfig>(builder.Configuration.GetSection("Cosmos"));

builder.Services.AddCosmosClient(builder.Environment)
    .AddCosmosRepositories()
    .AddValidators()
    .AddServices();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
