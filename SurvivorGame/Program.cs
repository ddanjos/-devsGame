using SadConsole;
using SadConsole.Configuration;
using SadRogue.Primitives;
using SurvivorGame.Mapa;

namespace SurvivorGame;

public class Program
{
    // O TERRENO (paredes, ruas, rio...) e os ITENS NO CHÃO são dois sistemas
    // separados de propósito: _terreno é um IMapa (Strategy - masmorra ou cidade,
    // tanto faz), _itensNoChao é o MapaJogo de vocês (registro de ItemNoMapa por
    // posição). Program.cs é quem junta os dois; nenhum dos dois precisa saber
    // que o outro existe.
    private static IMapa? _terreno;
    private static MapaJogo? _itensNoChao;
    private static Personagem? _personagem;

    public static void Main(string[] args)
    {
        _terreno = new MapaCidadeBlumenau();
        // _terreno = new MapaMasmorra(); // <- descomentem pra trocar pra masmorra

        _itensNoChao = new MapaJogo();

        // O personagem nasce no ponto de entrada do mapa ativo.
        // No MapaCidadeBlumenau isso é o ProWay (R. Sete de Setembro, 1600 - Centro).
        Point entrada = _terreno.PontoEntrada;
        _personagem = new Personagem("Sobrevivente", entrada.X, entrada.Y);

        Builder startup = new Builder()
            .SetScreenSize(_terreno.Largura, _terreno.Altura)
            .OnStart(Game_Started)
            .IsStartingScreenFocused(true);

        Game.Create(startup);
        Game.Instance.Run();
        Game.Instance.Dispose();
    }

    private static void Game_Started(object? sender, GameHost host)
    {
        var superficie = new ScreenSurface(_terreno!.Largura, _terreno.Altura);
        _terreno.DesenharEm(superficie);

        // Itens largados no chão (MapaJogo) são desenhados por cima do terreno.
        foreach (var item in _itensNoChao!.ItensNoChao)
        {
            superficie.Surface.SetGlyph(item.X, item.Y, item.Simbolo, Color.White, Color.Black);
        }

        // Personagem por cima de tudo.
        superficie.Surface.SetGlyph(_personagem!.X, _personagem.Y, '@', Color.LimeGreen, Color.Black);

        Game.Instance.Screen = superficie;
        Game.Instance.Screen.IsFocused = true;
    }
}
