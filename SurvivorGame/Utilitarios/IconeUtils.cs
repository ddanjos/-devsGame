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
        private static readonly Dictionary<string, ScreenSurface> _cache = new();

        public static ScreenSurface ObterIcone(ItemInventario item)
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
                _cache[caminho] = icone;
            }

            return icone;
        }
    }
}
