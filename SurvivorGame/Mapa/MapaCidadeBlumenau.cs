﻿using System;
using System.Collections.Generic;
using SadConsole;
using SadRogue.Primitives;

namespace SurvivorGame.Mapa
{
    /// <summary>
    /// Mapa do centro de Blumenau/SC: o rio Itajai-Acu cortando a cidade de oeste
    /// a leste, com o centro historico na margem sul (Rua XV de Novembro, ProWay e
    /// o cluster de museus) e a area residencial mais o Parque Ramiro Ruediger na
    /// margem norte.
    ///
    /// O desenho e montado em camadas, na ordem em que uma cidade se forma: malha
    /// viaria com quarteiroes de tamanhos IRREGULARES, o rio cortando essa malha,
    /// avenidas acompanhando as duas margens, pontes, parques e as pracas dos
    /// pontos turisticos. A versao anterior repetia "RBBRRBBRRBBR" em toda linha,
    /// o que deixava tudo com cara de tabuleiro de xadrez.
    ///
    /// Duas propriedades foram verificadas por busca em largura (BFS) antes de
    /// fixar este layout, e valem como regra pra qualquer alteracao futura:
    ///   1. todos os 11 pontos turisticos sao alcancaveis a pe saindo do ProWay -
    ///      desde que o mapa virou andavel, um local ilhado seria um bug de
    ///      verdade, nao so um detalhe estetico;
    ///   2. as unicas colunas sem agua sao as das pontes (16-17, 40-41 e 64-65),
    ///      entao o rio divide a cidade de verdade e nao da pra contorna-lo a pe
    ///      pela mata - senao as pontes seriam puramente decorativas.
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
    ///   '*' = ponto turistico (ver lista Locais)
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
            "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF",
            "==========================================================================================",
            "==========================================================================================",
            "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF==FFFFFFFFFF",
            "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF==FFFFFFFFFF",
            "FFFFFFFFRRRRRRRRRRRFFFFFFFRRRRRRRRRRRRRRRRRRRRRRRRRRRRRFFFFFFFRRRRRRRRRRRRRRRR==RRRRRFFFFF",
            "FFFFFFFFBRBBBBBBBRBBFFFFFRRRBBBRBBBBBBBRBBBBBBBRBBBBRBBBFFFFFBBBBRBBBBBBRBBBBBBBRBBFFFFFFF",
            "FFFFFFFFFRBBBBBBBRBBBBBBRRRRBBBRBBBBBBBRBBBBBBBRBBBBRBBBBRBBBBBBBRRRRRBBRBBBBBBBRBBFFFFFFF",
            "FFFFFFFFBRBBBBBBBRBBBBBBRRRRBBBRBBBBBBBRBBBBBBBRBBBBRBBBBRBBBBBBBRRRRRBBRBBBBBBBRBFFFFFFFF",
            "FFRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRFF",
            "FFFFFFRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRFFFFFFF",
            "FFRBBBPPPPPPPPPPPPBBBBBBRBBBBBBRBBBBBBBRBBBBBBBRBBBBRBBBBRBBBBBBBRBBBBBBRBBBBBBBRBBBBFFFFF",
            "FFRBBBPPPPPPPPPPPPBBBBBBRBBBBBBRBBBBBBBRBBBBBBBRBRRRRRBBBRBBBBBBBRBBBBBBRBBBBBBBRBBBBRBRFF",
            "FFRBBBPPPPPPPPPPPPBBBBBBRBBBBBBRBBBBBBBRBBBBBBBRBRRRRRBBBRBBBBBBBRBBBBBBRBBBBBBBRBBBBRBRFF",
            "FFFBBBPPPP*PPPPPPPBBBBBBRBBBBBBRBBBBBBBRBBBBBBBRBRRRRRBBBRBBBBBBBRBBBBBBRBBBBBBBRBBBBRBRFF",
            "FFFFFRPPPPPPPPPPPPRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRFF",
            "FFFFFFPPPPPPPPPPPPBBBBBBRBBBBBBRBBBBBBBRBBBBBBBRBBBBRBBBBRBBBBBBBRBBBBBBRBBBBBBBRBBBBFFFFF",
            "FFFFFFPPPPPPPPPPPPBBBBBBRBBBBBBRBBBBBBBRBBBBBBBRBBBBRBBBBRBBBBBBBRBBBBBBRBBBBBBBRBBFFFFFFF",
            "FFFFFFPPPPPPPPPPPPBBBBBBRBBBBBBRBBBBBBBRBBBBBBBRBBBBRBBBBRBBBBBBBRBBBBBBRBBBBBBBRBFFFFFRFF",
            "FFFFFRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRR~~~",
            "FFFBBBBBBRBBBBBBRRBBBBBBRBBRRRRRRRRRRRRRRRRRRBBRBBBBRBBBBRBBBBBBRRBBBBBBRRRRRRRR~~~~~~~~~~",
            "~~RRRRRRRRRRRRRRRRRRRRRRRRR~~~~~~~~~~~~~OO~~~RRRRRRRRRRRRRRRRRRRRRRRRRRRR~~~~~~~~~~~~~~~~~",
            "~~~~~~~~~~~~~~~~OO~~~~~~~~~~~~~~~~~~~~~~OO~~~~~~~~~~~~~~~~~~~~~~OO~~~~~~~~~~~~~~~~~~~~~~~~",
            "~~~~~~~~~~~~~~~~OO~~~~~~~~~~~~~~~~~~~~~~OO~~~~~~~~~~~~~~~~~~~~~~OO~~~~~~~~~~~~~~~RRRRRRRFF",
            "~~~~~~~~~~~~~~~~OO~~~~~~~~~~~~~~~~~~~~~~OO~RRRRRRRRRR~~~~~~~~~~~OORRRRRRRRRRRRRRRBBBBRBRFF",
            "FFRRRRR~~~~~~~~~OO~~~~~~~~~RRRRRRRRRRRRRRRRBBBBRBBBBRRRRRRRRRRRRRRBBBBBBRBBBBBBBRBBBBRBRFF",
            "FFRBBBBRRRRRRRRRRRRRRRRRRRRBBBBRBRRRBBBRRRBBBBBRBBBBRBBBBRBBBBBBRRBBBBBBRBBBBBBBRBBBBRBRFF",
            "FFRFFFBBBRBBBBBBRRBBRRRRRBBBBBBRBR*RBBBRBBBBBBBRBBBBRBBBBRBBBBBBBRBBBBBBRBBBBBBBRBBBBRBRFF",
            "FFFFFFFFBRBBBBBBBRBBRRRRRBBBBBBRBRRRBBBRBBBBBBBRBBBBRBBBBRBBBBBBBRBBBBBBRBBBBBBBRBBBBRFFFF",
            "FFFFFFFFFRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRFFFFFF",
            "FFFFFFFFFFBBBBBBBRBBBBBBRBBBBBBRBBBBBBBRBBBBBBBRBBBBRBBBBRBBBBBBBRBBBRRRRBBBBBBBRBBBFFFFFF",
            "FFFFFFFFFRBBBBBBBRBBBBBBRBBBBBBRBBBBBBBRRRBBBBBRBBBRRRBBRRRBBBBBBRBBBRRRRBBBBBBBRBBFFFFFFF",
            "FFRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRR*RRRRRRRRRRR*RRRR*RRRRRRRRRRRRRRRRRRRRRRRRRRRRRRFF",
            "FFRFFFBBBRBBBBBBBRBBBBBBRBBBBBBRBBBBBBBRRRBBBBBRBBBRRRBBRRRRRBBBBRBBBBBBRBBBBBBBRBBBFFFFFF",
            "FFRBBBBBBRBBBBBBBRBBBBBBRBBBBBBRBBBBBBBRBBBBBRRRBBBBRBRRRRR*RBBBBRBBBBBBRBBBBBBBRBBBBRFFFF",
            "FFRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRERRRRRRRR*RRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRFF",
            "FFRBBBBBBRBBBBBBBRBBBBBBRBBPPPPPPPPPPPPPBBBBBRRRBBBBRBRRRRBBBR*RRRBBBBBBRBBBBBBBRBBBBRBRFF",
            "FFFFFFBBBRBBBBBBBRBBBBBBRBBPPPPPPPPPPPPPBBBBBBBRBBBBRBBBBRBBBRRRBRBBBBBBRBBBBBBBRBBBFFFRFF",
            "FFFFFFFFBRBRRRBBBRBBBBBBRBBPPPPPP*PPPPPPBBBBBBBRBBBBRBBBBRBBBBBBBRBBBBBBRBBBBBBBRBFFFFFFFF",
            "FFFFFFFFBRBR*RBBBRBBBBBBRBBPPPPPPPPPPPPPBBBBRRRRRBBBRBBBBRBBBBBBBRBBBBBBRBBBBBBBRFFFFFFFFF",
            "FFFFFFFFFRBRRRBBBRBBBBBBRBBPPPPPPPPPPPPPBBBBRRRRRBBBRBBBBRBBBBBBBRBBBBBBRBBBBBBBFFFFFFFFFF",
            "FFFFFFFFRRRRRRRRRRRRRRRRRRRPPPPPPPPPPPPPRRRRRRRRRRRRRRRRRRRRRRRRRRRRFFFFFRRRRRRRRFFFFFFFFF",
            "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF",
            "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF",
            "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF"
        };

        /// <summary>
        /// Lugares clicáveis no mapa (pontos turísticos + o ProWay, ponto de partida).
        /// Cada um vira uma "porta de entrada" pra um cenário quando clicado.
        /// </summary>
        public static readonly IReadOnlyList<LocalMapa> Locais = new List<LocalMapa>
        {
            new("ProWay", new Point(46, 35),
                "Centro de treinamento em tecnologia, comunicacao e negocios na Rua Sete de Setembro. E daqui que a jornada do sobrevivente comeca."),
            new("Prefeitura Municipal de Blumenau", new Point(34, 27),
                "Sede do governo municipal, em estilo enxaimel, as margens do rio Itajai-Acu."),
            new("Catedral Sao Paulo Apostolo", new Point(40, 32),
                "Templo com vitrais coloridos e uma torre de 45 metros com tres sinos eletronicos, inaugurado em 1958."),
            new("Museu da Cerveja de Blumenau", new Point(52, 32),
                "Conta a historia da cultura cervejeira da cidade, bem no comeco da Rua XV de Novembro."),
            new("Museu de Habitos e Costumes", new Point(57, 32),
                "Museu pequeno dedicado ao cotidiano dos primeiros colonizadores de Blumenau."),
            new("Museu da Familia Colonial", new Point(55, 35),
                "Reconstitui a vida domestica dos imigrantes alemaes que fundaram a cidade."),
            new("Castelinho da Havan", new Point(62, 36),
                "Replica da prefeitura de Michelstadt, na Alemanha, construida em 1978 e hoje uma loja de departamentos."),
            new("Mausoleu Dr. Blumenau", new Point(59, 34),
                "Guarda os restos mortais do fundador da cidade, Dr. Hermann Bruno Otto Blumenau."),
            new("Parque Sao Francisco de Assis", new Point(33, 38),
                "Area verde bem no meio da cidade, um respiro de mata entre o centro historico e o resto do centro."),
            new("Parque Ramiro Ruediger", new Point(10, 14),
                "O maior parque publico de Blumenau, com pista de corrida, quadras e area de lazer."),
            new("Museu Hering", new Point(12, 39),
                "Conta a historia da familia Hering e da industria textil que ajudou a moldar a cidade.")
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
