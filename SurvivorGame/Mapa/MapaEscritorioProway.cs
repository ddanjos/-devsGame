using SadConsole;
using SadRogue.Primitives;
using SurvivorGame.Utilitarios;

namespace SurvivorGame.Mapa
{
    /// <summary>
    /// Andar da ProWay onde o personagem começa - interior do prédio antes de
    /// pegar o elevador. A arte É o mapa: carregamos o .xp que o Lindomar
    /// desenhou no REXPaint (Artes/Cenarios/mapa_inicio_teste.xp) e desenhamos
    /// ele célula por célula como o próprio terreno, em vez de redesenhar um
    /// mapa abstrato ao lado da arte dele. O jogador anda literalmente em cima
    /// do desenho original, 60x60, pixel a pixel.
    ///
    /// Sobre colisão: o arquivo .xp não marca "isso é parede" em lugar nenhum -
    /// ele é desenhado só com blocos de cor de fundo preenchendo a tela inteira
    /// (nenhuma célula fica "vazia"/fora da estrutura), então não existe um sinal
    /// confiável pra distinguir parede de piso só pela cor. Por isso o andar
    /// inteiro é caminhável (EhBloqueado só barra fora dos limites do mapa) - o
    /// que faz sentido pra um escritório de piso aberto. O elevador e a escada
    /// pro porão são pontos exatos de ativação, posicionados nas coordenadas
    /// reais de onde o Lindomar desenhou os indicadores (o marcador
    /// vermelho/azul no meio do corredor, no caso do elevador).
    /// </summary>
    internal class MapaEscritorioProway : IMapa
    {
        private const string CaminhoXp = "Artes/Cenarios/mapa_inicio_teste.xp";

        // Coordenadas reais dentro do .xp (60x60), lidas diretamente dos pixels
        // do arquivo - não são um palpite. O marcador azul (51,51,255) que o
        // Lindomar desenhou no meio do corredor central fica nas células
        // (30,18)-(30,27); usamos uma delas como o "botão" do elevador. A
        // entrada é o ponto onde o corredor escuro alcança a borda de baixo do
        // desenho (a "porta da rua").
        private static readonly Point PosicaoElevador = new(30, 22);
        private static readonly Point PosicaoEntrada = new(25, 58);

        // A escada pro porão não vem do desenho do Lindomar (ele não desenhou
        // um porão) - é só um ponto dentro de uma das salas, reaproveitando o
        // MapaMasmorra que já existia pronto no projeto sem uso nenhum.
        private static readonly Point PosicaoEscada = new(10, 10);

        private readonly ScreenSurface _arte;
        private readonly IMapa _andarZero;
        private readonly IMapa _porao;

        public int Largura { get; }
        public int Altura { get; }
        public Point PontoEntrada => PosicaoEntrada;

        public MapaEscritorioProway()
        {
            _arte = ArteUtils.CarregarArteCenario(CaminhoXp);
            Largura = _arte.Width;
            Altura = _arte.Height;

            _andarZero = new MapaAndarZero();
            _porao = new MapaMasmorra();
        }

        public Tile ObterTile(int x, int y)
        {
            if (new Point(x, y) == PosicaoElevador) return TileFactory.Criar(TileType.Elevador);
            if (new Point(x, y) == PosicaoEscada) return TileFactory.Criar(TileType.Escada);
            return TileFactory.Criar(TileType.Chao);
        }

        public bool EhBloqueado(int x, int y)
            => x < 0 || x >= Largura || y < 0 || y >= Altura;

        /// <summary>Copia a arte do Lindomar direto pra tela, célula por célula -
        /// é o mapa em si, não uma ilustração ao lado dele.</summary>
        public void DesenharEm(ScreenSurface superficie)
            => _arte.Surface.Copy(superficie.Surface, 0, 0);

        /// <summary>Pisou no elevador? Manda pro andar 0. Pisou na escada? Manda
        /// pro porão (MapaMasmorra). ESC a qualquer momento volta direto pro
        /// mapa da cidade (ver ExploracaoScreen).</summary>
        public IMapa? MapaDestino(int x, int y)
        {
            if (new Point(x, y) == PosicaoElevador) return _andarZero;
            if (new Point(x, y) == PosicaoEscada) return _porao;
            return null;
        }

        /// <summary>Dica mostrada assim que o jogador entra aqui - o jogo precisa
        /// se ensinar sozinho, então em vez de deixar o jogador procurando o
        /// elevador sem pista nenhuma, apontamos o objetivo direto.</summary>
        public string? Dica =>
            "Você está no escritório da ProWay. Ande até o indicador azul no meio do corredor para chamar o elevador.";
    }
}
