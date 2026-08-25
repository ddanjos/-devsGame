using System.Collections.Generic;
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
    /// Sobre colisão: o .xp não marca "isso é parede" com nenhum caractere -
    /// é tudo desenhado só com cor de fundo. Ainda assim, analisando os pixels
    /// (script Python, gzip+struct) achamos que a cor (52,52,52) forma
    /// consistentemente o contorno retangular de cada sala - é a cor de parede
    /// de verdade. Comparado com a arte renderizada, bate certinho com as
    /// linhas de parede que dá pra ver a olho nu. Verificamos com um BFS
    /// (busca em largura) que bloquear essa cor NÃO isola nenhum ponto
    /// importante do mapa, com uma única exceção documentada abaixo.
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

        // Cor de parede real, identificada por análise de pixel (ver comentário
        // da classe). Bloqueia o movimento onde aparecer.
        private static readonly Color CorParede = new(52, 52, 52);

        // A sala onde fica a escada é cercada por essa cor de parede quase
        // inteira - só essa ÚNICA célula (confirmada por busca em largura, o
        // menor número de paredes cruzadas até a escada) liga o corredor
        // periférico até lá dentro. Sem essa exceção pontual, a escada - e o
        // porão inteiro - ficariam impossíveis de alcançar. Provavelmente
        // corresponde a uma porta que o Lindomar desenhou sem destacar a cor.
        private static readonly Point ExcecaoParede = new(22, 15);

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
        {
            if (x < 0 || x >= Largura || y < 0 || y >= Altura) return true;
            if (new Point(x, y) == ExcecaoParede) return false;
            return _arte.Surface.GetBackground(x, y) == CorParede;
        }

        /// <summary>Copia a arte do Lindomar direto pra tela usando as dimensões 
        /// originais exatas para evitar que o desenho fique puxado ou esticado.</summary>
        public void DesenharEm(ScreenSurface superficie)
        {
            // Limpa a área antiga para não sobrepor lixo visual
            superficie.Surface.Clear();

            // Copia especificando a subárea exata (da origem 0,0 até a Largura/Altura originais da arte)
            // para o destino (0,0) da tela do jogo, garantindo proporção 1:1 pixel por pixel
            _arte.Surface.Copy(0, 0, Largura, Altura, superficie.Surface, 0, 0);
        }


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
            "Você está no escritório da ProWay. Ande até perto do indicador azul no meio do corredor e aperte E para chamar o elevador.";

        /// <summary>Elevador (desce pro andar 0) e escada (desce pro porão) viram
        /// prompt de "aperte E" assim que o jogador chega perto - ver comentário em
        /// IMapa.PontosInteresse.</summary>
        public IReadOnlyList<(Point Posicao, string Rotulo)> PontosInteresse => new (Point, string)[]
        {
            (PosicaoElevador, "chamar o elevador"),
            (PosicaoEscada, "descer a escada pro porão"),
        };
    }
}
