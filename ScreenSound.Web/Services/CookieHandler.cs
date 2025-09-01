
using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace ScreenSound.Web.Services
{
    public class CookieHandler : DelegatingHandler
    {
        //SOBRESCRITA DE MÉTODO QUE PASSA TODAS AS CREDENCIAIS DO NAVEGADOR
        //PARA A PRÓXIMA REQUISIÇÃO
        //PESSOA LOGA UMA VEZ, PERMANECE LOGADA
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            return base.SendAsync(request, cancellationToken);
        }
    }
}
