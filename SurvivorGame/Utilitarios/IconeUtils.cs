using System.Collections.Generic;
using SadConsole;
using SurvivorGame.Inventario;

namespace SurvivorGame.Utilitarios
{
    /// <summary>
    /// Cache de ícones de item carregados do REXPaint - mesma ideia do
    /// TileFactory (Factory + Flyweight): cada ícone é carregado do disco uma
    /// única vez e reaproveitado toda vez que a UI precisa desenhar aquele tipo
    /// de item, em vez de reabrir o arquivo .xp a cada redesenho da tela.
    /// </summary>
    internal static class IconeUtils
    {
        private static readonly Dictionary<string, ScreenSurface?> _cache = new();

        /// <summary>Devolve o ícone do item, ou NULL se o .xp dele não puder ser
        /// carregado. Nullable de propósito: quando o ArteUtils passou a devolver
        /// null em vez de lançar exceção, este cache guardava o null e o entregava
        /// como se fosse um objeto - e a tela de inventário estourava
        /// NullReferenceException ao ler icone.Width, fechando o jogo na tecla 'I'.
        /// Agora o tipo diz a verdade e quem chama é obrigado a tratar.</summary>
        public static ScreenSurface? ObterIcone(ItemInventario item)
        {
            string caminho = item switch
            {
                Consumivel => "Artes/Icones/icon_apple.xp",
                Arma => "Artes/Icones/icon_weapon.xp",
                Armadura => "Artes/Icones/icon_armor.xp",
                _ => "Artes/Icones/icon_apple.xp"
            };

            if (!_cache.TryGetValue(caminho, out ScreenSurface? icone))
            {
                icone = ArteUtils.CarregarArteCenario(caminho);

                // Guarda até o null: assim um arquivo faltando é tentado uma vez
                // só, e não a cada redesenho da tela.
                _cache[caminho] = icone;
            }

            return icone;
        }
    }
}
