using System.Linq;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using SurvivorGame.Combate;
using SurvivorGame.Mapa;
using SurvivorGame.UI;

namespace SurvivorGame.Cenarios
{
    /// <summary>
    /// Tela principal do overworld (a cidade). Antes essa lógica vivia solta em
    /// Program.cs como uma ScreenSurface "crua"; virou uma classe própria (mesmo
    /// padrão de CombateScreen/CenarioLocalScreen) porque precisávamos de
    /// ProcessKeyboard pra abrir o inventário com a tecla 'I' - uma ScreenSurface
    /// sem subclasse não tem como reagir a uma tecla específica.
    /// </summary>
    internal class MapaScreen : ScreenSurface
    {
        private readonly IMapa _terreno;
        private readonly MapaJogo _itensNoChao;
        private readonly MapaInimigos _inimigosNoMapa;
        private readonly Personagem _personagem;

        public MapaScreen(IMapa terreno, MapaJogo itensNoChao, MapaInimigos inimigosNoMapa, Personagem personagem)
            : base(terreno.Largura, terreno.Altura)
        {
            _terreno = terreno;
            _itensNoChao = itensNoChao;
            _inimigosNoMapa = inimigosNoMapa;
            _personagem = personagem;

            UseMouse = true;
            UseKeyboard = true;
            MouseButtonClicked += MapaTela_MouseButtonClicked;

            RedesenharMapaCompleto();
        }

        public override bool ProcessKeyboard(Keyboard keyboard)
        {
            if (keyboard.IsKeyPressed(Keys.I))
            {
                Game.Instance.Screen = new InventarioScreen(_personagem, this, Width, Height, _itensNoChao);
                Game.Instance.Screen.IsFocused = true;
                return true;
            }

            return base.ProcessKeyboard(keyboard);
        }

        private void MapaTela_MouseButtonClicked(object? sender, MouseScreenObjectState state)
        {
            Point celulaClicada = state.CellPosition;

            InimigoNoMapa? inimigoClicado = _inimigosNoMapa.ObterInimigoNaPosicao(celulaClicada);
            if (inimigoClicado is not null)
            {
                var combate = new CombateScreen(
                    _personagem,
                    inimigoClicado,
                    _inimigosNoMapa,
                    this,
                    Width,
                    Height,
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
                var cenario = new CenarioLocalScreen(local, this, Width, Height);
                Game.Instance.Screen = cenario;
                Game.Instance.Screen.IsFocused = true;
            }
        }

        /// <summary>Redesenha terreno + itens no chão + inimigos + jogador, nessa ordem
        /// (cada camada por cima da anterior). Passada como callback pro CombateScreen
        /// chamar ao voltar do combate (pra sumir com o sprite do inimigo derrotado).</summary>
        public void RedesenharMapaCompleto()
        {
            _terreno.DesenharEm(this);

            foreach (var item in _itensNoChao.ItensNoChao)
                Surface.SetGlyph(item.X, item.Y, item.Simbolo, Color.White, Color.Black);

            foreach (var inimigo in _inimigosNoMapa.Inimigos)
                Surface.SetGlyph(inimigo.X, inimigo.Y, inimigo.Simbolo, inimigo.Cor, Color.Black);

            Surface.SetGlyph(_personagem.X, _personagem.Y, '@', Color.LimeGreen, Color.Black);
        }
    }
}
