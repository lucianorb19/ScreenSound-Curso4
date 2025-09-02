using Microsoft.AspNetCore.Components.Authorization;
using ScreenSound.Web.Response;
using System.Net.Http.Json;
using System.Security.Claims;

namespace ScreenSound.Web.Services;

//CLASSE JÁ INICALIZA COM O IHttpClientFactory
public class AuthAPI(IHttpClientFactory factory) : AuthenticationStateProvider
{
    //PROPRIEDADES - CAMPOS
    private readonly HttpClient _httpClient = factory.CreateClient("API");
    private bool autenticado = false;

    //DEMAIS MÉTODOS

    //CONFIGURAÇÃO ESTADO DE AUTENTICAÇÃO
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        autenticado = false;
        
        //ClaimsPrincipal VAZIO - PESSOA SEM AUTENTICAÇÃO / SEM ESTADO DE AUTENTICAÇÃO
        var pessoa = new ClaimsPrincipal();

        //Rota "auth/manage/info" DO IDENTITY RETORNA SE USUÁRIO ESTÁ LOGADO OU NÃO
        var response = await _httpClient.GetAsync("auth/manage/info");

        //SE HOUVER USUÁRIO LOGADO
        if (response.IsSuccessStatusCode)
        {
            //SALVO INFORMAÇÕES DO USUÁRIO EM OBJETO InfoPessoaResponse info
            var info = await response.Content.ReadFromJsonAsync<InfoPessoaResponse>();
        
            //CRIO VETOR DE CLAIMs COM DUS CLAIMs
            Claim[] dados = 
                [
                    //A PRIMEIRA É PADRÃO DA MICROSOFT Name - EMAIL
                    //EMAIL NESSE ESTADO É A PRINCIPAL INFORMAÇÃO DE AUTENTICAÇÃO
                    new Claim(ClaimTypes.Name, info.Email),
                    //SEGUNDA CLAIM É O PRÓPRIO EMAIL
                    new Claim(ClaimTypes.Email, info.Email)
                ];
            //OBJETO ClaimIdentity QUE AGRUPA TODOS OS DADOS DA PESSOA
            //E INDICA TAMBÉM NUMA STRING QUAL TIPO DE AUTENTICAÇÃO ESTÁ SENDO USADA
            var identity = new ClaimsIdentity(dados, "Cookies");

            //COM TODAS AS INFORMAÇÕES PREENCHIDAS
            //PREENCHO O OBJETO ClaimsPrincipal
            //ClaimsPrincipal PREENCHIDO - PESSOA AUTENTICADA / ESTADO DE AUTENTICAÇÃO OK
            pessoa = new ClaimsPrincipal(identity);
            autenticado = true;
        }

        //RETORNO ESTADO DE AUTENTICAÇÃO 
        //VAZIO OU AUTENTICADO - A DEPENDER SE HÁ USUÁRIO LOGADO NO SISTEMA
        return new AuthenticationState(pessoa);
    }


    //LOGIN IDENTITY NA API
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
            //MUDANDA ESTADO DE AUTENTICAÇÃO
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

            return new AuthResponse{Sucesso = true};
        }

        //SE NÃO
        return new AuthResponse{Sucesso = false, Erro="[Login/Senha inválido]" };
    }

    //MÉTODO DE LOGOUT
    public async Task LogoutAsync()
    {
        await _httpClient.PostAsync("auth/logout", null);
        //MUDANDA ESTADO DE AUTENTICAÇÃO
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    //MÉTODO QUE VERIFICA SE USUÁRIO ESTÁ AUTENTICADO OU NÃO E RETORNA UM bool
    public async Task<bool> VerificaAutenticado()
    {
        await GetAuthenticationStateAsync();
        return autenticado;
    }

}
