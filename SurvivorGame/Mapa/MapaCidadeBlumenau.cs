﻿using System;
using System.Collections.Generic;
using SadConsole;
using SadRogue.Primitives;

namespace SurvivorGame.Mapa
{
    /// <summary>
    /// Mapa do centro de Blumenau/SC, gerado a partir de uma captura do Google Maps
    /// (vista satelite do centro historico, em torno da Prainha e do rio Itajai-Acu)
    /// e enriquecido com pontos turisticos reais pesquisados na internet.
    ///
    /// Legenda:
    ///   'F' = Floresta (mata / encosta)
    ///   'P' = Parque (area verde urbana)
    ///   'R' = Rua (area urbana caminhavel)
    ///   'B' = Predio (quarteirao/edificacao - bloqueia passagem)
    ///   '~' = Agua (rio Itajai-Acu - bloqueia passagem, exceto nas pontes)
    ///   'O' = Ponte (travessia sobre o rio)
    ///   '=' = Rodovia (via principal, ex: Rod. Jorge Lacerda)
    ///   'E' = ponto de partida do personagem (ProWay, R. Sete de Setembro 1600 - Centro)
    ///   '*' = ponto turistico (ver dicionario PontosTuristicos)
    ///
    /// Pontos turisticos pesquisados (fonte: guias de turismo de Blumenau) e
    /// posicionados de acordo com a imagem de referencia do centro da cidade:
    ///   Prefeitura Municipal, Catedral Sao Paulo Apostolo, Museu da Cerveja,
    ///   Museu de Habitos e Costumes, Museu da Familia Colonial, Castelinho da
    ///   Havan, Mausoleu Dr. Blumenau, Parque Sao Francisco de Assis, Parque
    ///   Ramiro Ruediger e Museu Hering. Os cinco primeiros (Museu da Cerveja,
    ///   Museu de Habitos e Costumes, Museu da Familia Colonial, Havan e
    ///   Mausoleu) realmente ficam a poucos metros um do outro na vida real,
    ///   entao o cluster deles no mapa nao e coincidencia.
    /// </summary>
    internal class MapaCidadeBlumenau : IMapa
    {
        private static readonly string[] Layout =
        {
            "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFRRRRRRRRRRRRR~~~~RRRRRRRRRRRRRRRFFF",
            "FFRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRFFFFFFFFRBBRRBBRRBBR~~~~~BBRRBBRRBBR=======",
            "FFRBBRRBBRRBBRRBBRRBBRRBBRRBBRRBBRRBBRRBBRRBBRRFFFFFFFFRBBRRBBRRBBR~~~~RBBRRBBR===========",
            "FFRBBRRBBRRBBRRBBRRBBRRBBRRBBRRBBRRBBRRBBRRBBRRFFFFFFFFRRRRRRRRRR~~~~~~RRRRR==============",
            "FFRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRFFFFFFFFRRRRRRRRRR~~~~~RRRRR===========RFFF",
            "FFRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRFFFFFFFFRBBRRBBR~~~~~~BRR=========BRRBBRFFF",
            "FFRBBRRB*RRBBRRBBRRBBRRBBRRBBRRBBRRBBRRBBRRBBRRFFFFFFFFRBBRRBBR~~~~~BBR=======RRBBRRBBRFFF",
            "FFRBBRRBBRRBBRRBBRRBBRRBBRRBBRRBBRRBBRR*BRRBBRRFFFFFFFFRRRRRR~~~~~~~R========RRRRRRRRRRFFF",
            "FFRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRFFFFFFFFRRRRRR~~~~~RRR=====RRRRRRRRRRRRRFFF",
            "FFRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRFFFFFFFFFFFFF~~~~~~F======FFFFFFFFFFFFFFFFF",
            "FFRBBRRBBRRBBRRBBRRBBRRBBRRBBRRBBRRBBRRBBRRBBRRFFFFFFFFFFFF~~~~~FF======FFFFFFFFFFFFFFFFFF",
            "FFRBBRRBBRRBBRRBBRRBBRRBBRRBBRRBBRRBBRRBBRRBBRRFFFFFFFFFFFF~~~~~F======FFFFFFFFFFFFFFFFFFF",
            "FFRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRFFFFFFFFFFFF~~~FF======FFFFFFFFFFFFFFFFFFFF",
            "FFRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRFFFFFFFFFFF~~~~F=====FFFFFFFFFFFFFFFFFFFFFF",
            "FFRBBRRBBRRBBRRBBRRBBRRBBRRBBRRBBRRBBRRBBRRBBRRPFFFFFFFFF~~~~~======FFFFFFFFFFFFFFFFFFFFFF",
            "FFRBBRRBBRRBBRRBBRRBBRRBBRRBBRRBBRRBBRRBBRRBBRRPFFFFFFFFF~~~~======FFFFFFFFFFFFFFFFFFFFFFF",
            "FFRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRPFFFFFFFF~~~~F====FFFFFFFFFFFFFFFFFFFFFFFFF",
            "FFRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRPFFFFFFF~~~~~=====FFFFFFFFFFFFFFFFFFFFFFFFF",
            "FFRBBRRBBRRBBRRBBRRBBRRBBRRBBRRBBRRBBRRBBRRBRRR~FFF~~~~~~~~=====FFFFFFFFFFFFFFFFFFFFFFFFFF",
            "FFRBBRRBBRRBBRRBBRRBBRRBBRRBBRRBBRRBBRRBBRRBR*R~~~~~~~~~~~=====FFFFFFFFFFFFFFFFFFFFFFFFFFF",
            "FFRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRR~~~~~~~~~OF====FFFFFFFFFFFFFFFFFFFFFFFFFFFF",
            "FFFFFFFFFFFFFFPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP~~~~~~~~~~~O=====FFFFFFFFFFFFFFFFFFFFFFFFFFFF",
            "FFFFFFFFFFFFFFPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP~~~~~~~~~O=*===F*FFFFFFFFFFFFFFFFFFFFFFFFFF",
            "FFFFFFFFFFFFFFPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPF~~~~~~~O===*===FFFFFFFFFFFFFFFFFFFFFFFFFF",
            "FFFFFFFFFFFFFFPPPPPPPPPPPPPPPPPPPP*PPPPPPPPPPPPPFFFFFFF~OO=*=====FFFFFFFFFFFFFFFFFFFFFFFFF",
            "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFOO=======FFFFFFFFFFFFFFFFFFFFFFFF",
            "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF~==*==FFFFFFFFFFFFFFFFFFFFFFFF",
            "RRRRRRRRRRRRRRRRRRRRFFFFFFFFFFFFFFFFFFFFFFFFFRRRRRRRRRRRRRRRR~R====RRFFFFFFFFFFFFFFFFFFFFF",
            "RBBRRBBRRBBRRBBRRBBRFFFFFFFFFFFFFFFFFFFFFFFFFRBERRBBRRBBRRBBRR~====BRFFFFFFFFFFFFFFFFFFFFF",
            "RBBRRBBRRBBRRBBRRBBRFFFFFFFFFFFFFFFFFFFFFFFFFRBBRRBBRRBBRRBBRR~====BRFFFFFFFFFFFFFFFFFFFFF",
            "RRRRRRRRRRRRRRRRRRRRFFFFFFFFFFFFFFFFFFFFFFFFFRRRRRRRRRRRRRRRRR~====RRFFFFFFFFFFFFFFFFFFFFF",
            "RRRRRRRRRRRRRRRRRRRRFFFFFFFFFFFFFFFFFFFFFFFFFRRRRRRRRRRRRRRRRR~====RRFFFFFFFFFFFFFFFFFFFFF",
            "RBBRRBBRRBB*RBBRRBBRFFFFFFFFFFFFFFFFFFFFFFFFFRBBRRBBRRBBRRBBRR~===BBRFFFFFFFFFFFFFFFFFFFFF",
            "RBBRRBBRRBBRRBBRRBBRFFFFFFFFFFFFFFFFFFFFFFFFFRBBRRBBRRBBRRBBRR====BBRFFFFFFFFFFFFFFFFFFFFF",
            "RRRRRRRRRRRRRRRRRRRRFFFFFFFFFFFFFFFFFFFFFFFFFRRRRRRRRRRRRRRRRR====RRRFFFFFFFFFFFFFFFFFFFFF",
            "RRRRRRRRRRRRRRRRRRRRFFFFFFFFFFFFFFFFFFFFFFFFFRRRRRRRRRRRRRRRRR===RRRRFFFFFFFFFFFFFFFFFFFFF",
            "RBBRRBBRRBBRRBBRRBBRFFFFFFFFFFFFFFFFFFFFFFFFFRBBRRBBRRBBRRBBRR===RBBRFFFFFFFFFFFFFFFFFFFFF",
            "RBBRRBBRRBBRRBBRRBBRFFFFFFFFFFFFFFFFFFFFFFFFFRBBRRBBRRBBRRBBRR===RBBRFFFFFFFFFFFFFFFFFFFFF",
            "RRRRRRRRRRRRRRRRRRRRFFFFFFFFFFFFFFFFFFFFFFFFFRRRRRRRRRRRRRRRRR===RRRRFFFFFFFFFFFFFFFFFFFFF",
            "RRRRRRRRRRRRRRRRRRRRFFFFFFFFFFFFFFFFFFFFFFFFFRRRRRRRRRRRRRRRR====RRRRFFFFFFFFFFFFFFFFFFFFF",
            "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFRRRRRRRRRRRRRRRR====RRRRFFFFFFFFFFFFFFFFFFFFF",
            "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFRRRRRRRRRRRRRRRR===~RRRRFFFFFFFFFFFFFFFFFFFFF",
            "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF===FFFFFFFFFFFFFFFFFFFFFFFFFF",
            "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF===FFFFFFFFFFFFFFFFFFFFFFFFFF",
            "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF===FFFFFFFFFFFFFFFFFFFFFFFFFF"
        };

        /// <summary>Nome do ponto turistico -> posicao no grid.</summary>
        public static readonly IReadOnlyDictionary<string, Point> PontosTuristicos = new Dictionary<string, Point>
        {
            { "Prefeitura Municipal de Blumenau", new Point(39, 7) },
            { "Catedral Sao Paulo Apostolo", new Point(45, 19) },
            { "Museu da Cerveja de Blumenau", new Point(58, 22) },
            { "Museu de Habitos e Costumes", new Point(63, 22) },
            { "Museu da Familia Colonial", new Point(59, 24) },
            { "Castelinho da Havan", new Point(63, 26) },
            { "Mausoleu Dr. Blumenau", new Point(60, 23) },
            { "Parque Sao Francisco de Assis", new Point(34, 24) },
            { "Parque Ramiro Ruediger", new Point(8, 6) },
            { "Museu Hering", new Point(11, 32) }
        };

        public int Largura { get; }
        public int Altura { get; }
        public Point PontoEntrada { get; private set; }

        private readonly Tile[,] _tiles;

        public MapaCidadeBlumenau()
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
                        _tiles[x, y] = TileFactory.Criar(TileType.Inicio);
                        continue;
                    }

                    TileType tipo = c switch
                    {
                        'F' => TileType.Floresta,
                        'P' => TileType.Parque,
                        'R' => TileType.Rua,
                        'B' => TileType.Predio,
                        '~' => TileType.Agua,
                        'O' => TileType.Ponte,
                        '=' => TileType.Rodovia,
                        '*' => TileType.PontoTuristico,
                        _ => TileType.Chao
                    };

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
