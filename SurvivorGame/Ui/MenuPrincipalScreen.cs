using System;
using System.Collections.Generic;
using SadConsole;
using SurvivorGame.Regras;

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
        protected override string Titulo => "Survivor Blu";
        protected override string? Subtitulo => "Blumenau Apocalíptica";
        protected override IReadOnlyList<ItemDeMenu> Opcoes => _opcoes;

        private readonly List<ItemDeMenu> _opcoes;

        public MenuPrincipalScreen(int largura, int altura) : base(largura, altura)
        {
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
