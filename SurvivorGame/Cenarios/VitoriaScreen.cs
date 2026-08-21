using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;

namespace SurvivorGame.Cenarios
{
    /// <summary>
    /// Tela de vitória: atingida quando o jogador vence um combate e, com esse
    /// resultado, já tem os 3 itens-chave (GerenciadorJogo.PodeTransmitir). Fim de
    /// jogo por completude do objetivo, não por morte.
    /// </summary>
    internal class VitoriaScreen : ScreenSurface
    {
        private readonly System.Action _aoSair;

        public VitoriaScreen(System.Action aoSair, int largura, int altura)
            : base(largura, altura)
        {
            _aoSair = aoSair;

            UseKeyboard = true;
            IsFocused = true;

            Redesenhar();
        }

        public override bool ProcessKeyboard(Keyboard keyboard)
        {
            if (keyboard.IsKeyPressed(Keys.Enter) || keyboard.IsKeyPressed(Keys.Escape))
            {
                _aoSair();
            }

            return true;
        }

        private void Redesenhar()
        {
            Surface.Clear();

            string[] linhas =
            {
                "VOCÊ MONTOU O TRANSMISSOR E ENVIOU O SINAL!",
                "",
                "Um resgate está a caminho.",
                "",
                "FIM DE JOGO - Pressione Enter para sair"
            };

            int y = Height / 2 - linhas.Length / 2;
            foreach (string linha in linhas)
            {
                Surface.Print(Width / 2 - linha.Length / 2, y, linha, Color.LimeGreen, Color.Black);
                y++;
            }
        }
    }
}
