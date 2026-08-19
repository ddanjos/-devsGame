using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using SurvivorGame.Mapa;
using System.Text;

namespace SurvivorGame.Cenarios
{
    /// <summary>
    /// Cenário simples de um lugar do mapa. Por enquanto só mostra nome + descrição
    /// do lugar clicado; no futuro cada lugar pode ganhar seu próprio cenário
    /// jogável (ambiente próprio, NPCs, itens, etc) — essa classe é só o esqueleto.
    ///
    /// Troca de tela = State pattern: Game.Instance.Screen aponta pra UM objeto por
    /// vez (o mapa OU um cenário). Program.cs não precisa saber qual está ativo,
    /// só troca o Screen quando necessário.
    /// </summary>
    internal class CenarioLocalScreen : ScreenSurface
    {
        private readonly IScreenObject _telaAnterior;

        public CenarioLocalScreen(LocalMapa local, IScreenObject telaAnterior, int largura, int altura)
            : base(largura, altura)
        {
            _telaAnterior = telaAnterior;

            UseKeyboard = true;
            IsFocused = true;

            Surface.Clear();
            Surface.Print(2, 2, local.Nome, Color.Gold, Color.Black);

            int linha = 4;
            foreach (string trecho in QuebrarLinhas(local.Descricao, largura - 4))
            {
                Surface.Print(2, linha, trecho, Color.White, Color.Black);
                linha++;
            }

            Surface.Print(2, altura - 2, "Pressione ESC para voltar ao mapa", Color.Gray, Color.Black);
        }

        public override bool ProcessKeyboard(Keyboard keyboard)
        {
            if (keyboard.IsKeyPressed(Keys.Escape))
            {
                Game.Instance.Screen = _telaAnterior;
                Game.Instance.Screen!.IsFocused = true;
                return true;
            }

            return base.ProcessKeyboard(keyboard);
        }

        /// <summary>Quebra um texto em linhas de até larguraMaxima caracteres, sem cortar palavras no meio.</summary>
        private static IEnumerable<string> QuebrarLinhas(string texto, int larguraMaxima)
        {
            var palavras = texto.Split(' ');
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
    }
}
