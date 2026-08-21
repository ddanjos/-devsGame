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
            // Partida nova: zera o progresso da missão (as 3 peças do rádio) e o
            // estado guardado dos locais. Sem isso, rodar o jogo de novo na mesma
            // execução herdaria as peças da partida anterior.
            SurvivorGame.Regras.GerenciadorJogo.Reiniciar();
            FabricaLocais.Reiniciar();

            _terreno = new MapaCidadeBlumenau();
            _itensNoChao = new MapaJogo();
            _inimigosNoMapa = new MapaInimigos();

            Point entrada = _terreno.PontoEntrada;
            _personagem = new Personagem("Sobrevivente", entrada.X, entrada.Y);

            // A janela precisa caber o maior cenário do jogo, não só a cidade: os
            // mapas de interior desenhados pelo Lindomar em REXPaint são 60x60
            // (Artes/Cenarios/*.xp), mais altos que os 45 da cidade. Sem isso, o
            // andar 0 (que tem a saída lá embaixo, perto da linha 59) ficaria
            // cortado fora da janela.
            int larguraJanela = Math.Max(_terreno.Largura, 60);
            int alturaJanela = Math.Max(_terreno.Altura, 60);

            Builder startup = new Builder()
                .SetScreenSize(larguraJanela, alturaJanela)
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

            // Vem da FabricaInimigos pra os números do rato ficarem num lugar só
            // (antes estavam duplicados aqui e dentro do andar 0).
            Inimigo rato = FabricaInimigos.CriarRatoSelvagem();
            ScreenSurface arteRato = FabricaInimigos.CarregarArteRato();

            _inimigosNoMapa!.AdicionarInimigo(new InimigoNoMapa(entrada.X + 3, entrada.Y + 2, 'r', Color.Red, rato, arteRato));
        }
    }
}
