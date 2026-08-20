using SadConsole;
using SadRogue.Primitives;

namespace SurvivorGame.Mapa
{
    /// <summary>
    /// Andar da ProWay onde o personagem começa - interior do prédio antes de
    /// pegar o elevador. Baseado no desenho do Lindomar em REXPaint
    /// (Artes/Cenarios/mapa_inicio_teste.xp, mostrado como tela de entrada pelo
    /// ExploracaoScreen antes de liberar o movimento).
    ///
    /// O layout de colisão abaixo é uma APROXIMAÇÃO da topologia desenhada por ele
    /// (5 salas + corredor central com o elevador no meio) feita à mão como grade
    /// de texto - eu não consegui extrair as paredes exatas do .xp porque o
    /// desenho usa só blocos de cor de fundo (sem glyphs de texto), então não dá
    /// pra distinguir "parede" de "decoração" só pela cor com segurança. Ajustem
    /// os caracteres abaixo pra bater melhor com o desenho original se precisar.
    ///
    /// Legenda: '#' parede, '.' chão, 'E' entrada (porta da rua),
    /// 'L' elevador (leva pro andar 0 - ver MapaDestino).
    /// </summary>
    internal class MapaEscritorioProway : IMapa
    {
        private static readonly string[] Layout =
        {
            "##################################################",
            "##..................##......###.................##",
            "##..................##......###.................##",
            "##..........................###.................##",
            "##..................##..........................##",
            "##..................##......###.................##",
            "##..................##......###.................##",
            "######################......###.................##",
            "##...............#####......###.................##",
            "##...............#####......######################",
            "##..........................######################",
            "##...............#####......######################",
            "##...............#####..L...######################",
            "##...............#####......######################",
            "######################......######################",
            "##..................##......###.................##",
            "##..................##......###.................##",
            "##..................##......###.................##",
            "##..............................................##",
            "##..................##......###.................##",
            "##..................##......###.................##",
            "##..................##......###.................##",
            "##..................##......###.................##",
            "##..................##......###.................##",
            "######################..E...######################",
            "##################################################",
        };

        public int Largura { get; }
        public int Altura { get; }
        public Point PontoEntrada { get; private set; }

        private readonly Tile[,] _tiles;
        private readonly IMapa _andarZero;

        public MapaEscritorioProway()
        {
            _andarZero = new MapaAndarZero();

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

                    TileType tipo = c switch
                    {
                        '#' => TileType.Parede,
                        'L' => TileType.Elevador,
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

        /// <summary>Pisou no elevador ('L')? Manda pro andar 0.</summary>
        public IMapa? MapaDestino(int x, int y)
            => ObterTile(x, y).Tipo == TileType.Elevador ? _andarZero : null;

        public string? CaminhoArte => "Artes/Cenarios/mapa_inicio_teste.xp";
    }
}
