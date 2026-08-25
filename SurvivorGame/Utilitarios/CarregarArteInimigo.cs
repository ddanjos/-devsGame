using System;
using System.Collections.Generic;
using SadConsole;
using SadConsole.Readers;
using System.IO;

namespace SurvivorGame.Utilitarios;

/// <summary>
/// Carrega arte REXPaint (.xp) do disco.
///
/// Duas coisas importantes acontecem aqui, e as duas vieram de problema real:
///
/// 1. RESOLUÇÃO DE CAMINHO. Os .xp são copiados pra pasta de saída, ao lado do
///    executável, mas o caminho usado no código é relativo ("Artes/Cenarios/
///    x.xp"). Um caminho relativo é resolvido contra o diretório de TRABALHO do
///    processo, que nem sempre é a pasta do executável (atalho no desktop,
///    "dotnet run" da raiz do repositório, arrastar o .exe). Por isso tentamos
///    também contra AppContext.BaseDirectory, que é sempre a pasta do .exe.
///    Também toleramos diferença de maiúsculas/minúsculas no nome do arquivo,
///    que passa despercebida no Windows e quebra no Linux.
///
/// 2. FALHA SUAVE. Arte é enfeite, não regra: um .xp faltando ou corrompido NÃO
///    pode fechar o jogo. Antes, CarregarArteCenario lançava exceção e a exceção
///    subia pelo ProcessKeyboard, fechando a janela ao entrar num local. Agora
///    devolve null e a tela simplesmente desenha sem fundo. Com 14 arquivos de
///    arte no projeto, um erro de digitação em um nome não pode virar 14 formas
///    novas de o jogo morrer.
/// </summary>
public static class ArteUtils
{
    /// <summary>Sprite de inimigo. Se não achar nada, cai no rato - assim o
    /// combate nunca fica sem nenhum desenho.</summary>
    public static ScreenSurface CarregarArteInimigo(string caminhoXP)
    {
        ScreenSurface? arte = TentarCarregar(caminhoXP);
        if (arte is not null) return arte;

        return TentarCarregar(Path.Combine("Artes", "Inimigos", "ratoselvagem.xp"))
               ?? new ScreenSurface(1, 1);
    }

    /// <summary>Plano de fundo de um cenário (local do mapa ou tela de batalha).
    /// Devolve null quando não dá pra carregar - quem chama desenha sem fundo.</summary>
    public static ScreenSurface? CarregarArteCenario(string caminhoXP) => TentarCarregar(caminhoXP);

    private static ScreenSurface? TentarCarregar(string caminhoXP)
    {
        try
        {
            string? encontrado = Resolver(caminhoXP);
            if (encontrado is null) return null;

            using Stream stream = File.OpenRead(encontrado);
            REXPaintImage imagem = REXPaintImage.Load(stream);
            ICellSurface[] camadas = imagem.ToCellSurface();

            return camadas.Length == 0 ? null : new ScreenSurface(camadas[0]);
        }
        catch
        {
            // .xp corrompido ou versão de formato que o SadRex não entende.
            return null;
        }
    }

    /// <summary>Devolve o primeiro caminho que existe de verdade, ou null.</summary>
    private static string? Resolver(string caminhoRelativo)
    {
        foreach (string candidato in Candidatos(caminhoRelativo))
            if (File.Exists(candidato))
                return candidato;

        return null;
    }

    private static IEnumerable<string> Candidatos(string caminho)
    {
        string pasta = Path.GetDirectoryName(caminho) ?? string.Empty;
        string arquivo = Path.GetFileName(caminho);

        string[] nomes =
        {
            arquivo,
            arquivo.ToLowerInvariant(),
            arquivo.Length > 0 ? char.ToUpperInvariant(arquivo[0]) + arquivo[1..] : arquivo,
        };

        // Primeiro relativo ao diretório de trabalho, depois ao do executável.
        string[] raizes = { string.Empty, AppContext.BaseDirectory };

        foreach (string raiz in raizes)
            foreach (string nome in nomes)
                yield return raiz.Length == 0
                    ? Path.Combine(pasta, nome)
                    : Path.Combine(raiz, pasta, nome);
    }
}
