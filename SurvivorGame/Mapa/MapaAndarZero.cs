using SadConsole;
using SadRogue.Primitives;

namespace SurvivorGame.Mapa
{
    /// <summary>
    /// Andar 0 do prédio da ProWay: cafeteria (mesas + balcão) e a saída pro
    /// prédio. Baseado no desenho do Lindomar (Artes/Cenarios/location2.xp).
    ///
    /// "Sair do prédio" aqui NÃO leva pra outro IMapa novo - a cidade já existe
    /// (MapaCidadeBlumenau) e é justamente a tela de onde o jogador clicou em
    /// "ProWay" pra entrar. Por isso esse mapa não implementa MapaDestino: o
    /// ExploracaoScreen reconhece o TileType.SaidaPredio direto e volta pra tela
    /// anterior (a cidade), em vez de criar um mapa novo. Ver ExploracaoScreen.Mover.
    ///
    /// Mesma ressalva do MapaEscritorioProway: o layout abaixo é uma aproximação
    /// desenhada à mão a partir do render do .xp, não uma extração exata dele.
    ///
    /// Legenda: '#' parede, '.' chão, 'E' entrada (saiu do elevador aqui),
    /// 'S' saída do prédio (volta pro mapa da cidade).
    /// </summary>
    internal class MapaAndarZero : IMapa
    {
        private static readonly string[] Layout =
        {
            "##################################################",
            "##################################################",
            "##......................E.......................##",
            "##..............................................##",
            "##..............................................##",
            "##..............................................##",
            "##..............................................##",
            "##..............................................##",
            "##..............................................##",
            "##..............................................##",
            "##..............................................##",
            "##..............................................##",
            "##..............................................##",
            "##..............................................##",
            "#######################....#######################",
            "#######################....#######################",
            "#######################....#######################",
            "#######################....#######################",
            "#######################....#######################",
            "#######################....#######################",
            "#######################....#######################",
            "#######################....#######################",
            "#######################....#######################",
            "#######################....#######################",
            "#######################.S..#######################",
            "##################################################",
        };

        public int Largura { get; }
        public int Altura { get; }
        public Point PontoEntrada { get; private set; }

        private readonly Tile[,] _tiles;

        public MapaAndarZero()
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

                    TileType tipo = c switch
                    {
                        '#' => TileType.Parede,
                        'S' => TileType.SaidaPredio,
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

        public string? CaminhoArte => "Artes/Cenarios/location2.xp";
    }
}
