using System;
using System.Collections.Generic;
using System.Linq;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using SurvivorGame.Inventario;
using SurvivorGame.Mapa;
using SurvivorGame.Regras;
using SurvivorGame.Utilitarios;

namespace SurvivorGame.UI
{
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
            if (keyboard.IsKeyPressed(Keys.Escape) || keyboard.IsKeyPressed(Keys.I))
            {
                Game.Instance.Screen = _telaAnterior;
                Game.Instance.Screen.IsFocused = true;
                return true;
            }

            var itens = _jogador.Inventario.Itens;

            if (_modo == ModoInventario.SelecionandoItem)
            {
                if (itens.Count == 0) return true;

                if (keyboard.IsKeyPressed(Keys.Down))
                {
                    _indiceItem = (_indiceItem + 1) % itens.Count;
                    Redesenhar();
                }
                else if (keyboard.IsKeyPressed(Keys.Up))
                {
                    _indiceItem = (_indiceItem - 1 + itens.Count) % itens.Count;
                    Redesenhar();
                }
                else if (keyboard.IsKeyPressed(Keys.Enter))
                {
                    _modo = ModoInventario.SubMenuAcoes;
                    _indiceAcao = 0;
                    Redesenhar();
                }
            }
            else if (_modo == ModoInventario.SubMenuAcoes)
            {
                if (keyboard.IsKeyPressed(Keys.Down))
                {
                    _indiceAcao = (_indiceAcao + 1) % _opcoesAcao.Length;
                    Redesenhar();
                }
                else if (keyboard.IsKeyPressed(Keys.Up))
                {
                    _indiceAcao = (_indiceAcao - 1 + _opcoesAcao.Length) % _opcoesAcao.Length;
                    Redesenhar();
                }
                else if (keyboard.IsKeyPressed(Keys.Enter))
                {
                    ExecutarAcaoMenu(itens[_indiceItem]);
                }
            }

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
                _indiceItem = Math.Max(0, _jogador.Inventario.Itens.Count - 1);

            Redesenhar();
        }

        private void Redesenhar()
        {
            Surface.Clear();

            // Cabeçalho
            Surface.Print(2, 1, "=== INVENTÁRIO DO SOBREVIVENTE ===", Color.Yellow, Color.Black);
            Surface.Print(2, 2, $"HP: {_jogador.Vida}/{_jogador.VidaMaxima}", Color.LimeGreen, Color.Black);

            // Coluna Esquerda: Itens da Mochila
            Surface.Print(2, 4, "--- Mochila ---", Color.Cyan, Color.Black);
            var itens = _jogador.Inventario.Itens;

            if (itens.Count == 0)
            {
                Surface.Print(2, 6, "(Mochila Vazia)", Color.Gray, Color.Black);
            }
            else
            {
                for (int i = 0; i < itens.Count; i++)
                {
                    bool sel = i == _indiceItem;
                    string prefixo = sel ? "> " : "  ";
                    Color cor = sel ? Color.Yellow : Color.White;

                    string equipadosStr = _jogador.Equipamentos.Contains(itens[i]) ? " [EQUIPADO]" : "";
                    Surface.Print(2, 6 + i, $"{prefixo}{itens[i].Nome} x{itens[i].Quantidade}{equipadosStr}", cor, Color.Black);
                }
            }

            // Coluna Direita: Equipamentos Atuais
            Surface.Print(40, 4, "--- Equipados ---", Color.Cyan, Color.Black);
            Surface.Print(40, 6, $"Arma: {_jogador.ArmaEquipada?.Nome ?? "Nenhuma"}", Color.White, Color.Black);
            Surface.Print(40, 7, $"Armadura: {_jogador.ArmaduraEquipada?.Nome ?? "Nenhuma"}", Color.White, Color.Black);

            // Ícone do item selecionado (canto direito) - vem dos .xp que o colega
            // desenhou (Artes/Icones/), carregado via IconeUtils (Flyweight, igual
            // ao TileFactory).
            if (itens.Count > 0)
            {
                ScreenSurface icone = IconeUtils.ObterIcone(itens[_indiceItem]);
                int iconeX = Width - icone.Width - 3;
                int iconeY = 4;
                Surface.Print(iconeX, iconeY - 1, "Ícone:", Color.Cyan, Color.Black);
                icone.Surface.Copy(Surface, iconeX, iconeY);
            }

            // Painel Inferior: Sub-Menu de Ações
            if (_modo == ModoInventario.SubMenuAcoes && itens.Count > 0)
            {
                int yMenu = Height - 6;
                Surface.Print(2, yMenu - 1, $"Opções para '{itens[_indiceItem].Nome}':", Color.Orange, Color.Black);

                for (int i = 0; i < _opcoesAcao.Length; i++)
                {
                    bool sel = i == _indiceAcao;
                    string prefixo = sel ? "> " : "  ";
                    Color cor = sel ? Color.Yellow : Color.Gray;

                    Surface.Print(2, yMenu + i, $"{prefixo}{_opcoesAcao[i]}", cor, Color.Black);
                }
            }

            Surface.Print(2, Height - 1, "ESC ou 'I' para fechar | Enter para selecionar", Color.DarkGray, Color.Black);
        }
    }
}