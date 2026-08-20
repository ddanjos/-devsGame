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

        public ExploracaoScreen(IMapa mapa, Personagem jogador, IScreenObject telaAnterior,
            int larguraJanela, int alturaJanela, MapaJogo? itensNoChao = null)
            : base(larguraJanela, alturaJanela)
        {
            _mapa = mapa;
            _jogador = jogador;
            _telaAnterior = telaAnterior;
            _itensNoChao = itensNoChao ?? new MapaJogo();
            _posicao = mapa.PontoEntrada;

            _arteIntroducao = mapa.CaminhoArte is not null
                ? ArteUtils.CarregarArteCenario(mapa.CaminhoArte)
                : null;
            _explorando = _arteIntroducao is null;

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
                var proximaTela = new ExploracaoScreen(proximoMapa, _jogador, _telaAnterior, Width, Height, _itensNoChao);
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
            Surface.SetGlyph(_posicao.X, _posicao.Y, '@', Color.LimeGreen, Color.Black);

            Surface.Print(2, Height - 2, "Setas/WASD para mover | ESC para voltar", Color.Gray, Color.Black);
            if (!string.IsNullOrEmpty(_mensagem))
                Surface.Print(2, Height - 1, _mensagem, Color.Yellow, Color.Black);
        }
    }
}
