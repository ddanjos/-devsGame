using System.Linq;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using SurvivorGame.Inventario;
using SurvivorGame.Mapa;
using SurvivorGame.Regras;

namespace SurvivorGame.UI
{
    /// <summary>
    /// Tela de inventário: navega a mochila do jogador (setas + Enter), mostra o
    /// que está equipado num painel à direita, e permite Usar/Equipar, Desequipar
    /// ou Largar no Chão o item selecionado. Mesmo padrão das outras telas
    /// (ScreenSurface + ProcessKeyboard) - ver CombateScreen/CenarioLocalScreen.
    /// </summary>
    internal class InventarioScreen : ScreenSurface
    {
        private enum ModoInventario { SelecionandoItem, SubMenuAcoes }

        private readonly Personagem _jogador;
        private readonly IScreenObject _telaAnterior;

        /// <summary>Opcional: se informado, "Largar no Chão" cria o item de verdade no mapa
        /// (na posição atual do jogador). Se null, o item só é removido do inventário.</summary>
        private readonly MapaJogo? _mapaJogo;

        private ModoInventario _modo = ModoInventario.SelecionandoItem;
        private int _indiceItem;
        private int _indiceAcao;

        private readonly string[] _opcoesAcao = { "Usar / Equipar", "Desequipar", "Largar no Chão", "Voltar" };

        public InventarioScreen(Personagem jogador, IScreenObject telaAnterior, int largura, int altura, MapaJogo? mapaJogo = null)
            : base(largura, altura)
        {
            _jogador = jogador;
            _telaAnterior = telaAnterior;
            _mapaJogo = mapaJogo;

            UseKeyboard = true;
            IsFocused = true;

            Redesenhar();
        }

        public override bool ProcessKeyboard(Keyboard keyboard)
        {
            bool subiu = keyboard.IsKeyPressed(Keys.Up);
            bool desceu = keyboard.IsKeyPressed(Keys.Down);
            bool confirmou = keyboard.IsKeyPressed(Keys.Enter);
            bool voltou = keyboard.IsKeyPressed(Keys.Escape) || keyboard.IsKeyPressed(Keys.I);

            var itens = _jogador.Inventario.Itens;

            switch (_modo)
            {
                case ModoInventario.SelecionandoItem:
                    if (voltou)
                    {
                        Game.Instance.Screen = _telaAnterior;
                        Game.Instance.Screen!.IsFocused = true;
                        return true;
                    }

                    if (itens.Count == 0) break;

                    if (desceu) _indiceItem = (_indiceItem + 1) % itens.Count;
                    else if (subiu) _indiceItem = (_indiceItem - 1 + itens.Count) % itens.Count;
                    else if (confirmou)
                    {
                        _modo = ModoInventario.SubMenuAcoes;
                        _indiceAcao = 0;
                    }
                    break;

                case ModoInventario.SubMenuAcoes:
                    if (voltou)
                    {
                        _modo = ModoInventario.SelecionandoItem;
                    }
                    else if (desceu) _indiceAcao = (_indiceAcao + 1) % _opcoesAcao.Length;
                    else if (subiu) _indiceAcao = (_indiceAcao - 1 + _opcoesAcao.Length) % _opcoesAcao.Length;
                    else if (confirmou)
                    {
                        ExecutarAcaoMenu(itens[_indiceItem]);
                        return true; // ExecutarAcaoMenu já redesenha
                    }
                    break;
            }

            Redesenhar();
            return true;
        }

        private void ExecutarAcaoMenu(ItemInventario item)
        {
            switch (_indiceAcao)
            {
                case 0: // Usar / Equipar
                    if (item is Consumivel consumivel)
                    {
                        _jogador.Curar(consumivel.Cura);
                        _jogador.Inventario.RemoverItem(item, 1);
                    }
                    else if (item is Arma or Armadura)
                    {
                        _jogador.Equipar(item);
                    }
                    break;

                case 1: // Desequipar
                    if (item is Arma or Armadura)
                    {
                        _jogador.Desequipar(item);
                    }
                    break;

                case 2: // Largar no Chão
                    // Se o item largado estava equipado, desequipa primeiro.
                    _jogador.Desequipar(item);

                    if (_mapaJogo is not null)
                        AcoesJogador.DroparItem(_jogador, _mapaJogo, item.Nome, 1);
                    else
                        _jogador.Inventario.RemoverItem(item, 1);
                    break;

                case 3: // Voltar
                    break;
            }

            _modo = ModoInventario.SelecionandoItem;
            if (_indiceItem >= _jogador.Inventario.Itens.Count)
                _indiceItem = System.Math.Max(0, _jogador.Inventario.Itens.Count - 1);

            Redesenhar();
        }

        private void Redesenhar()
        {
            Surface.Clear();
            Surface.Print(2, 1, "INVENTÁRIO", Color.Gold, Color.Black);

            var itens = _jogador.Inventario.Itens;

            if (itens.Count == 0)
            {
                Surface.Print(2, 3, "(Mochila Vazia)", Color.Gray, Color.Black);
            }
            else
            {
                for (int i = 0; i < itens.Count; i++)
                {
                    bool selecionado = i == _indiceItem && _modo == ModoInventario.SelecionandoItem;
                    string prefixo = selecionado ? "> " : "  ";
                    string tagEquipado = _jogador.Equipamentos.Contains(itens[i]) ? " [EQUIPADO]" : "";
                    Color cor = selecionado ? Color.Yellow : Color.White;
                    Surface.Print(2, 3 + i, $"{prefixo}{itens[i].Nome} x{itens[i].Quantidade}{tagEquipado}", cor, Color.Black);
                }
            }

            // Painel de equipamentos, à direita
            int colunaDireita = Width / 2 + 4;
            Surface.Print(colunaDireita, 1, "EQUIPADO", Color.Gold, Color.Black);
            Surface.Print(colunaDireita, 3,
                _jogador.ArmaEquipada is not null ? $"Arma: {_jogador.ArmaEquipada.Nome} (+{_jogador.ArmaEquipada.Dano} dano)" : "Arma: (nenhuma)",
                Color.White, Color.Black);
            Surface.Print(colunaDireita, 4,
                _jogador.ArmaduraEquipada is not null ? $"Armadura: {_jogador.ArmaduraEquipada.Nome} (+{_jogador.ArmaduraEquipada.Defesa} defesa)" : "Armadura: (nenhuma)",
                Color.White, Color.Black);

            // Submenu de ações do item selecionado
            if (_modo == ModoInventario.SubMenuAcoes && itens.Count > 0)
            {
                int yMenu = Height - 7;
                Surface.Print(2, yMenu - 1, $"-- {itens[_indiceItem].Nome} --", Color.Cyan, Color.Black);
                for (int i = 0; i < _opcoesAcao.Length; i++)
                {
                    bool selecionado = i == _indiceAcao;
                    string prefixo = selecionado ? "> " : "  ";
                    Color cor = selecionado ? Color.Yellow : Color.White;
                    Surface.Print(2, yMenu + i, prefixo + _opcoesAcao[i], cor, Color.Black);
                }
            }

            Surface.Print(2, Height - 1, "Setas: navega | Enter: confirma | ESC/I: fecha", Color.Gray, Color.Black);
        }
    }
}
