using SadRogue.Primitives;

namespace SurvivorGame.Mapa
{
    /// <summary>
    /// Representa as características de UM tipo de terreno (não uma célula específica do mapa).
    /// Guarda só o estado "intrínseco" (compartilhável): glyph, cores, se bloqueia passagem.
    /// A posição (x, y) é estado "extrínseco" e fica na matriz do mapa, não aqui -
    /// isso é o que permite o TileFactory reutilizar a mesma instância (Flyweight).
    /// </summary>
    internal class Tile
    {
        public TileType Tipo { get; }
        public int Glyph { get; }
        public Color CorFrente { get; }
        public Color CorFundo { get; }
        public bool Bloqueado { get; }

        public Tile(TileType tipo, int glyph, Color corFrente, Color corFundo, bool bloqueado)
        {
            Tipo = tipo;
            Glyph = glyph;
            CorFrente = corFrente;
            CorFundo = corFundo;
            Bloqueado = bloqueado;
        }
    }
}
