using System.Collections.Generic;
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
    /// Mesma ideia do MapaEscritorioProway sobre colisão: analisamos os pixels
    /// do .xp e achamos duas cores que formam paredes de verdade aqui - o
    /// contorno cinza (77,77,77) que fecha a cafeteria e os balcões, e uma
    /// faixa preta (0,0,0) que é claramente um pilar/divisória sólida numa das
    /// salas. Confirmamos por busca em largura que isso não isola a saída, com
    /// uma exceção pontual documentada abaixo.
    /// </summary>
    internal class MapaAndarZero : IMapa
    {
        private const string CaminhoXp = "Artes/Cenarios/location2.xp";

        // Coordenadas reais dentro do .xp (60x60). A entrada é a "portinha"
        // escura desenhada no topo-centro (onde o elevador chega); a saída é o
        // fim do corredor de entulho que desce até a borda de baixo do desenho.
        private static readonly Point PosicaoEntrada = new(29, 3);
        private static readonly Point PosicaoSaida = new(25, 59);

        // Cores de parede reais, identificadas por análise de pixel (ver
        // comentário da classe).
        private static readonly Color CorParede1 = new(77, 77, 77);
        private static readonly Color CorParede2 = new(0, 0, 0);

        // O corredor de entulho (rubble) reusa a MESMA cor cinza das paredes pra
        // desenhar destroços espalhados - o que, célula por célula, forma um
        // labirinto de verdade (testamos com busca em largura). O problema é que
        // não tem NENHUM jeito visual de saber qual destroço é andável e qual
        // bloqueia - o jogador ficaria tentando cada célula no escuro, o que não
        // é justo. Por isso esse retângulo (o corredor inteiro, da onde as salas
        // terminam até a borda de baixo) vira sempre andável, ignorando a cor -
        // as paredes das SALAS continuam bloqueando normalmente, só o corredor
        // de passagem que vira piso aberto.
        private static bool NoCorredorDeSaida(int x, int y)
            => x is >= 24 and <= 34 && y is >= 30 and <= 59;

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
        {
            if (x < 0 || x >= Largura || y < 0 || y >= Altura) return true;
            if (NoCorredorDeSaida(x, y)) return false;
            Color cor = _arte.Surface.GetBackground(x, y);
            return cor == CorParede1 || cor == CorParede2;
        }

        public void DesenharEm(ScreenSurface superficie)
            => _arte.Surface.Copy(superficie.Surface, 0, 0);

        public string? Dica =>
            "Você chegou na cafeteria (andar 0). Ignore as mesas e cadeiras dos lados - desça pelo corredor bem no centro da tela até perto da borda de baixo e aperte E para voltar pra rua.";

        /// <summary>A saída também vira prompt de "aperte E" assim que o jogador
        /// chega perto, além de continuar funcionando ao pisar exatamente em cima
        /// dela (ver Mover em ExploracaoScreen) - ver comentário em
        /// IMapa.PontosInteresse.</summary>
        public IReadOnlyList<(Point Posicao, string Rotulo)> PontosInteresse => new (Point, string)[]
        {
            (PosicaoSaida, "sair pro andar de cima"),
        };
    }
}
