using SadConsole;
using SadRogue.Primitives;

namespace SurvivorGame.Utilitarios
{

    public static class AjusteVisual
    {
        public static readonly Point TamanhoCelula = new Point(10, 16);

        public static void CorrigirProporcaoDeCelula(this IScreenSurface tela)
            => tela.FontSize = TamanhoCelula;
    }
}
