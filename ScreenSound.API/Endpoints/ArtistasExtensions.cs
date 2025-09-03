using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using ScreenSound.API.Requests;
using ScreenSound.API.Response;
using ScreenSound.Banco;
using ScreenSound.Modelos;
using ScreenSound.Shared.Dados.Modelos;
using System.Security.Claims;

namespace ScreenSound.API.Endpoints;

public static class ArtistasExtensions
{
    public static void AddEndPointsArtistas(this WebApplication app)
    {
        //VARIÁVEL groupBuilder QUE AGRUPA TODAS ROTAS COMEÇANDO COM artistas
        //JÁ REQUERINDO AUTORIZAÇÃO
        //E ORGANIZANDO COM TAG artistas
        var groupBuilder = app.MapGroup("artistas")
            .RequireAuthorization()
            .WithTags("artistas");

        #region Endpoint Artistas
        groupBuilder.MapGet("", ([FromServices] DAL<Artista> dal) =>
        {
            var listaDeArtistas = dal.Listar();
            if (listaDeArtistas is null)
            {
                return Results.NotFound();
            }
            var listaDeArtistaResponse = EntityListToResponseList(listaDeArtistas);
            return Results.Ok(listaDeArtistaResponse);
        });

        groupBuilder.MapGet("{nome}", ([FromServices] DAL<Artista> dal, string nome) =>
        {
            var artista = dal.RecuperarPor(a => a.Nome.ToUpper().Equals(nome.ToUpper()));
            if (artista is null)
            {
                return Results.NotFound();
            }
            return Results.Ok(EntityToResponse(artista));

        });

        groupBuilder.MapPost("", async ([FromServices]IHostEnvironment env,[FromServices] DAL<Artista> dal, [FromBody] ArtistaRequest artistaRequest) =>
        {
            
            var nome = artistaRequest.nome.Trim();
            var imagemArtista = DateTime.Now.ToString("ddMMyyyyhhss") + "." + nome + ".jpg";

            var path = Path.Combine(env.ContentRootPath,
                "wwwroot", "FotosPerfil", imagemArtista);

            using MemoryStream ms = new MemoryStream(Convert.FromBase64String(artistaRequest.fotoPerfil!));
            using FileStream fs = new(path, FileMode.Create);
            await ms.CopyToAsync(fs);

            var artista = new Artista(artistaRequest.nome, artistaRequest.bio) { FotoPerfil = $"/FotosPerfil/{imagemArtista}" };

            dal.Adicionar(artista);
            return Results.Ok();
        });

        groupBuilder.MapDelete("{id}", ([FromServices] DAL<Artista> dal, int id) => {
            var artista = dal.RecuperarPor(a => a.Id == id);
            if (artista is null)
            {
                return Results.NotFound();
            }
            dal.Deletar(artista);
            return Results.NoContent();

        });

        groupBuilder.MapPut("", ([FromServices] DAL<Artista> dal, [FromBody] ArtistaRequestEdit artistaRequestEdit) => {
            var artistaAAtualizar = dal.RecuperarPor(a => a.Id == artistaRequestEdit.Id);
            if (artistaAAtualizar is null)
            {
                return Results.NotFound();
            }
            artistaAAtualizar.Nome = artistaRequestEdit.nome;
            artistaAAtualizar.Bio = artistaRequestEdit.bio;        
            dal.Atualizar(artistaAAtualizar);
            return Results.Ok();
        });


        //ENDPOINTS AVALIAÇÃO
        groupBuilder.MapGet("{id}/avaliacao",(
            int id,
            HttpContext context,
            [FromServices] DAL<PessoaComAcesso> dalPessoa,
            [FromServices] DAL<Artista> dalArtista
            ) =>
        {
            //IDENTIFICAR O ARTISTA CONSULTADO
            var artista = dalArtista.RecuperarPor(a => a.Id == id);
            if(artista is null) return Results.NotFound($"Artisa de id {id} não encontrado!");

            //IDENTIFICAR O USUÁRIO CONECTADO
            var email = context.User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value
                ?? throw new InvalidOperationException("Usuário não conectado!");

            var pessoa = dalPessoa
                .RecuperarPor(p => p.Email.Equals(email))
                ?? throw new InvalidOperationException("Usuário não conectado!");

            //IDENTIFICAR A AVALIACAO (CASO EXISTA)
            var avaliacao = artista.Avaliacoes
                .FirstOrDefault(av => av.ArtistaId == artista.Id
                                    && av.PessoaId == pessoa.Id);

            //EXIBIR
            if (avaliacao is null)
            {
                //SE NÃO EXISTIR NENHUMA AVALIAÇÃO DESSA PESSOA PARA ESTE ARTISTA
                //RETORNO QUE A NOTA É ZERO
                return Results.Ok(new AvaliacaoArtistaResponse(artista.Id,
                                                               artista.Nome,
                                                               0));
                //return Results.NotFound("Não existe avaliação para este artista!");
            }
            else//CASO JÁ EXISTA ESSA AVALIAÇÃO
                //RETORNO EM FORMATO DE AvaliacaoArtistaResponse
            {
                return Results.Ok(new AvaliacaoArtistaResponse(
                                            avaliacao.ArtistaId, 
                                            artista.Nome,
                                            avaliacao.Nota));
            }

        });

        
        //ENDPOINT PARA INSERÇÃO/ATUALIZAÇÃO DE UMA AVALIAÇÃO
        groupBuilder.MapPost("avaliacao", (
            HttpContext context,
            [FromBody] AvaliacaoArtistaRequest request,
            [FromServices] DAL<Artista> dalArtista,
            [FromServices] DAL<PessoaComAcesso> dalPessoa) =>
        {
            //IDENTIFICAÇÃO DO ARTISTA A AVALIAR
            var artista = dalArtista.RecuperarPor(a => a.Id == request.artistaId);
            if (artista is null) return Results.NotFound();

            //IDENTIFICAÇÃO DO USUÁRIO QUE ESTÁ AVALIANDO
            var email = context.User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value
                ?? throw new InvalidOperationException("Usuário não conectado!");
            //context.User.Claims - USA INFORMAÇÕES DO COOKIE DE AUTENTICAÇÃO

            var pessoa = dalPessoa
                .RecuperarPor(p => p.Email.Equals(email))
                ?? throw new InvalidOperationException("Usuário não conectado!");

            //IDENTIFICAÇÃO DA AVALIAÇÃO (CASO EXISTA)
            var avaliacao = artista.Avaliacoes
                .FirstOrDefault(av => av.ArtistaId == artista.Id 
                                    && av.PessoaId == pessoa.Id);

            //SE ESSA AVALIAÇÃO AINDA NÃO EXISTIR, OU SEJA
            //É A PRIMEIRA VEZ QUE ESSE USUÁRIO AVALIA ESSA BANDA/ARTISTA
            if(avaliacao is null)
            {
                //ADICIONO A AVALIAÇÃO NA TABELA 
                artista.AdicionarNota(pessoa.Id, request.nota);
            }
            else//CASO JÁ EXISTA ESSA AVALIAÇÃO
            {
                //ATUALIZO SUA INFORMAÇÃO DE NOTA COM A NOVA NOTA PASSADA
                avaliacao.Nota = request.nota;
            }

            dalArtista.Atualizar(artista);
            return Results.Created();
        });


        #endregion
    }

    private static ICollection<ArtistaResponse> EntityListToResponseList(IEnumerable<Artista> listaDeArtistas)
    {
        return listaDeArtistas.Select(a => EntityToResponse(a)).ToList();
    }

    private static ArtistaResponse EntityToResponse(Artista artista)
    {
        return new ArtistaResponse(artista.Id, artista.Nome, 
                            artista.Bio, artista.FotoPerfil)
        {
            //MÉDIA DE NOTAS DESSE ARTISTA
            Classificacao = artista
                .Avaliacoes
                .Select(a => a.Nota)//DA LISTA DE AvaliacaoArtista SELECIONO AS NOTAS
                .DefaultIfEmpty(0) //SE FOR VAZIO, CONSIDERO TUDO ZERO
                .Average() //E CALCULO A MÉDIA
        };
    }

  
}
