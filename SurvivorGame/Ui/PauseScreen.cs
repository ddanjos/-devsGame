using System.Collections.Generic;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using SurvivorGame.Regras;

namespace SurvivorGame.Ui
{
    /// <summary>
    /// SISTEMA DE PAUSE (SCRUM-13). Abre com ESC em cima do mapa da cidade e
    /// congela a partida: o jogo é por turnos e nada acontece sozinho, então
    /// "pausar" aqui é literalmente trocar a tela ativa - o mapa continua
    /// existindo, intacto, esperando na variável _telaAnterior.
    ///
    /// É deste menu que se salva (SCRUM-11), e é de propósito que ele só exista no
    /// mapa: salvar só do mapa mantém o arquivo de save simples e impossível de
    /// carregar num estado quebrado. Ver o comentário em Regras/SaveJogo.
    ///
    /// Herda de TelaDeMenu, a mesma base do Menu Principal.
    /// </summary>
    internal class PauseScreen : TelaDeMenu
    {
        protected override string Titulo => "PAUSA";
        protected override string? Subtitulo => "A cidade espera. Nada acontece enquanto você decide.";
        protected override IReadOnlyList<ItemDeMenu> Opcoes => _opcoes;
        protected override string Rodape => "Setas + Enter | ESC para voltar ao jogo";
        protected override Color CorDaMensagem => _ultimoSalvamentoDeuCerto ? Color.LimeGreen : Color.OrangeRed;

        private readonly IScreenObject _telaAnterior;
        private readonly Personagem _jogador;
        private readonly List<ItemDeMenu> _opcoes;
        private bool _ultimoSalvamentoDeuCerto = true;

        public PauseScreen(Personagem jogador, IScreenObject telaAnterior, int largura, int altura)
            : base(largura, altura)
        {
            _jogador = jogador;
            _telaAnterior = telaAnterior;

            _opcoes = new List<ItemDeMenu>
            {
                new("Continuar jogando", Voltar),
                new("Salvar jogo", () => Salvar()),
                new("Salvar e sair para o menu", SalvarESair),
                new("Sair para o menu (sem salvar)", Program.MostrarMenuPrincipal),
            };

            Iniciar();
        }

        /// <summary>ESC faz o mesmo que "Continuar jogando" - é o que qualquer
        /// jogador tenta primeiro.</summary>
        protected override bool AoPressionarOutraTecla(Keyboard keyboard)
        {
            // Espelha a guarda do MapaScreen: o ESC que ABRIU esta tela pode ainda
            // estar pressionado, e o SadConsole repete tecla segurada. Sem isso, um
            // ESC segurado abria e fechava o pause dezenas de vezes por segundo.
            if (!keyboard.IsKeyDown(Keys.Escape))
                _escFoiSolto = true;

            if (_escFoiSolto && keyboard.IsKeyPressed(Keys.Escape))
                Voltar();

            return true;
        }

        private bool _escFoiSolto;

        private void Voltar()
        {
            Game.Instance.Screen = _telaAnterior;
            Game.Instance.Screen!.IsFocused = true;
        }

        private bool Salvar()
        {
            _ultimoSalvamentoDeuCerto = SaveJogo.Salvar(_jogador, out string erro);

            // A mensagem de erro do sistema traz o caminho completo do arquivo e
            // passa fácil das 90 colunas da tela - e Surface.Print transborda pra
            // linha de baixo, em cima do rodapé. Por isso o corte.
            Mensagem = _ultimoSalvamentoDeuCerto
                ? $"Jogo salvo em {SaveJogo.Caminho.Split(System.IO.Path.DirectorySeparatorChar)[^1]}."
                : "Não foi possível salvar: " + Truncar(erro, 55);

            Redesenhar();
            return _ultimoSalvamentoDeuCerto;
        }

        private static string Truncar(string texto, int maximo) =>
            texto.Length <= maximo ? texto : texto[..(maximo - 3)] + "...";

        /// <summary>Só sai se o save realmente deu certo - sair depois de uma falha
        /// silenciosa perderia a partida inteira.</summary>
        private void SalvarESair()
        {
            if (Salvar())
                Program.MostrarMenuPrincipal();
        }
    }
}
