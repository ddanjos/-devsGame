using System;
using SadConsole;
using SadConsole.Configuration;
using SadRogue.Primitives;
using SurvivorGame.Cenarios;
using SurvivorGame.Combate;
using SurvivorGame.Mapa;
using SurvivorGame.Utilitarios;

namespace SurvivorGame
{
    public class Program
    {
        private static IMapa? _terreno;
        private static MapaJogo? _itensNoChao;
        private static MapaInimigos? _inimigosNoMapa;
        private static Personagem? _personagem;

        public static void Main(string[] args)
        {
            _terreno = new MapaCidadeBlumenau();
            _itensNoChao = new MapaJogo();
            _inimigosNoMapa = new MapaInimigos();

            Point entrada = _terreno.PontoEntrada;
            _personagem = new Personagem("Sobrevivente", entrada.X, entrada.Y);

            Builder startup = new Builder()
                .SetScreenSize(_terreno.Largura, _terreno.Altura)
                .OnStart(Game_Started);

            Game.Create(startup);
            Game.Instance.Run();
            Game.Instance.Dispose();
        }

        private static void Game_Started(object? sender, GameHost host)
        {
            if (_terreno is null || _personagem is null || _itensNoChao is null || _inimigosNoMapa is null)
                return;

            CriarInimigosDoMapa();

            var mapaScreen = new MapaScreen(_terreno, _itensNoChao, _inimigosNoMapa, _personagem);

            Game.Instance.Screen = mapaScreen;
            Game.Instance.Screen.IsFocused = true;
        }

        private static void CriarInimigosDoMapa()
        {
            Point entrada = _terreno!.PontoEntrada;

            var rato = new Inimigo("Rato Selvagem", vidaMaxima: 40, habilidades: new[]
            {
                new Habilidade("Mordida", dano: 8),
                new Habilidade("Arranhão", dano: 5)
            });

            ScreenSurface arteRato = ArteUtils.CarregarArteInimigo("Artes/Inimigos/ratoselvagem.xp");

            _inimigosNoMapa!.AdicionarInimigo(new InimigoNoMapa(entrada.X + 3, entrada.Y + 2, 'r', Color.Red, rato, arteRato));
        }
    }
}
