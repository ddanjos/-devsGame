using System;
using SadConsole;
using SadConsole.Configuration;
using SadRogue.Primitives;
using SurvivorGame.Cenarios;
using SurvivorGame.Combate;
using SurvivorGame.Mapa;
using SurvivorGame.Regras;
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
            // Precisamos do tamanho do mapa ANTES de Game.Create - SadConsole fixa o
            // tamanho da janela em SetScreenSize e não redimensiona depois. Como
            // MapaCidadeBlumenau é sempre igual (mesmo layout todo restart), criar uma
            // instância só pra medir é seguro e barato.
            var tamanhoInicial = new MapaCidadeBlumenau();

            Builder startup = new Builder()
                .SetScreenSize(tamanhoInicial.Largura, tamanhoInicial.Altura)
                .OnStart(Game_Started);

            Game.Create(startup);
            Game.Instance.Run();
            Game.Instance.Dispose();
        }

        private static void Game_Started(object? sender, GameHost host)
        {
            IniciarNovoJogo();
        }

        /// <summary>
        /// Monta (ou remonta) um jogo do zero: novo mapa, novo personagem, inimigos
        /// recriados e progresso de missão zerado. Usado tanto no início quanto ao
        /// escolher "Tentar Novamente" na tela de Game Over.
        /// </summary>
        private static void IniciarNovoJogo()
        {
            GerenciadorJogo.Reiniciar();

            _terreno = new MapaCidadeBlumenau();
            _itensNoChao = new MapaJogo();
            _inimigosNoMapa = new MapaInimigos();

            Point entrada = _terreno.PontoEntrada;
            _personagem = new Personagem("Sobrevivente", entrada.X, entrada.Y);

            CriarInimigosDoMapa();

            var mapaScreen = new MapaScreen(_terreno, _itensNoChao, _inimigosNoMapa, _personagem, IniciarNovoJogo, SairDoJogo);

            Game.Instance.Screen = mapaScreen;
            Game.Instance.Screen.IsFocused = true;
        }

        private static void SairDoJogo()
        {
            Game.Instance.Dispose();
            Environment.Exit(0);
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

            var zumbi = new Inimigo("Zumbi", vidaMaxima: 60, habilidades: new[]
            {
                new Habilidade("Mordida Infectada", dano: 12),
                new Habilidade("Golpe Cambaleante", dano: 6)
            });

            ScreenSurface arteZumbi = ArteUtils.CarregarArteInimigo("Artes/Inimigos/zumbi.xp");
            _inimigosNoMapa!.AdicionarInimigo(new InimigoNoMapa(entrada.X - 3, entrada.Y + 5, 'z', Color.LightGreen, zumbi, arteZumbi));
        }
    }
}
