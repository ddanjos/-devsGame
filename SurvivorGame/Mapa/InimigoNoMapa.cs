using SadConsole;
using SadRogue.Primitives;
using SurvivorGame.Combate;

namespace SurvivorGame.Mapa;

public class InimigoNoMapa
{
    public int X { get; set; }
    public int Y { get; set; }

    // Suporte opcional a uma arte .xp do REXPaint
    public ScreenSurface? ArteXP { get; set; }

    // Fallback de caractere simples (se não houver .xp)
    public char Simbolo { get; set; }
    public Color Cor { get; set; }

    public Inimigo DadosCombate { get; }

    public InimigoNoMapa(int x, int y, char simbolo, Color cor, Inimigo dadosCombate, ScreenSurface? arteXP = null)
    {
        X = x;
        Y = y;
        Simbolo = simbolo;
        Cor = cor;
        DadosCombate = dadosCombate;
        ArteXP = arteXP;
    }
}