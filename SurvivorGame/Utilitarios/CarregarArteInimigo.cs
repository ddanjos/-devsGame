using System.IO;
using SadConsole;
using SadConsole.Readers;

namespace SurvivorGame.Utilitarios;

public static class ArteUtils
{
    public static ScreenSurface CarregarArteInimigo(string caminhoXP)
    {
        if (!File.Exists(caminhoXP))
        {
            throw new FileNotFoundException($"Arquivo de arte não encontrado em: {caminhoXP}");
        }

        // 1. Carrega a imagem do REXPaint via Stream
        using Stream stream = File.OpenRead(caminhoXP);
        REXPaintImage rexImage = REXPaintImage.Load(stream);

        // 2. Converte a REXPaintImage em uma CellSurface (superfície de células do SadConsole)
        // O método ToCellSurface() é o padrão mantido na API do SadConsole para converter .xp
        ICellSurface[] camadas = rexImage.ToCellSurface();

        // 3. Cria a ScreenSurface usando a superfície de células recém-gerada
        ScreenSurface superficie = new ScreenSurface(camadas[0]);

        return superficie;
    }
}