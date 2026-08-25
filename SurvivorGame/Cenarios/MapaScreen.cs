using System.Linq;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using SurvivorGame.Combate;
using SurvivorGame.Mapa;
using SurvivorGame.Ui;
using SurvivorGame.UI;
using SurvivorGame.Utilitarios;
using SurvivorGame.Audio;

namespace SurvivorGame.Cenarios
{
    /// <summary>
    /// Tela principal do overworld (a cidade). Antes essa lógica vivia solta em
    /// Program.cs como uma ScreenSurface "crua"; virou uma classe própria (mesmo
    /// padrão de CombateScreen/CenarioLocalScreen) porque precisávamos de
    /// ProcessKeyboard pra abrir o inventário com a tecla 'I' - uma ScreenSurface
    /// sem subclasse não tem como reagir a uma tecla específica.
    ///
    /// O jogador anda de verdade aqui também (setas/WASD), igual à
    /// ExploracaoScreen - a diferença é que a posição AQUI é _personagem.X/Y
    /// de verdade (a posição dele no mundo), não uma posição local só desse
    /// mapa. Colisão vem do próprio _terreno.EhBloqueado (prédio e água
    /// bloqueiam; rua, calçada, praça e ponte não). Ao ficar perto de um
    /// Local (ProWay etc.), aparece um prompt pra apertar 'E' e entrar - não
    /// entra sozinho só de encostar, pra não ser fácil de ativar sem querer.
    /// O clique do mouse continua funcionando (compatibilidade + combate).
    /// </summary>
    internal class MapaScreen : ScreenSurface
    {
        private readonly IMapa _terreno;
        private readonly MapaJogo _itensNoChao;
        private readonly MapaInimigos _inimigosNoMapa;
        private readonly Personagem _personagem;

        private LocalMapa? _localProximo;
        private string _mensagem = string.Empty;

        public MapaScreen(IMapa terreno, MapaJogo itensNoChao, MapaInimigos inimigosNoMapa, Personagem personagem)
            : base(terreno.Largura, terreno.Altura)
        {
            // A ORDEM IMPORTA: _personagem tem que ser o ÚLTIMO. A guarda de null
            // do OnFocused abaixo testa só ele, apostando que se ele já existe,
            // todo o resto também existe. Reordenar estas quatro linhas transforma
            // essa guarda numa NullReferenceException silenciosa.
            _terreno = terreno;
            _itensNoChao = itensNoChao;
            _inimigosNoMapa = inimigosNoMapa;
            _personagem = personagem;

            UseMouse = true;
            UseKeyboard = true;
            MouseButtonClicked += MapaTela_MouseButtonClicked;

            AtualizarLocalProximo();
            RedesenharMapaCompleto();
        }

        /// <summary>Ver a guarda de ESC em ProcessKeyboard. Começa false a cada
        /// vez que esta tela reganha o foco, porque a tecla que nos trouxe de volta
        /// pode ainda estar pressionada.</summary>
        private bool _escFoiSolto;

        /// <summary>Redesenha ao reganhar o foco - ao voltar do pause, do
        /// inventário ou de um local, a superfície ainda tem o desenho de antes.</summary>
        public override void OnFocused()
        {
            base.OnFocused();
            if (_personagem is null) return;

            _escFoiSolto = false;
            GerenciadorSom.TocarTrilha(Trilha.Exploracao);
            AtualizarLocalProximo();
            RedesenharMapaCompleto();
        }

        public override bool ProcessKeyboard(Keyboard keyboard)
        {
            // ESC abre o Menu de Pause (SCRUM-13). Só existe AQUI, no mapa: é de
            // lá que se salva, e salvar só do mapa mantém o save simples e
            // impossível de restaurar num estado quebrado (ver Regras/SaveJogo).
            // O SadConsole repete tecla segurada (~25x/s depois de 0,8s). Sem esta
            // guarda, segurar ESC fazia mapa e pause piscarem um no outro, e sair
            // de um local com ESC segurado caía direto no menu de pause. Só
            // aceitamos o ESC depois de ver um frame com ele solto.
            if (!keyboard.IsKeyDown(Keys.Escape))
                _escFoiSolto = true;

            if (_escFoiSolto && keyboard.IsKeyPressed(Keys.Escape))
            {
                _escFoiSolto = false;
                Game.Instance.Screen = new PauseScreen(
                    _personagem, this, Game.Instance.ScreenCellsX, Game.Instance.ScreenCellsY);
                Game.Instance.Screen.IsFocused = true;
                return true;
            }

            if (keyboard.IsKeyPressed(Keys.I))
            {
                Game.Instance.Screen = new InventarioScreen(_personagem, this, Game.Instance.ScreenCellsX, Game.Instance.ScreenCellsY, _itensNoChao);
                Game.Instance.Screen.IsFocused = true;
                return true;
            }

            if (keyboard.IsKeyPressed(Keys.E) && _localProximo is not null)
            {
                EntrarEm(_localProximo);
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
            int novoX = _personagem.X + dx;
            int novoY = _personagem.Y + dy;

            if (novoX < 0 || novoY < 0 || novoX >= _terreno.Largura || novoY >= _terreno.Altura)
                return;

            if (_terreno.EhBloqueado(novoX, novoY))
                return;

            _personagem.X = novoX;
            _personagem.Y = novoY;

            AtualizarLocalProximo();
            RedesenharMapaCompleto();
        }

        /// <summary>Checa se o jogador ficou a uma célula (incluindo diagonal) de
        /// algum Local - se sim, guarda pra 'E' poder entrar e mostra o prompt.
        /// Distância "Chebyshev" (a maior entre dx e dy) é o jeito certo de medir
        /// "vizinho, incluindo diagonal" numa grade - diferente da distância reta,
        /// que exageraria a diagonal.</summary>
        private void AtualizarLocalProximo()
        {
            _localProximo = MapaCidadeBlumenau.Locais.FirstOrDefault(l =>
            {
                int dx = System.Math.Abs(l.Posicao.X - _personagem.X);
                int dy = System.Math.Abs(l.Posicao.Y - _personagem.Y);
                return System.Math.Max(dx, dy) <= 1;
            });

            _mensagem = _localProximo is not null
                ? $"Perto de {_localProximo.Nome}. Pressione E para entrar, ou continue andando."
                : string.Empty;
        }

        private void MapaTela_MouseButtonClicked(object? sender, MouseScreenObjectState state)
        {
            Point celulaClicada = state.CellPosition;

            InimigoNoMapa? inimigoClicado = _inimigosNoMapa.ObterInimigoNaPosicao(celulaClicada);
            if (inimigoClicado is not null)
            {
                var combate = new CombateScreen(
                    _personagem,
                    inimigoClicado,
                    _inimigosNoMapa,
                    this,
                    Game.Instance.ScreenCellsX,
                    Game.Instance.ScreenCellsY,
                    RedesenharMapaCompleto
                );

                Game.Instance.Screen = combate;
                Game.Instance.Screen.IsFocused = true;
                return;
            }

            // Clique continua funcionando como atalho (compatibilidade com quem já
            // tinha o hábito) - mas o jeito "oficial" agora é andar até ficar perto
            // e apertar E, ver ProcessKeyboard/AtualizarLocalProximo.
            LocalMapa? local = MapaCidadeBlumenau.Locais
                .FirstOrDefault(l => l.Posicao == celulaClicada);

            if (local is not null)
                EntrarEm(local);
        }

        /// <summary>Abre o cenário de um Local - o escritório da ProWay (agora no
        /// formato ponto-e-clique do SCRUM-9, ver Mapa/LocalEscritorioProway), ou a
        /// telinha de descrição padrão pros demais. Compartilhado entre o prompt de
        /// proximidade (tecla E) e o clique do mouse.</summary>
        private void EntrarEm(LocalMapa local)
        {
            // A fábrica sabe montar o local jogável de cada ponto do mapa (ver
            // Mapa/FabricaLocais) - assim esta tela não precisa conhecer o conteúdo
            // do jogo inteiro. Se um ponto ainda não tiver conteúdo, ela devolve
            // null e caímos na telinha de descrição de sempre.
            ILocalExploravel? localJogavel = FabricaLocais.Criar(local.Nome);
            if (localJogavel is not null)
            {
                // Tamanho da JANELA, não o do mapa: os planos de fundo do Lindomar
                // são 60x60 e o mapa da cidade só tem 45 linhas - abrir o local no
                // tamanho do mapa cortaria um quarto de cada desenho.
                var tela = new LocalExploravelScreen(localJogavel, _personagem, this,
                    Game.Instance.ScreenCellsX, Game.Instance.ScreenCellsY);
                Game.Instance.Screen = tela;
                Game.Instance.Screen.IsFocused = true;
                return;
            }

            var cenario = new CenarioLocalScreen(local, this,
                Game.Instance.ScreenCellsX, Game.Instance.ScreenCellsY);
            Game.Instance.Screen = cenario;
            Game.Instance.Screen.IsFocused = true;
        }

        /// <summary>Redesenha terreno + itens no chão + inimigos + jogador, nessa ordem
        /// (cada camada por cima da anterior). Passada como callback pro CombateScreen
        /// chamar ao voltar do combate (pra sumir com o sprite do inimigo derrotado).</summary>
        public void RedesenharMapaCompleto()
        {
            _terreno.DesenharEm(this);

            foreach (var item in _itensNoChao.ItensNoChao)
                Surface.SetGlyph(item.X, item.Y, item.Simbolo, Color.White, Color.Black);

            foreach (var inimigo in _inimigosNoMapa.Inimigos)
                Surface.SetGlyph(inimigo.X, inimigo.Y, inimigo.Simbolo, inimigo.Cor, Color.Black);

            Surface.SetGlyph(_personagem.X, _personagem.Y, '@', Color.LimeGreen, Color.Black);

            // Missão sempre à vista: sem isso o jogador junta as 3 peças e não
            // descobre onde terminar o jogo (aconteceu em playtest).
            Surface.PrintTexto(2, Height - 3, Regras.GerenciadorJogo.ResumoDaMissao,
                Regras.GerenciadorJogo.PodeTransmitir ? Color.Gold : Color.LightGreen, Color.Black);

            Surface.PrintTexto(2, Height - 1, "Setas/WASD para mover | E entrar | I inventário | ESC pausar", Color.Gray, Color.Black);
            if (!string.IsNullOrEmpty(_mensagem))
                Surface.PrintTexto(2, Height - 2, _mensagem, Color.Yellow, Color.Black);
        }
    }
}
