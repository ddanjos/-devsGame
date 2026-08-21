using System;
using System.Collections.Generic;
using SadRogue.Primitives;

namespace SurvivorGame.Mapa
{
    /// <summary>
    /// Fábrica de Tiles.
    /// Factory: centraliza a criação de cada TileType (glyph, cores, se bloqueia).
    /// Flyweight: como um Tile não guarda posição, a fábrica cria cada tipo UMA
    /// única vez e devolve sempre a mesma instância - o mapa inteiro (por maior
    /// que seja) usa só um punhado de objetos Tile na memória.
    /// </summary>
    internal static class TileFactory
    {
        private static readonly Dictionary<TileType, Tile> _cache = new();

        public static Tile Criar(TileType tipo)
        {
            if (_cache.TryGetValue(tipo, out Tile? tileExistente))
                return tileExistente;

            Tile novoTile = tipo switch
            {
                TileType.Parede => new Tile(TileType.Parede, glyph: 178, Color.Gray, Color.Black, bloqueado: true),
                TileType.Chao => new Tile(TileType.Chao, glyph: '.', Color.DarkGray, Color.Black, bloqueado: false),

                TileType.Agua => new Tile(TileType.Agua, glyph: '~', Color.Cyan, new Color(30, 60, 90), bloqueado: true),
                TileType.Ponte => new Tile(TileType.Ponte, glyph: '=', Color.White, new Color(120, 100, 60), bloqueado: false),
                TileType.Floresta => new Tile(TileType.Floresta, glyph: 6, Color.DarkGreen, new Color(20, 55, 20), bloqueado: false),
                TileType.Parque => new Tile(TileType.Parque, glyph: 176, Color.LightGreen, new Color(50, 100, 50), bloqueado: false),
                TileType.Rua => new Tile(TileType.Rua, glyph: '.', Color.LightGray, new Color(90, 90, 90), bloqueado: false),
                TileType.Predio => new Tile(TileType.Predio, glyph: 219, new Color(90, 80, 70), new Color(60, 55, 50), bloqueado: true),
                TileType.Rodovia => new Tile(TileType.Rodovia, glyph: '=', Color.Yellow, new Color(110, 95, 55), bloqueado: false),
                TileType.Inicio => new Tile(TileType.Inicio, glyph: '@', Color.White, new Color(70, 130, 70), bloqueado: false),
                TileType.PontoTuristico => new Tile(TileType.PontoTuristico, glyph: '*', Color.Gold, new Color(90, 90, 90), bloqueado: false),

                TileType.Elevador => new Tile(TileType.Elevador, glyph: '=', Color.Cyan, new Color(20, 20, 60), bloqueado: false),
                TileType.SaidaPredio => new Tile(TileType.SaidaPredio, glyph: '>', Color.Yellow, new Color(40, 30, 20), bloqueado: false),
                TileType.Escada => new Tile(TileType.Escada, glyph: 'v', new Color(200, 160, 60), new Color(40, 25, 10), bloqueado: false),

                _ => throw new ArgumentOutOfRangeException(nameof(tipo), tipo, "Tipo de tile desconhecido")
            };

            _cache[tipo] = novoTile;
            return novoTile;
        }
    }
}
