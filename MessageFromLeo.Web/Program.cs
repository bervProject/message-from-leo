using MessageFromLeo.Web;
using MessageFromLeo.Web.Components;
using MessageFromLeo.Mongo.Repositories;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var mongoUrl = builder.Configuration["MongoUrl"];
if (!string.IsNullOrWhiteSpace(mongoUrl))
{
    builder.Services.AddSingleton(new MongoClient(mongoUrl));
    builder.Services.AddSingleton<IMessageRepository, MessageRepository>();
    builder.Services.AddHostedService<ManageIndex>();
}


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
