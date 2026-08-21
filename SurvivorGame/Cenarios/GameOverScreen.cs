using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;

namespace SurvivorGame.Cenarios
{
    /// <summary>
    /// Tela de fim de jogo por derrota (Vida chegou a 0 em combate). Único destino
    /// possível a partir daqui é reiniciar (novo Personagem, novo mapa de inimigos/
    /// itens, progresso de missão zerado) ou sair - não dá pra "continuar" morto.
    /// </summary>
    internal class GameOverScreen : ScreenSurface
    {
        private readonly System.Action _aoReiniciar;
        private readonly System.Action _aoSair;
        private int _indiceSelecionado;
        private readonly string[] _opcoes = { "Tentar Novamente", "Sair do Jogo" };

        public GameOverScreen(System.Action aoReiniciar, System.Action aoSair, int largura, int altura)
            : base(largura, altura)
        {
            _aoReiniciar = aoReiniciar;
            _aoSair = aoSair;

            UseKeyboard = true;
            IsFocused = true;

            Redesenhar();
        }

        public override bool ProcessKeyboard(Keyboard keyboard)
        {
            if (keyboard.IsKeyPressed(Keys.Up) || keyboard.IsKeyPressed(Keys.Down))
            {
                _indiceSelecionado = 1 - _indiceSelecionado;
                Redesenhar();
            }
            else if (keyboard.IsKeyPressed(Keys.Enter))
            {
                if (_indiceSelecionado == 0) _aoReiniciar();
                else _aoSair();
            }

            return true;
        }

        private void Redesenhar()
        {
            Surface.Clear();

            string titulo = "VOCÊ MORREU";
            Surface.Print(Width / 2 - titulo.Length / 2, Height / 2 - 3, titulo, Color.DarkRed, Color.Black);

            for (int i = 0; i < _opcoes.Length; i++)
            {
                bool selecionado = i == _indiceSelecionado;
                string prefixo = selecionado ? "> " : "  ";
                Color cor = selecionado ? Color.Yellow : Color.White;
                string linha = prefixo + _opcoes[i];
                Surface.Print(Width / 2 - linha.Length / 2, Height / 2 + i, linha, cor, Color.Black);
            }
        }
    }
}
