using Microsoft.Xna.Framework.Audio;
using SadConsole;
using SadConsole.Readers;
using SadConsole.UI.Controls;
using SurvivorGame.Combate;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.IO;
namespace SurvivorGame.Utilitarios;

public static class ArteUtils
{
    public static ScreenSurface CarregarArteInimigo(string caminhoXP)
    {
        // Se o caminho direto não existir, tenta resolver problemas de letras maiúsculas/minúsculas
        if (!File.Exists(caminhoXP))
        {
            string diretorio = Path.GetDirectoryName(caminhoXP) ?? "";
            string nomeArquivo = Path.GetFileName(caminhoXP);

            // Tenta achar forçando o nome do arquivo todo em minúsculo
            string caminhoMinusculo = Path.Combine(diretorio, nomeArquivo.ToLower());
            if (File.Exists(caminhoMinusculo))
            {
                caminhoXP = caminhoMinusculo;
            }
            // Tenta achar forçando a primeira letra em maiúsculo (PascalCase simples) se o de cima falhar
            else if (nomeArquivo.Length > 0)
            {
                string caminhoPascal = Path.Combine(diretorio, char.ToUpper(nomeArquivo[0]) + nomeArquivo.Substring(1));
                if (File.Exists(caminhoPascal))
                {
                    caminhoXP = caminhoPascal;
                }
                // Fallback de segurança extrema: se não achar nada na pasta de inimigos, usa o ratoselvagem para o jogo não travar
                else if (caminhoXP.Contains("Inimigos"))
                {
                    caminhoXP = Path.Combine(diretorio, "ratoselvagem.xp");
                }
            }
        }

        return CarregarXP(caminhoXP);
    }

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
    



