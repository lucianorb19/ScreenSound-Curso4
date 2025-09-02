using ScreenSound.Shared.Modelos.Modelos;

namespace ScreenSound.Modelos; 

public class Artista 
{
    //PROPRIEDADES - CAMPOS
    public virtual ICollection<Musica> Musicas { get; set; } = new List<Musica>();
    public virtual ICollection<AvaliacaoArtista> Avaliacoes { get; set; } = new List<AvaliacaoArtista>();
    public string Nome { get; set; }
    public string FotoPerfil { get; set; } = string.Empty;
    public string Bio { get; set; }
    public int Id { get; set; }


    //CONSTRUTORES
    public Artista()
    {
        //FotoPerfil = "https://cdn.pixabay.com/photo/2016/08/08/09/17/avatar-1577909_1280.png";
    }
    public Artista(string nome, string bio)
    {
        Nome = nome;
        Bio = bio;
        //FotoPerfil = "https://cdn.pixabay.com/photo/2016/08/08/09/17/avatar-1577909_1280.png";
    }


    //DEMAIS MÉTODOS
    public void AdicionarMusica(Musica musica)
    {
        Musicas.Add(musica);
    }

    public void ExibirDiscografia()
    {
        Console.WriteLine($"Discografia do artista {Nome}");
        foreach (var musica in Musicas)
        {
            Console.WriteLine($"Música: {musica.Nome} - Ano de Lançamento: {musica.AnoLancamento}");
        }
    }

    public void AdicionarNota(int pessoaId, int nota)
    {
        //NOTA ENTRE INTERVALO 1 A 5
        nota = Math.Clamp(nota, 1, 5);
        //nota = Math.Min( Math.Max(nota, 1), 5);
        
        Avaliacoes.Add(new AvaliacaoArtista()
        {
            ArtistaId = this.Id,
            PessoaId = pessoaId,
            Nota = nota
        });
    }

    public override string ToString()
    {
        return $@"Id: {Id}
            Nome: {Nome}
            Foto de Perfil: {FotoPerfil}
            Bio: {Bio}";
    }
}