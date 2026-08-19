using System;
using System.Linq;
using SadConsole;
using SadConsole.Configuration;
using SadConsole.Input;
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
        private static ScreenSurface? _mapaTela;

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

            // 4. Jogador
            _mapaTela.Surface.SetGlyph(_personagem.X, _personagem.Y, '@', Color.LimeGreen, Color.Black);

            _mapaTela.UseMouse = true;
            _mapaTela.MouseButtonClicked += MapaTela_MouseButtonClicked;

            Game.Instance.Screen = _mapaTela;
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

        private static void MapaTela_MouseButtonClicked(object? sender, MouseScreenObjectState state)
        {
            if (_inimigosNoMapa is null || _personagem is null || _mapaTela is null || _terreno is null)
                return;

            // Obtém a coordenada do clique no grid do SadConsole
            Point celulaClicada = state.CellPosition;

            InimigoNoMapa? inimigoClicado = _inimigosNoMapa.ObterInimigoNaPosicao(celulaClicada);
            if (inimigoClicado is not null)
            {
                var combate = new CombateScreen(
                    _personagem,
                    inimigoClicado,
                    _inimigosNoMapa,
                    _mapaTela,
                    _terreno.Largura,
                    _terreno.Altura,
                    RedesenharMapaCompleto
                );

                Game.Instance.Screen = combate;
                Game.Instance.Screen.IsFocused = true;
                return;
            }

            LocalMapa? local = MapaCidadeBlumenau.Locais
                .FirstOrDefault(l => l.Posicao == celulaClicada);

            if (local is not null)
            {
                var cenario = new CenarioLocalScreen(local, _mapaTela, _terreno.Largura, _terreno.Altura);
                Game.Instance.Screen = cenario;
                Game.Instance.Screen.IsFocused = true;
            }
        }

        private static void RedesenharMapaCompleto()
        {
            if (_mapaTela is null || _terreno is null || _itensNoChao is null || _inimigosNoMapa is null || _personagem is null)
                return;

            _terreno.DesenharEm(_mapaTela);

            foreach (var item in _itensNoChao.ItensNoChao)
                _mapaTela.Surface.SetGlyph(item.X, item.Y, item.Simbolo, Color.White, Color.Black);

            foreach (var inimigo in _inimigosNoMapa.Inimigos)
                _mapaTela.Surface.SetGlyph(inimigo.X, inimigo.Y, inimigo.Simbolo, inimigo.Cor, Color.Black);

            _mapaTela.Surface.SetGlyph(_personagem.X, _personagem.Y, '@', Color.LimeGreen, Color.Black);
        }
    }
}