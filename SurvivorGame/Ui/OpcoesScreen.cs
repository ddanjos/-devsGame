using System.Collections.Generic;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using SurvivorGame.Audio;
using SurvivorGame.Regras;

namespace SurvivorGame.Ui
{
    /// <summary>
    /// OPÇÕES (fecha a SCRUM-8). Liga e desliga música e efeitos.
    ///
    /// É a terceira subclasse de TelaDeMenu, e é a que mostra por que a classe base
    /// valeu a pena: esta tela inteira são 4 itens de menu e dois métodos de duas
    /// linhas. Navegação, desenho, seleção e o ESC vêm de graça.
    ///
    /// Os itens são reconstruídos a cada alternância porque o texto deles mostra o
    /// estado atual ("Musica: LIGADA"). A alternativa - guardar a lista e editar o
    /// texto no lugar - deixaria o rótulo mentindo se alguém mudasse a configuração
    /// por outro caminho.
    /// </summary>
    internal class OpcoesScreen : TelaDeMenu
    {
        protected override string Titulo => "OPCOES";
        protected override string? Subtitulo => GerenciadorSom.Disponivel
            ? "Enter alterna. A escolha fica salva pra proxima vez."
            : "Sem dispositivo de audio nesta maquina - o jogo roda mudo.";
        protected override IReadOnlyList<ItemDeMenu> Opcoes => _opcoes;
        protected override string Rodape => "Setas + Enter | ESC para voltar";

        private readonly IScreenObject _telaAnterior;
        private List<ItemDeMenu> _opcoes = new();

        public OpcoesScreen(IScreenObject telaAnterior, int largura, int altura)
            : base(largura, altura)
        {
            _telaAnterior = telaAnterior;
            Montar();
            Iniciar();
        }

        private void Montar()
        {
            _opcoes = new List<ItemDeMenu>
            {
                new($"Musica:  {Ligado(Configuracao.MusicaLigada)}",
                    AlternarMusica,
                    habilitado: GerenciadorSom.Disponivel),

                new($"Efeitos: {Ligado(Configuracao.EfeitosLigados)}",
                    AlternarEfeitos,
                    habilitado: GerenciadorSom.Disponivel),

                new("Voltar", Voltar),
            };
        }

        /// <summary>Sem dispositivo de áudio, mostrar "LIGADA" seria mentira: o som
        /// não vai sair de qualquer jeito. Aí o rótulo vira um traço.</summary>
        private static string Ligado(bool valor) =>
            !GerenciadorSom.Disponivel ? "---" : valor ? "LIGADA" : "desligada";

        private void AlternarMusica()
        {
            Configuracao.MusicaLigada = !Configuracao.MusicaLigada;
            Configuracao.Salvar();

            // Efeito imediato: a música para ou volta na hora, sem sair da tela.
            GerenciadorSom.AplicarPreferencias();
            Redesenhado();
        }

        private void AlternarEfeitos()
        {
            Configuracao.EfeitosLigados = !Configuracao.EfeitosLigados;
            Configuracao.Salvar();

            // Toca o clique DEPOIS de ligar, pra o jogador ouvir a confirmação do
            // que acabou de ativar. Ao desligar, o próprio Tocar já não faz nada.
            GerenciadorSom.Tocar(Efeito.MenuConfirmar);
            Redesenhado();
        }

        private void Redesenhado()
        {
            Montar();
            Redesenhar();
        }

        protected override bool AoPressionarOutraTecla(Keyboard keyboard)
        {
            if (keyboard.IsKeyPressed(Keys.Escape)) Voltar();
            return true;
        }

        private void Voltar()
        {
            GerenciadorSom.Tocar(Efeito.MenuVoltar);
            Game.Instance.Screen = _telaAnterior;
            Game.Instance.Screen!.IsFocused = true;
        }
    }
}
