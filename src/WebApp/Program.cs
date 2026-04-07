using Microsoft.Extensions.AI;
using WebApp;
using WebApp.Components;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.AddAzureChatCompletionsClient("chat").AddChatClient();
builder.Services.AddScoped<CallingFunctions>();
builder.Services.AddKeyedScoped(nameof(CallingFunctions), (provider, key) =>
{
    var functions = provider.GetRequiredService<CallingFunctions>();
    var baseClient = provider.GetRequiredService<IChatClient>();
    return baseClient.AsBuilder()
       .ConfigureOptions(x =>
       {
           x.Tools = [functions.GetWeatherTool, functions.TestExceptionTool, functions.GetTimeTool];
       })
     .UseFunctionInvocation()
     .Build();
});

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
