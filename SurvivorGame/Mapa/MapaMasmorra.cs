﻿using System;
using System.Collections.Generic;
using SadConsole;
using SadRogue.Primitives;

namespace SurvivorGame.Mapa
{
    /// <summary>
    /// Mapa do jogo, gerado a partir do desenho enviado pela equipe.
    /// Interpretação do desenho (legenda):
    ///   '#' = parede (contorno das "bolhas"/salas do desenho)
    ///   '.' = chao / area caminhavel (interior das salas e corredores)
    ///   'E' = ponto de entrada (onde estava a seta no desenho, lado oeste)
    ///
    /// Topologia reconhecida no desenho:
    ///   - 3 salas na parte de cima (esquerda grande, meio pequena/estreita, direita grande)
    ///   - um "hub" central que liga tudo
    ///   - 3 salas na parte de baixo (esquerda, meio com reentrancia, direita grande em L)
    ///   - corredor de entrada vindo da esquerda (a seta do desenho)
    ///
    /// Se o layout não bater 100% com o desenho original, é só ajustar os
    /// caracteres desta matriz — é a forma mais simples de "redesenhar" o mapa.
    /// </summary>
    internal class MapaMasmorra : IMapa
    {
        private static readonly string[] Layout =
        {
            "########################################################################",
            "########################################################################",
            "##..................##############........................##############",
            "##..................##############........................##############",
            "##..................####......####........................##############",
            "##..................####......####........................##############",
            "##..................####......####........................##############",
            "##..................####......####........................##############",
            "##........................................................##############",
            "##........................................................##############",
            "###################..###......####........................##############",
            "###################..#####..######........................##############",
            "###################..#####..######........................##############",
            "###################..#####..#########..#################################",
            "###################..#####..#########..#################################",
            "E....................#####..#########..#################################",
            ".....................###...............#################################",
            "########################...............#....................############",
            "########################..............##....................############",
            "########################....................................############",
            "########################....................................############",
            "########################..............##....................############",
            "########################..............##....................############",
            "########################..............##....................############",
            "######################..################....................############",
            "##########...............###############....................############",
            "##.......................###############....................############",
            "##.............###.........#############....................############",
            "##.............###.........#############....................############",
            "##.............###..........................................############",
            "##.............###..........................................############",
            "##.............###.........#############....................############",
            "##.............###.........#############....................############",
            "########################################################################"
        };

        public int Largura { get; }
        public int Altura { get; }
        public Point PontoEntrada { get; private set; }

        private readonly Tile[,] _tiles;

        public MapaMasmorra()
        {
            Altura = Layout.Length;
            Largura = Layout[0].Length;
            _tiles = new Tile[Largura, Altura];
            Construir();
        }

        private void Construir()
        {
            for (int y = 0; y < Altura; y++)
            {
                string linha = Layout[y];
                for (int x = 0; x < Largura; x++)
                {
                    char c = linha[x];

                    if (c == 'E')
                    {
                        PontoEntrada = new Point(x, y);
                        _tiles[x, y] = TileFactory.Criar(TileType.Chao);
                        continue;
                    }

                    TileType tipo = c == '#' ? TileType.Parede : TileType.Chao;
                    _tiles[x, y] = TileFactory.Criar(tipo);
                }
            }
        }

        public Tile ObterTile(int x, int y) => _tiles[x, y];

        public bool EhBloqueado(int x, int y)
        {
            if (x < 0 || x >= Largura || y < 0 || y >= Altura)
                return true;

            return _tiles[x, y].Bloqueado;
        }

        /// <summary>
        /// Desenha o mapa inteiro em uma ScreenSurface do SadConsole.
        /// </summary>
        public void DesenharEm(ScreenSurface superficie)
        {
            for (int y = 0; y < Altura; y++)
            {
                for (int x = 0; x < Largura; x++)
                {
                    Tile tile = _tiles[x, y];
                    superficie.Surface.SetGlyph(x, y, tile.Glyph, tile.CorFrente, tile.CorFundo);
                }
            }
        }
    }
}
