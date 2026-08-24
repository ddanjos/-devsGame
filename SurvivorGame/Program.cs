using SadConsole;
using SadConsole.Configuration;
using SadRogue.Primitives;
using SurvivorGame.Cenarios;
using SurvivorGame.Mapa;
using SurvivorGame.Ui;

namespace SurvivorGame
{
    public class Program
    {
        private static IMapa? _terreno;
        private static MapaJogo? _itensNoChao;
        private static MapaInimigos? _inimigosNoMapa;
        private static Personagem? _personagem;

        private static int _larguraJanela;
        private static int _alturaJanela;

        public static void Main(string[] args)
        {
            _terreno = new MapaCidadeBlumenau();

            // A janela precisa caber o maior cenário do jogo, não só a cidade: os
            // mapas de interior desenhados pelo Lindomar em REXPaint são 60x60
            // (Artes/Cenarios/*.xp), mais altos que os 45 da cidade. Sem isso, o
            // andar 0 (que tem a saída lá embaixo, perto da linha 59) ficaria
            // cortado fora da janela.
            _larguraJanela = Math.Max(_terreno.Largura, 60);
            _alturaJanela = Math.Max(_terreno.Altura, 60);

            Builder startup = new Builder()
                .SetScreenSize(_larguraJanela, _alturaJanela)
                .OnStart(Game_Started);
            System.Console.OutputEncoding = System.Text.Encoding.UTF8;
            Game.Create(startup);
            Game.Instance.Run();
            Game.Instance.Dispose();
        }

        private static void Game_Started(object? sender, GameHost host)
        {
            MostrarMenuPrincipal();
        }

        public static void MostrarMenuPrincipal()
        {
            var menuScreen = new MenuPrincipalScreen(_larguraJanela, _alturaJanela);
            Game.Instance.Screen = menuScreen;
            Game.Instance.Screen.IsFocused = true;
        }

        public static void IniciarNovaPartida()
        {
            // Partida nova: zera o progresso da missão (as 3 peças do rádio) e o
            // estado guardado dos locais. Sem isso, rodar o jogo de novo na mesma
            // execução herdaria as peças da partida anterior.
            global::SurvivorGame.Regras.GerenciadorJogo.Reiniciar();
            FabricaLocais.Reiniciar();

            _terreno = new MapaCidadeBlumenau();
            _itensNoChao = new MapaJogo();
            _inimigosNoMapa = new MapaInimigos();

            Point entrada = _terreno.PontoEntrada;
            _personagem = new Personagem("Sobrevivente", entrada.X, entrada.Y);

            if (_terreno is null || _personagem is null || _itensNoChao is null || _inimigosNoMapa is null)
                return;

            var mapaScreen = new MapaScreen(_terreno, _itensNoChao, _inimigosNoMapa, _personagem);
            Game.Instance.Screen = mapaScreen;
            Game.Instance.Screen.IsFocused = true;
        }

        // REMOVIDO: antes plantávamos um "Rato Selvagem" decorativo a três células
        // do ponto de partida, herdado de quando o mapa da cidade era o único lugar
        // com combate. Ele quebrava a missão principal: GerenciadorJogo concede a
        // Antena a QUALQUER vitória contra um inimigo com "Rato" no nome, então
        // derrotar esse rato da rua entregava a peça e deixava o conteúdo do andar 0
        // (o ninho atrás do balcão, ver Mapa/LocalAndarZero) sem função nenhuma -
        // ele passava a responder "o ninho está vazio agora" antes do jogador ter
        // ido lá. Os encontros agora acontecem dentro dos locais, pelas ações.
    }
}
