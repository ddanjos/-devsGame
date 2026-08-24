using System.Collections.Generic;

namespace SurvivorGame.Mapa
{
    /// <summary>
    /// Um "local" no sentido do SCRUM-9 (Sistema de Mapa): não é mais um terreno
    /// andável (isso é o IMapa antigo) - é uma tela ponto-e-clique: arte + nome +
    /// descrição de onde o personagem está, mais uma lista de ações que ele pode
    /// escolher (ver AcaoLocal). Renomeado de propósito pra não confundir com IMapa
    /// - são dois paradigmas de exploração diferentes coexistindo no projeto
    /// enquanto a migração não termina (ver LocalExploravelScreen).
    /// </summary>
    internal interface ILocalExploravel
    {
        string Nome { get; }
        string Descricao { get; }

        /// <summary>Arte .xp mostrada como ilustração do local (não como mapa
        /// andável - aqui é só imagem, igual ao inimigo no CombateScreen). null =
        /// sem arte, só texto.</summary>
        string? CaminhoArte => null;

        IReadOnlyList<AcaoLocal> Acoes { get; }
    }
}
