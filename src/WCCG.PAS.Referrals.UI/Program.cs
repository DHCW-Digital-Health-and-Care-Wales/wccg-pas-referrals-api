using WCCG.PAS.Referrals.UI.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services
    .AddCosmosClient(builder.Configuration)
    .AddCosmosRepositories(builder.Configuration)
    .AddServices();

//Home page
builder.Services.AddMvc().AddRazorPagesOptions(o => { o.Conventions.AddPageRoute("/CosmosViewer", ""); });

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
