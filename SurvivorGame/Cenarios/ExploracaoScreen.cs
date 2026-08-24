using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using SurvivorGame.Mapa;
using SurvivorGame.Utilitarios;

namespace SurvivorGame.Cenarios
{
    /// <summary>
    /// Tela de exploração livre: o jogador anda de verdade (setas ou WASD) por um
    /// IMapa, com colisão contra paredes. Essa peça não existia em lugar nenhum do
    /// projeto - até aqui só havia clique de mouse no mapa estático da cidade
    /// (MapaScreen) e navegação por menu em telas de combate/inventário. É
    /// reaproveitável pra qualquer IMapa: hoje usada pro andar da ProWay e pro
    /// andar 0 (ver Mapa/MapaEscritorioProway e Mapa/MapaAndarZero), mas serve
    /// igual pra MapaMasmorra ou qualquer mapa de interior futuro.
    ///
    /// Ao entrar num mapa com CaminhoArte definido, mostra a arte .xp em tela
    /// cheia primeiro ("pressione qualquer tecla para entrar") - mesma ideia do
    /// sprite de inimigo no CombateScreen, só que ocupando a tela toda - e só
    /// libera o movimento depois disso.
    ///
    /// A posição do jogador AQUI DENTRO (_posicao) é local a este mapa de
    /// interior e NÃO sobrescreve Personagem.X/Y (que é a posição dele no mapa da
    /// cidade) - senão, ao sair do prédio, o "@" apareceria em coordenadas sem
    /// sentido no mapa da cidade.
    /// </summary>
    internal class ExploracaoScreen : ScreenSurface
    {
        private readonly IMapa _mapa;
        private readonly Personagem _jogador;
        private readonly IScreenObject _telaAnterior;
        private readonly MapaJogo _itensNoChao;
        private readonly ScreenSurface? _arteIntroducao;

        private Point _posicao;
        private bool _explorando;
        private string _mensagem = string.Empty;
        private (Point Posicao, string Rotulo)? _pontoProximo;

        /// <summary>
        /// O tamanho da tela é o tamanho do PRÓPRIO mapa (mapa.Largura/Altura) -
        /// pros mapas de interior baseados em REXPaint isso é literalmente 60x60,
        /// o mesmo tamanho do arquivo .xp do Lindomar, então o que o jogador vê
        /// enquanto anda é a arte original dele, célula por célula, não uma cópia
        /// redesenhada. A janela do jogo já é grande o suficiente pra isso (ver
        /// Program.cs).
        /// </summary>
        public ExploracaoScreen(IMapa mapa, Personagem jogador, IScreenObject telaAnterior,
            MapaJogo? itensNoChao = null)
            : base(mapa.Largura, mapa.Altura)
        {
            _mapa = mapa;
            _jogador = jogador;
            _telaAnterior = telaAnterior;
            _itensNoChao = itensNoChao ?? new MapaJogo();
            _posicao = mapa.PontoEntrada;
            _mensagem = mapa.Dica ?? string.Empty;

            _arteIntroducao = mapa.CaminhoArte is not null
                ? ArteUtils.CarregarArteCenario(mapa.CaminhoArte)
                : null;
            _explorando = _arteIntroducao is null;

            AtualizarPontoProximo();

            UseKeyboard = true;
            IsFocused = true;

            Redesenhar();
        }

        public override bool ProcessKeyboard(Keyboard keyboard)
        {
            if (!_explorando)
            {
                // Qualquer tecla dispensa a tela de arte e libera o movimento.
                if (keyboard.KeysPressed.Count > 0)
                {
                    _explorando = true;
                    Redesenhar();
                }
                return true;
            }

            if (keyboard.IsKeyPressed(Keys.Escape))
            {
                VoltarParaTelaAnterior();
                return true;
            }

            if (keyboard.IsKeyPressed(Keys.E) && _pontoProximo is not null)
            {
                InteragirComPontoProximo();
                return true;
            }

            int dx = 0, dy = 0;
            if (keyboard.IsKeyPressed(Keys.Up) || keyboard.IsKeyPressed(Keys.W)) dy = -1;
            else if (keyboard.IsKeyPressed(Keys.Down) || keyboard.IsKeyPressed(Keys.S)) dy = 1;
            else if (keyboard.IsKeyPressed(Keys.Left) || keyboard.IsKeyPressed(Keys.A)) dx = -1;
            else if (keyboard.IsKeyPressed(Keys.Right) || keyboard.IsKeyPressed(Keys.D)) dx = 1;

            if (dx != 0 || dy != 0)
                Mover(dx, dy);

            return true;
        }

        private void Mover(int dx, int dy)
        {
            int novoX = _posicao.X + dx;
            int novoY = _posicao.Y + dy;

            if (novoX < 0 || novoY < 0 || novoX >= _mapa.Largura || novoY >= _mapa.Altura)
                return;

            if (_mapa.EhBloqueado(novoX, novoY))
                return;

            _posicao = new Point(novoX, novoY);
            _mensagem = string.Empty;
            AtualizarPontoProximo();

            // Pegar item do chão, se houver um nessa posição (reaproveita
            // MapaJogo/AdicionarItem, que já existiam mas nunca eram acionados
            // porque nada no jogo se movia até agora).
            var itemNoChao = _itensNoChao.ObterItensNaPosicao(novoX, novoY);
            if (itemNoChao is not null)
            {
                bool coletou = _jogador.Inventario.AdicionarItem(itemNoChao.Item);
                if (coletou)
                {
                    _itensNoChao.RemoverItem(itemNoChao);
                    _mensagem = $"Você pegou: {itemNoChao.Item.Nome}";
                }
                else
                {
                    _mensagem = "Mochila cheia! Não foi possível pegar o item.";
                }
            }

            // Saída do prédio: não é uma transição pra outro IMapa, é "fechar" a
            // exploração e voltar pra cidade que já existia.
            if (_mapa.ObterTile(novoX, novoY).Tipo == TileType.SaidaPredio)
            {
                VoltarParaTelaAnterior();
                return;
            }

            // Outros gatilhos (ex: elevador) levam a um IMapa de destino - abre
            // uma nova ExploracaoScreen pra ele.
            IMapa? proximoMapa = _mapa.MapaDestino(novoX, novoY);
            if (proximoMapa is not null)
            {
                var proximaTela = new ExploracaoScreen(proximoMapa, _jogador, _telaAnterior, _itensNoChao);
                Game.Instance.Screen = proximaTela;
                Game.Instance.Screen.IsFocused = true;
                return;
            }

            Redesenhar();
        }

        private void VoltarParaTelaAnterior()
        {
            Game.Instance.Screen = _telaAnterior;
            Game.Instance.Screen!.IsFocused = true;
        }

        /// <summary>Checa se o jogador ficou a uma célula (incluindo diagonal) de
        /// algum PontoInteresse do mapa (elevador, escada, saída...) - se sim,
        /// guarda pra 'E' poder acionar e pro prompt aparecer. Mesma ideia (e
        /// mesma distância "Chebyshev") do MapaScreen.AtualizarLocalProximo, só que
        /// aplicada aos gatilhos de DENTRO de um mapa de interior.</summary>
        private void AtualizarPontoProximo()
        {
            _pontoProximo = null;
            foreach (var ponto in _mapa.PontosInteresse)
            {
                int dx = System.Math.Abs(ponto.Posicao.X - _posicao.X);
                int dy = System.Math.Abs(ponto.Posicao.Y - _posicao.Y);
                if (System.Math.Max(dx, dy) <= 1)
                {
                    _pontoProximo = ponto;
                    break;
                }
            }
        }

        /// <summary>Aciona o ponto de interesse mais próximo (tecla E). Reaproveita
        /// exatamente a mesma lógica que já existia em Mover pra quando o jogador
        /// pisava exatamente na célula: se for a saída do prédio, volta pra tela
        /// anterior; se for um gatilho de transição (elevador, escada), abre o
        /// IMapa de destino numa nova ExploracaoScreen.</summary>
        private void InteragirComPontoProximo()
        {
            if (_pontoProximo is null) return;
            Point posicao = _pontoProximo.Value.Posicao;

            if (_mapa.ObterTile(posicao.X, posicao.Y).Tipo == TileType.SaidaPredio)
            {
                VoltarParaTelaAnterior();
                return;
            }

            IMapa? proximoMapa = _mapa.MapaDestino(posicao.X, posicao.Y);
            if (proximoMapa is not null)
            {
                var proximaTela = new ExploracaoScreen(proximoMapa, _jogador, _telaAnterior, _itensNoChao);
                Game.Instance.Screen = proximaTela;
                Game.Instance.Screen.IsFocused = true;
            }
        }

        private void Redesenhar()
        {
            Surface.Clear();

            if (!_explorando && _arteIntroducao is not null)
            {
                int posX = System.Math.Max(0, (Width / 2) - (_arteIntroducao.Width / 2));
                int posY = System.Math.Max(0, (Height / 2) - (_arteIntroducao.Height / 2));
                _arteIntroducao.Surface.Copy(Surface, posX, posY);
                Surface.Print(2, Height - 1, "Pressione qualquer tecla para entrar...", Color.Gray, Color.Black);
                return;
            }

            _mapa.DesenharEm(this);

            // A arte original do Lindomar (copiada célula por célula acima) não
            // marca elevador/escada/saída com nada de especial - visualmente são
            // idênticos ao resto do cenário. Isso é a causa raiz de "não sei como
            // sair da sala": o jogador só descobria esses pontos por sorte ou lendo
            // a Dica de texto. Agora todo PontoInteresse ganha um marcador bem
            // chamativo (asterisco amarelo em fundo vermelho) direto em cima da
            // arte, visível de longe, sem precisar chegar perto pra saber que ele
            // existe ali - chegar perto só ainda é necessário pra ATIVAR (tecla E).
            foreach (var ponto in _mapa.PontosInteresse)
                Surface.SetGlyph(ponto.Posicao.X, ponto.Posicao.Y, '*', Color.Yellow, Color.Red);

            Surface.SetGlyph(_posicao.X, _posicao.Y, '@', Color.LimeGreen, Color.Black);

            Surface.Print(2, Height - 1, "Setas/WASD para mover | E para interagir | ESC para voltar", Color.Gray, Color.Black);

            // Perto de um ponto de interesse (elevador, escada, saída...)? Esse
            // prompt tem prioridade sobre a mensagem comum (dica inicial ou aviso
            // de item pego) - é a ação mais relevante nesse instante. Pode ser mais
            // longo que uma linha cabe, então quebra em várias, iguais o
            // QuebrarLinhas do CenarioLocalScreen, impressas de baixo pra cima logo
            // acima do rodapé de controles.
            string textoMensagem = _pontoProximo is not null
                ? $"Perto: pressione E para {_pontoProximo.Value.Rotulo}."
                : _mensagem;

            if (!string.IsNullOrEmpty(textoMensagem))
            {
                List<string> linhas = QuebrarLinhas(textoMensagem, Width - 4).ToList();
                int linhaInicial = Height - 1 - linhas.Count;
                for (int i = 0; i < linhas.Count; i++)
                    Surface.Print(2, linhaInicial + i, linhas[i], Color.Yellow, Color.Black);
            }
        }

        /// <summary>Quebra um texto em linhas de até larguraMaxima caracteres, sem
        /// cortar palavras no meio (mesma ideia do CenarioLocalScreen).</summary>
        private static IEnumerable<string> QuebrarLinhas(string texto, int larguraMaxima)
        {
            string[] palavras = texto.Split(' ');
            var linhaAtual = new System.Text.StringBuilder();

            foreach (string palavra in palavras)
            {
                if (linhaAtual.Length + palavra.Length + 1 > larguraMaxima)
                {
                    yield return linhaAtual.ToString();
                    linhaAtual.Clear();
                }

                if (linhaAtual.Length > 0)
                    linhaAtual.Append(' ');

                linhaAtual.Append(palavra);
            }

            if (linhaAtual.Length > 0)
                yield return linhaAtual.ToString();
        }
    }
}
