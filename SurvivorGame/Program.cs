using SadConsole;
using SadConsole.Configuration;
using SadRogue.Primitives;
using SurvivorGame.Cenarios;
using SurvivorGame.Mapa;
using SurvivorGame.Ui;
using SurvivorGame.Audio;
using SurvivorGame.Utilitarios;

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

        public static int LarguraJanela => _larguraJanela;
        public static int AlturaJanela => _alturaJanela;

        public static void Main(string[] args)
        {
            _terreno = new MapaCidadeBlumenau();

            _larguraJanela = Math.Max(_terreno.Largura, 60);
            _alturaJanela = Math.Max(_terreno.Altura, 60);

            
            Builder startup = new Builder()
                .SetWindowSizeInPixels(
                    _larguraJanela * AjusteVisual.TamanhoCelula.X,
                    _alturaJanela * AjusteVisual.TamanhoCelula.Y)
                .OnStart(Game_Started);
            System.Console.OutputEncoding = System.Text.Encoding.UTF8;
            Game.Create(startup);
            Game.Instance.Run();
            Game.Instance.Dispose();
        }

        private static void Game_Started(object? sender, GameHost host)
        {
            
            SurvivorGame.Regras.Configuracao.Carregar();
            GerenciadorSom.Iniciar();
            GerenciadorSom.TocarTrilha(Trilha.Exploracao);

            MostrarMenuPrincipal();
        }

        public static void MostrarMenuPrincipal()
        {
            var menuScreen = new MenuPrincipalScreen(_larguraJanela, _alturaJanela);
            Game.Instance.Screen = menuScreen;
            Game.Instance.Screen.IsFocused = true;
        }

      
        public static bool CarregarPartida()
        {
           
            global::SurvivorGame.Regras.GerenciadorJogo.Reiniciar();
            FabricaLocais.Reiniciar();

            Personagem? salvo = SurvivorGame.Regras.SaveJogo.Carregar();
            if (salvo is null) return false;

         
            _terreno = new MapaCidadeBlumenau();
            _itensNoChao = new MapaJogo();
            _inimigosNoMapa = new MapaInimigos();

          
            _personagem = salvo;
            AbrirMapa();
            return true;
        }

        public static void IniciarNovaPartida()
        {
        
            global::SurvivorGame.Regras.GerenciadorJogo.Reiniciar();
            FabricaLocais.Reiniciar();

            _terreno = new MapaCidadeBlumenau();
            _itensNoChao = new MapaJogo();
            _inimigosNoMapa = new MapaInimigos();

            Point entrada = _terreno.PontoEntrada;
            _personagem = new Personagem("Sobrevivente", entrada.X, entrada.Y);

            if (_terreno is null || _personagem is null || _itensNoChao is null || _inimigosNoMapa is null)
                return;

            AbrirMapa();
        }

      
        private static void AbrirMapa()
        {
            _terreno ??= new MapaCidadeBlumenau();
            _itensNoChao ??= new MapaJogo();
            _inimigosNoMapa ??= new MapaInimigos();

            if (_personagem is null) return;

            GerenciadorSom.TocarTrilha(Trilha.Exploracao);

            var mapaScreen = new MapaScreen(_terreno, _itensNoChao, _inimigosNoMapa, _personagem);
            Game.Instance.Screen = mapaScreen;
            Game.Instance.Screen.IsFocused = true;
        }

    }
}
