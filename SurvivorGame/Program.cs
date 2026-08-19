using SadConsole;
using SadConsole.Configuration;
using SadConsole.Input;
using SadRogue.Primitives;
using SurvivorGame.Cenarios;
using SurvivorGame.Combate;
using SurvivorGame.Mapa;
using SurvivorGame.Utilitarios;
using System.Linq;

namespace SurvivorGame;

public class Program
{
    private static IMapa? _terreno;
    private static MapaJogo? _itensNoChao;
    private static MapaInimigos? _inimigosNoMapa;
    private static Personagem? _personagem;
    private static ScreenSurface? _mapaTela;

    public static void Main(string[] args)
    {
     
        // 1. Inicializa todos os dados do jogo PRIMEIRO
        _terreno = new MapaCidadeBlumenau();
        
        _itensNoChao = new MapaJogo();
        _inimigosNoMapa = new MapaInimigos();
       
        Point entrada = _terreno.PontoEntrada;
        _personagem = new Personagem("Sobrevivente", entrada.X, entrada.Y);
        CriarInimigosDoMapa();
        // 3. Configura a janela e o ciclo de vida do SadConsole
        Builder startup = new Builder()
            .SetScreenSize(_terreno.Largura, _terreno.Altura)
            .OnStart(Game_Started);

        Game.Create(startup);
        Game.Instance.Run();
        Game.Instance.Dispose();
    }


    private static void CriarInimigosDoMapa()
    {
        Point entrada = _terreno!.PontoEntrada;

        var rato = new Inimigo("Rato Selvagem", vidaMaxima: 40, habilidades: new[]
        {
            new Habilidade("Mordida", dano: 8),
            new Habilidade("Arranhão", dano: 5)
        });

        // Carregando o arquivo .xp de forma segura após o SadConsole estar pronto
        ScreenSurface arteRato = ArteUtils.CarregarArteInimigo("Artes/Inimigos/ratoselvagem.xp");

        _inimigosNoMapa!.AdicionarInimigo(new InimigoNoMapa(entrada.X + 3, entrada.Y + 2, 'r', Color.Red, rato, arteRato));
    }



    private static void Game_Started(object? sender, GameHost host)
    {
        // Garantia contra Nulo
        if (_terreno is null || _personagem is null || _itensNoChao is null || _inimigosNoMapa is null)
            return;

        _mapaTela = new ScreenSurface(_terreno.Largura, _terreno.Altura);

        // 1. Terreno
        _terreno.DesenharEm(_mapaTela);

        // 2. Itens no chão
        foreach (var item in _itensNoChao.ItensNoChao)
        {
            _mapaTela.Surface.SetGlyph(item.X, item.Y, item.Simbolo, Color.White, Color.Black);
        }

        // 3. Inimigos no mapa
        foreach (var inimigo in _inimigosNoMapa.Inimigos)
        {
            _mapaTela.Surface.SetGlyph(inimigo.X, inimigo.Y, inimigo.Simbolo, inimigo.Cor, Color.Black);
        }

        // 4. Personagem
        _mapaTela.Surface.SetGlyph(_personagem.X, _personagem.Y, '@', Color.LimeGreen, Color.Black);

        // Configuração do Mouse
        _mapaTela.UseMouse = true;
        _mapaTela.MouseButtonClicked += MapaTela_MouseButtonClicked;

        Game.Instance.Screen = _mapaTela;
        Game.Instance.Screen.IsFocused = true;
    }

    private static void MapaTela_MouseButtonClicked(object? sender, MouseScreenObjectState state)
    {
        if (_inimigosNoMapa is null || _personagem is null || _mapaTela is null || _terreno is null)
            return;

        Point celulaClicada = state.CellPosition;

        // 1. Clicou no inimigo?
        InimigoNoMapa? inimigoClicado = _inimigosNoMapa.ObterInimigoNaPosicao(celulaClicada);
        if (inimigoClicado is not null)
        {
            var combate = new CombateScreen(_personagem, inimigoClicado.DadosCombate, _mapaTela, _terreno.Largura, _terreno.Altura);
            Game.Instance.Screen = combate;
            Game.Instance.Screen.IsFocused = true;
            return;
        }

        // 2. Clicou em um local de cenário?
        LocalMapa? local = MapaCidadeBlumenau.Locais
            .FirstOrDefault(l => l.Posicao == celulaClicada);

        if (local is not null)
        {
            var cenario = new CenarioLocalScreen(local, _mapaTela, _terreno.Largura, _terreno.Altura);
            Game.Instance.Screen = cenario;
            Game.Instance.Screen.IsFocused = true;
        }
    }
}