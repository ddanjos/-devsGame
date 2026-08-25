using System;
using System.Collections.Generic;
using SadConsole;
using SurvivorGame.Regras;
using SadConsole.Input;
using SadRogue.Primitives;

namespace SurvivorGame.Ui
{
    /// <summary>
    /// MENU DO JOGO (SCRUM-8) - a primeira tela da partida.
    ///
    /// Era uma tela que desenhava e lia o teclado por conta própria; virou uma
    /// subclasse de TelaDeMenu quando o Menu de Pause (SCRUM-13) apareceu e as
    /// duas ficaram idênticas em estrutura. Agora esta classe só DECLARA o que
    /// aparece - navegação, desenho e seleção são da classe base.
    ///
    /// "Continuar" só fica disponível se existir um savegame.json (SCRUM-11).
    /// Desabilitado ele continua visível, em cinza, pro jogador saber que a opção
    /// existe e por que não dá pra usar agora.
    /// </summary>
    internal class MenuPrincipalScreen : TelaDeMenu
    {
        protected override string Titulo => "";
        protected override string? Subtitulo => "";
        protected override IReadOnlyList<ItemDeMenu> Opcoes => _opcoes;

        private readonly List<ItemDeMenu> _opcoes;

        public MenuPrincipalScreen(int width, int height) : base(width, height)
        {
            // 1. Textos do cabeçalho
            string titulo = "O ULTIMO SINAL";
            string subTitulo = "Blumenau Apocaliptica";

            // 2. Criamos uma superfície dedicada ao título
            var surfaceTitulo = new ScreenSurface(titulo.Length, 1);

            // 3. Multiplicamos o tamanho da fonte por 4
            surfaceTitulo.FontSize = surfaceTitulo.Font.GetFontSize(SadConsole.IFont.Sizes.Four);

            // 4. Desenhamos o título com a cor estilizada
            surfaceTitulo.Surface.Print(0, 0, titulo, new Color(91, 209, 215));

            // 5. Posicionamos o título de forma centralizada/proporcional
            int tituloLarguraReal = titulo.Length * 2;
            surfaceTitulo.Position = new SadRogue.Primitives.Point((width - tituloLarguraReal) / 12, height / 12);

            // 6. Adicionamos a superfície do título como filha desta tela
            this.Children.Add(surfaceTitulo);

            // 7. O subtítulo na superfície principal
            this.Surface.Print(width / 2 - subTitulo.Length / 2, (height / 3) + 3, subTitulo, Color.Gray);

            // 8. Lógica das opções do seu antigo menu
            DateTime? quando = SaveJogo.QuandoFoiSalvo();

            _opcoes = new List<ItemDeMenu>
    {
        new("Continuar",
            Continuar,
            habilitado: quando is not null,
            detalhe: quando is not null
                ? $"salvo em {quando:dd/MM/yyyy HH:mm}"
                : "nenhum jogo salvo"),

        new("Novo Jogo", Program.IniciarNovaPartida),

        new("Opcoes", () =>
        {
            Game.Instance.Screen = new OpcoesScreen(this, Width, Height);
            Game.Instance.Screen.IsFocused = true;
        }),

        new("Sair do Jogo", () => Game.Instance.MonoGameInstance.Exit()),
    };

            Iniciar();
        }

        /// <summary>Se o save estiver corrompido, Carregar() devolve null - aí, em
        /// vez de travar, avisamos e deixamos o jogador escolher "Novo Jogo".</summary>
        private void Continuar()
        {
            if (Program.CarregarPartida()) return;

            Mensagem = "Não foi possível ler o jogo salvo. Comece um Novo Jogo.";
            Redesenhar();
        }
    }
}
