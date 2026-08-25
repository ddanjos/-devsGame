using System;
using System.Collections.Generic;
using System.Linq;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using SurvivorGame.Audio;
using SurvivorGame.Utilitarios;

namespace SurvivorGame.Ui
{
    /// <summary>Uma linha de menu: o texto, se está disponível e o que fazer ao
    /// confirmar. A ação é um delegate, então a tela de menu não precisa saber
    /// NADA sobre o que cada opção faz - mesma ideia do AcaoLocal no
    /// ponto-e-clique (padrão Command).</summary>
    internal class ItemDeMenu
    {
        public string Texto { get; }
        public bool Habilitado { get; }
        public Action Executar { get; }

        /// <summary>Linha extra em cinza embaixo da opção (ex: a data do save).</summary>
        public string? Detalhe { get; }

        public ItemDeMenu(string texto, Action executar, bool habilitado = true, string? detalhe = null)
        {
            Texto = texto;
            Executar = executar;
            Habilitado = habilitado;
            Detalhe = detalhe;
        }
    }

    /// <summary>
    /// Classe BASE das telas de menu (SCRUM-8). O Menu Principal e o Menu de Pause
    /// são a mesma coisa em estrutura - título, lista de opções, navegação por
    /// seta, confirmar com Enter - e só mudam no CONTEÚDO. Em vez de escrever esse
    /// laço duas vezes, ele mora aqui uma vez só.
    ///
    /// É o padrão Template Method: a classe base define o esqueleto do
    /// comportamento (desenhar, navegar, confirmar) e deixa as subclasses
    /// preencherem as partes variáveis (Titulo, Opcoes). Foi exatamente essa
    /// tela-base que o Sistema de Pause (SCRUM-13) estava esperando pra existir.
    ///
    /// Opções desabilitadas (ex: "Continuar" sem nenhum save gravado) aparecem em
    /// cinza e são PULADAS na navegação, em vez de sumirem: o jogador vê que a
    /// funcionalidade existe e entende por que não pode usar agora.
    /// </summary>
    internal abstract class TelaDeMenu : ScreenSurface
    {
        protected abstract string Titulo { get; }
        protected virtual string? Subtitulo => null;
        protected abstract IReadOnlyList<ItemDeMenu> Opcoes { get; }
        protected virtual string Rodape => "Setas para escolher | Enter para confirmar";

        /// <summary>Aviso mostrado embaixo do menu (ex: "Jogo salvo!").</summary>
        protected string Mensagem { get; set; } = string.Empty;
        protected virtual Color CorDaMensagem => Color.LimeGreen;

        private int _indice;

        protected TelaDeMenu(int largura, int altura) : base(largura, altura)
        {
            // Cobre MenuPrincipalScreen, PauseScreen e OpcoesScreen de uma vez só -
            // ver Utilitarios/AjusteVisual pro porquê.
            this.CorrigirProporcaoDeCelula();
            UseKeyboard = true;
        }

        /// <summary>Chamado pela subclasse no fim do construtor dela - só aí as
        /// opções dela já existem e dá pra desenhar. Mesma armadilha de ordem que
        /// já nos mordeu no LocalExploravelScreen.</summary>
        protected void Iniciar()
        {
            _indice = PrimeiroHabilitado();
            Redesenhar();
            IsFocused = true;
        }

        public override void OnFocused()
        {
            base.OnFocused();
            if (Opcoes is null || Opcoes.Count == 0) return;
            Redesenhar();
        }

        public override bool ProcessKeyboard(Keyboard keyboard)
        {
            if (Opcoes.Count == 0) return true;

            if (keyboard.IsKeyPressed(Keys.Down)) { GerenciadorSom.Tocar(Efeito.MenuMover); Mover(1); return true; }
            if (keyboard.IsKeyPressed(Keys.Up)) { GerenciadorSom.Tocar(Efeito.MenuMover); Mover(-1); return true; }

            if (keyboard.IsKeyPressed(Keys.Enter))
            {
                ItemDeMenu escolhida = Opcoes[_indice];

                // O som sai daqui, na classe base: qualquer menu do jogo - principal,
                // pause, opções - ganha o retorno sonoro sem escrever uma linha.
                GerenciadorSom.Tocar(escolhida.Habilitado ? Efeito.MenuConfirmar : Efeito.Erro);

                if (escolhida.Habilitado) escolhida.Executar();
                return true;
            }

            return AoPressionarOutraTecla(keyboard);
        }

        /// <summary>Gancho pra subclasse tratar teclas próprias (ex: ESC fechando o
        /// pause). Por padrão não faz nada.</summary>
        protected virtual bool AoPressionarOutraTecla(Keyboard keyboard) => true;

        /// <summary>Anda pelo menu pulando o que está desabilitado. Roda no
        /// máximo Count vezes pra não entrar em laço infinito se TUDO estiver
        /// desabilitado.</summary>
        private void Mover(int passo)
        {
            for (int i = 0; i < Opcoes.Count; i++)
            {
                _indice = (_indice + passo + Opcoes.Count) % Opcoes.Count;
                if (Opcoes[_indice].Habilitado) break;
            }

            Redesenhar();
        }

        private int PrimeiroHabilitado()
        {
            int i = Opcoes.ToList().FindIndex(o => o.Habilitado);
            return i < 0 ? 0 : i;
        }

        protected void Redesenhar()
        {
            Surface.Clear();

            int y = Height / 4;
            Surface.PrintTexto(Centralizado(Titulo), y, Titulo, Color.OrangeRed, Color.Black);

            if (Subtitulo is not null)
                Surface.PrintTexto(Centralizado(Subtitulo), y + 2, Subtitulo, Color.Gray, Color.Black);

            int yOpcoes = Height / 2;
            for (int i = 0; i < Opcoes.Count; i++)
            {
                ItemDeMenu opcao = Opcoes[i];
                bool selecionada = i == _indice;

                string texto = (selecionada ? "> " : "  ") + opcao.Texto;
                Color cor = !opcao.Habilitado ? Color.DimGray
                          : selecionada ? Color.Yellow
                          : Color.White;

                int x = Centralizado(texto);
                Surface.PrintTexto(x, yOpcoes, texto, cor, Color.Black);
                yOpcoes++;

                if (opcao.Detalhe is not null)
                {
                    Surface.PrintTexto(x + 2, yOpcoes, opcao.Detalhe, Color.DarkGray, Color.Black);
                    yOpcoes++;
                }

                yOpcoes++;
            }

            if (!string.IsNullOrEmpty(Mensagem))
                Surface.PrintTexto(Centralizado(Mensagem), Height - 4, Mensagem, CorDaMensagem, Color.Black);

            Surface.PrintTexto(Centralizado(Rodape), Height - 2, Rodape, Color.Gray, Color.Black);
        }

        private int Centralizado(string texto) => Math.Max(0, (Width / 2) - (texto.Length / 2));
    }
}
