using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using ScreenSound.Web;
using ScreenSound.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();

//SERVIÇOS ESTADO DE AUTENTICAÇÃO
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, AuthAPI>();
builder.Services.AddScoped<AuthAPI>(sp => (AuthAPI) 
    sp.GetRequiredService<AuthenticationStateProvider>());


builder.Services.AddScoped<CookieHandler>();
builder.Services.AddScoped<ArtistaAPI>();
builder.Services.AddScoped<MusicaAPI>();

//CONFIGURANDO CLIENTE HTTP CHAMADO API
builder.Services.AddHttpClient("API",client => {
    client.BaseAddress = new Uri(builder.Configuration["APIServer:Url"]!);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
}).AddHttpMessageHandler<CookieHandler>();//CONFIGURAÇÃO DE COOKIE

await builder.Build().RunAsync();
