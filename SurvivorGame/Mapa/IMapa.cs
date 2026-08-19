using SadConsole;
using SadRogue.Primitives;

namespace SurvivorGame.Mapa
{
    /// <summary>
    /// Contrato comum a qualquer mapa de TERRENO do jogo (a masmorra do desenho, a
    /// cidade de Blumenau, ou qualquer mapa futuro). Isso é diferente do MapaJogo
    /// de vocês, que guarda os itens largados no chão - IMapa é só o terreno
    /// (paredes, chão, água, ruas...). Program.cs usa os dois juntos.
    ///
    /// Strategy pattern: quem consome um IMapa não precisa saber qual implementação
    /// está ativa, só que ele sabe se desenhar e informar se uma posição é bloqueada.
    /// </summary>
    internal interface IMapa
    {
        int Largura { get; }
        int Altura { get; }
        Point PontoEntrada { get; }

        bool EhBloqueado(int x, int y);
        void DesenharEm(ScreenSurface superficie);
    }
}
