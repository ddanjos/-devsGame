using System.Linq;
using SadConsole;
using SadConsole.Configuration;
using SadConsole.Input;
using SadRogue.Primitives;
using SurvivorGame.Cenarios;
using SurvivorGame.Combate;
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
    private static ScreenSurface? _mapaTela;

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
        _mapaTela = new ScreenSurface(_terreno!.Largura, _terreno.Altura);
        _terreno.DesenharEm(_mapaTela);

        // Itens largados no chão (MapaJogo) são desenhados por cima do terreno.
        foreach (var item in _itensNoChao!.ItensNoChao)
        {
            _mapaTela.Surface.SetGlyph(item.X, item.Y, item.Simbolo, Color.White, Color.Black);
        }

        // Personagem por cima de tudo.
        _mapaTela.Surface.SetGlyph(_personagem!.X, _personagem.Y, '@', Color.LimeGreen, Color.Black);

        // Clique num lugar do mapa -> troca a tela pro cenário daquele lugar.
        _mapaTela.UseMouse = true;
        _mapaTela.MouseButtonClicked += MapaTela_MouseButtonClicked;

        Game.Instance.Screen = _mapaTela;
        Game.Instance.Screen.IsFocused = true;

        // ---- TESTE DO COMBATE ----
        // Ainda não existe um "gatilho" de combate no mapa (encontro aleatório,
        // clicar num inimigo, etc). Pra testar o sistema agora, descomentem as
        // linhas abaixo - abre direto numa batalha contra um inimigo de exemplo.
        //
        // var inimigoTeste = new Inimigo("Rato Selvagem", vidaMaxima: 40, habilidades: new[]
        // {
        //     new Habilidade("Mordida", dano: 8),
        //     new Habilidade("Arranhão", dano: 5)
        // });
        // Game.Instance.Screen = new CombateScreen(_personagem, inimigoTeste, _mapaTela, _terreno.Largura, _terreno.Altura);
        // Game.Instance.Screen.IsFocused = true;
    }

    private static void MapaTela_MouseButtonClicked(object? sender, MouseScreenObjectState state)
    {
        // NOTA: se "CellPosition" não existir na versão de vocês, o autocomplete do
        // Rider deve sugerir o nome certo (algo como "SurfaceCellPosition") - é só
        // trocar aqui, o resto do código não muda.
        Point celulaClicada = state.CellPosition;

        LocalMapa? local = MapaCidadeBlumenau.Locais
            .FirstOrDefault(l => l.Posicao == celulaClicada);

        if (local is null)
            return;

        var cenario = new CenarioLocalScreen(local, _mapaTela!, _terreno!.Largura, _terreno.Altura);
        Game.Instance.Screen = cenario;
        Game.Instance.Screen.IsFocused = true;
    }
}
