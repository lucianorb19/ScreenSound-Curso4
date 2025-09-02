using ScreenSound.Modelos;

namespace ScreenSound.Shared.Modelos.Modelos
{
    public class AvaliacaoArtista
    {
        //PROPRIEDADES - CAMPOS
        public int ArtistaId { get; set; }
        public virtual Artista? Artista { get; set; }

        //RELAÇÃO COM PESSOA SEM REFERÊNCIA DIRETO AO OBJETO
        //PARA NÃO CRIAR DEPENDÊNCIA ENTRE O PROJETO DE MODELOS
        //E O PROJETO DE DADOS (ONDE FICA A CLASSE PESSOA COM ACESSO)
        public int PessoaId { get;set; }
        
        public int Nota { get; set; }

    }
}
