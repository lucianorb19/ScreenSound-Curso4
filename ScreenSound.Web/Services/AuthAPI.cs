using ScreenSound.Web.Response;
using System.Net.Http.Json;

namespace ScreenSound.Web.Services;

//CLASSE JÁ INICALIZA COM O IHttpClientFactory
public class AuthAPI(IHttpClientFactory factory)
{
    private readonly HttpClient _httpClient = factory.CreateClient("API");

    
    public async Task<AuthResponse> LoginAsync(string email, string senha)
    {
        var response = await _httpClient.PostAsJsonAsync("auth/login?useCookies=true", new
        {
            email,
            password = senha
        });

        //SE A TENTATIVA DE LOGIN FOR BEM SUCEDIDA
        if (response.IsSuccessStatusCode)
        {   
            return new AuthResponse{Sucesso = true};
        }

        //SE NÃO
        return new AuthResponse{Sucesso = false, Erro="[Login/Senha inválido]" };
    }

   
}
