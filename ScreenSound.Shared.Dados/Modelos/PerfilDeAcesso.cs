using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenSound.Shared.Dados.Modelos
{
    public class PerfilDeAcesso : IdentityRole<int>
    {
    }
}

/*
 OBSERVAÇÕES
: IdentityRole<int> - O int INDICA QUE A CHAVE PRIMÁRIA DA TABELA QUE VAI REPRESENTAR
ESSA CLASSE NO BD, VAI SER UM INTEIRO
 */