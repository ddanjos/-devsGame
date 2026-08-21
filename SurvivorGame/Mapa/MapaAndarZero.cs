using SadConsole;
using SadRogue.Primitives;
using SurvivorGame.Utilitarios;

namespace SurvivorGame.Mapa
{
    /// <summary>
    /// Andar 0 do prédio da ProWay: cafeteria (mesas + balcão) e a saída pro
    /// prédio. Mesma ideia do MapaEscritorioProway: a arte do Lindomar
    /// (Artes/Cenarios/location2.xp) É o mapa, desenhada célula por célula como
    /// terreno de verdade, não como ilustração ao lado.
    ///
    /// "Sair do prédio" aqui NÃO leva pra outro IMapa novo - a cidade já existe
    /// (MapaCidadeBlumenau) e é justamente a tela de onde o jogador clicou em
    /// "ProWay" pra entrar. Por isso esse mapa não implementa MapaDestino: o
    /// ExploracaoScreen reconhece o TileType.SaidaPredio direto e volta pra tela
    /// anterior (a cidade). Ver ExploracaoScreen.Mover.
    ///
    /// Mesma ressalva do MapaEscritorioProway sobre colisão: o .xp não marca
    /// paredes, só cor de fundo preenchendo a tela inteira - por isso o andar
    /// inteiro é caminhável, e a saída é um ponto exato de ativação na
    /// extremidade de baixo do corredor de entulho que o Lindomar desenhou
    /// descendo no meio da imagem.
    /// </summary>
    internal class MapaAndarZero : IMapa
    {
        private const string CaminhoXp = "Artes/Cenarios/location2.xp";

        // Coordenadas reais dentro do .xp (60x60). A entrada é a "portinha"
        // escura desenhada no topo-centro (onde o elevador chega); a saída é o
        // fim do corredor de entulho que desce até a borda de baixo do desenho.
        private static readonly Point PosicaoEntrada = new(29, 3);
        private static readonly Point PosicaoSaida = new(25, 59);

        private readonly ScreenSurface _arte;

        public int Largura { get; }
        public int Altura { get; }
        public Point PontoEntrada => PosicaoEntrada;

        public MapaAndarZero()
        {
            _arte = ArteUtils.CarregarArteCenario(CaminhoXp);
            Largura = _arte.Width;
            Altura = _arte.Height;
        }

        public Tile ObterTile(int x, int y)
            => new Point(x, y) == PosicaoSaida ? TileFactory.Criar(TileType.SaidaPredio) : TileFactory.Criar(TileType.Chao);

        public bool EhBloqueado(int x, int y)
            => x < 0 || x >= Largura || y < 0 || y >= Altura;

        public void DesenharEm(ScreenSurface superficie)
            => _arte.Surface.Copy(superficie.Surface, 0, 0);

        public string? Dica =>
            "Você chegou na cafeteria (andar 0). Desça pelo corredor de entulho até o fim para voltar pra rua.";
    }
}
