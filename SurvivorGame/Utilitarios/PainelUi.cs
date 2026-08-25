using SadConsole;
using SadRogue.Primitives;

namespace SurvivorGame.Utilitarios
{
    /// <summary>
    /// Duas ferramentas de desenho que passaram a ser necessárias quando os planos
    /// de fundo do Lindomar entraram (25/08). Antes as telas desenhavam texto sobre
    /// preto e nada disso fazia falta.
    /// </summary>
    internal static class PainelUi
    {
        /// <summary>
        /// Copia uma arte por cima do que já está na tela RESPEITANDO
        /// TRANSPARÊNCIA.
        ///
        /// Isso importa muito: o REXPaint marca o vazio com magenta, e o leitor do
        /// SadConsole traduz esse magenta pra uma cor com alfa 0. O Surface.Copy
        /// normal copia célula por célula, alfa incluído - então colar o sprite do
        /// inimigo em cima do fundo de batalha abriria um retângulo transparente em
        /// volta dele, apagando o cenário justamente onde ele deveria aparecer. O
        /// rato, por exemplo, tem 225 das suas 518 células transparentes.
        ///
        /// Aqui, célula vazia é pulada; célula que só tem glifo (fundo
        /// transparente, desenho visível) mantém o fundo do destino.
        /// </summary>
        public static void DesenharPorCima(ScreenSurface arte, ICellSurface destino, int emX, int emY)
        {
            for (int y = 0; y < arte.Surface.Height; y++)
            {
                for (int x = 0; x < arte.Surface.Width; x++)
                {
                    int dx = emX + x;
                    int dy = emY + y;

                    if (dx < 0 || dy < 0 || dx >= destino.Width || dy >= destino.Height)
                        continue;

                    ColoredGlyphBase origem = arte.Surface[x, y];

                    bool semDesenho = origem.Glyph == 0 || origem.Glyph == 32;
                    bool fundoTransparente = origem.Background.A == 0;

                    if (fundoTransparente && semDesenho)
                        continue;

                    destino.SetGlyph(dx, dy, origem.Glyph, origem.Foreground,
                        fundoTransparente ? destino[dx, dy].Background : origem.Background);
                }
            }
        }

        /// <summary>
        /// Faixa preta atrás do texto, com um traço fino separando da arte.
        ///
        /// Sem ela, cada letra impressa sobre o cenário fica com seu próprio
        /// retângulo preto recortado no meio do desenho, e o texto some de longe -
        /// exatamente o que não pode acontecer num projetor. Com a faixa, o
        /// conjunto lê como uma caixa de diálogo.
        ///
        /// Fill (e não só pintar o fundo) porque os glifos da arte continuariam
        /// desenhados por cima do texto se a célula não fosse limpa.
        /// </summary>
        public static void DesenharFaixa(ICellSurface superficie, int altura)
        {
            if (altura <= 0 || altura > superficie.Height) return;

            int topo = superficie.Height - altura;

            superficie.Fill(new Rectangle(0, topo, superficie.Width, altura),
                Color.White, Color.Black, 0);

            // 196 é o traço horizontal do CP437. Não dá pra usar '─' (Unicode
            // 9472): o SadConsole usa o código do char como índice de glifo, e
            // 9472 não existe na fonte. Mesma armadilha de Utilitarios/TextoCp437.
            superficie.DrawLine(new Point(0, topo), new Point(superficie.Width - 1, topo),
                196, Color.DarkSlateGray, Color.Black);
        }

        /// <summary>Mesma ideia da faixa, mas no TOPO da tela - usada pelo combate,
        /// que mostra nome e vida do inimigo lá em cima, sobre o cenário.</summary>
        public static void DesenharFaixaSuperior(ICellSurface superficie, int altura)
        {
            if (altura <= 0 || altura > superficie.Height) return;

            superficie.Fill(new Rectangle(0, 0, superficie.Width, altura),
                Color.White, Color.Black, 0);

            superficie.DrawLine(new Point(0, altura - 1), new Point(superficie.Width - 1, altura - 1),
                196, Color.DarkSlateGray, Color.Black);
        }
    }
}
