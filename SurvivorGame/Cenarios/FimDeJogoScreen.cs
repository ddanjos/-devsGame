using System;
using System.Collections.Generic;
using System.Text;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using SurvivorGame.Utilitarios;
using SurvivorGame.Audio;

namespace SurvivorGame.Cenarios
{
    /// <summary>
    /// Tela final do jogo - vitória (transmissão enviada) ou derrota (vida chegou
    /// a zero). Até agora o jogo simplesmente não TINHA fim: perder um combate só
    /// voltava pro mapa com a vida zerada, e a condição de vitória
    /// (GerenciadorJogo.PodeTransmitir) nunca era consultada em lugar nenhum.
    ///
    /// Volta pro MENU PRINCIPAL ao apertar uma tecla. Antes chamava
    /// Environment.Exit(0), porque o menu ainda não existia (SCRUM-8); agora existe,
    /// então fechar a janela na cara do jogador virou bug - inclusive no melhor
    /// momento do jogo, que é a tela de vitória.
    ///
    /// A tecla que ABRIU esta tela é ignorada de propósito: o SadConsole repete
    /// tecla segurada (~25x por segundo depois de 0,8s), então "pressione qualquer
    /// tecla" disparava sozinho e o texto do final sumia antes de ser lido.
    /// </summary>
    internal class FimDeJogoScreen : ScreenSurface
    {
        private readonly bool _venceu;
        private readonly string _detalhe;

        public FimDeJogoScreen(bool venceu, string detalhe, int largura, int altura)
            : base(largura, altura)
        {
            _venceu = venceu;
            _detalhe = detalhe;

            UseKeyboard = true;
            IsFocused = true;

            Desenhar();
        }

        /// <summary>Vira true no primeiro frame em que NENHUMA tecla está
        /// pressionada. Até lá, ignoramos o teclado - é o que impede a tecla ainda
        /// segurada do combate ou da ação anterior de pular esta tela.</summary>
        private bool _tecladoLiberado;

        public override bool ProcessKeyboard(Keyboard keyboard)
        {
            if (!_tecladoLiberado)
            {
                if (keyboard.KeysDown.Count == 0) _tecladoLiberado = true;
                return true;
            }

            if (keyboard.KeysPressed.Count > 0)
            {
                GerenciadorSom.TocarTrilha(Trilha.Exploracao);
                Program.MostrarMenuPrincipal();
            }

            return true;
        }

        private void Desenhar()
        {
            Surface.Clear();

            string titulo = _venceu ? "SINAL ENVIADO" : "FIM DA JORNADA";
            Color corTitulo = _venceu ? Color.Gold : Color.IndianRed;

            int y = Height / 2 - 6;
            Surface.PrintTexto(Math.Max(2, (Width - titulo.Length) / 2), y, titulo, corTitulo, Color.Black);

            y += 3;
            foreach (string linha in QuebrarLinhas(_detalhe, Width - 8))
            {
                Surface.PrintTexto(4, y, linha, Color.White, Color.Black);
                y++;
            }

            Surface.PrintTexto(2, Height - 2, "Pressione qualquer tecla para voltar ao menu.", Color.Gray, Color.Black);
        }

        private static IEnumerable<string> QuebrarLinhas(string texto, int larguraMaxima)
        {
            string[] palavras = texto.Split(' ');
            var linhaAtual = new StringBuilder();

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

        /// <summary>Texto de vitória - fim da missão principal do HISTORIA.md.</summary>
        public const string TextoVitoria =
            "Você encaixa a antena, prende a bateria e troca o fusível queimado. O rádio " +
            "chia, engasga, e então: uma voz do outro lado da estática. Eles ouviram. " +
            "Uma equipe de resgate está a caminho de Blumenau. Você senta no chão do " +
            "escritório e, pela primeira vez desde o Evento, respira fundo.";

        /// <summary>Texto de derrota por vida zerada.</summary>
        public const string TextoDerrota =
            "Suas forças acabam. O rádio na mesa da ProWay continua ali, quieto, " +
            "esperando alguém que consiga terminar o que você começou.";

        /// <summary>Derrota específica por fome/sede - o aviso vinha sendo dado a " +
        /// cada ação, então vale nomear a causa.</summary>
        public const string TextoDerrotaInanicao =
            "Sem comida e sem água, o corpo simplesmente para. O rádio na mesa da ProWay " +
            "continua ali, quieto, esperando alguém que consiga terminar o que você começou.";
    }
}
