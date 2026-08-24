using System.IO;
using SadConsole;
using SadConsole.Readers;

namespace SurvivorGame.Utilitarios;

public static class ArteUtils
{
    public static ScreenSurface CarregarArteInimigo(string caminhoXP) => CarregarXP(caminhoXP);

    /// <summary>
    /// Mesma coisa que CarregarArteInimigo, só com um nome que deixa claro que
    /// também serve pra carregar a arte de CENÁRIOS inteiros (não só sprites de
    /// inimigo) - por exemplo os mapas de interior desenhados pelo Lindomar no
    /// REXPaint (ver ExploracaoScreen, que mostra essa arte como tela de entrada).
    /// </summary>
    public static ScreenSurface CarregarArteCenario(string caminhoXP) => CarregarXP(caminhoXP);

    private static ScreenSurface CarregarXP(string caminhoXP)
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