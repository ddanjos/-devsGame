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

        private readonly string[] _opcoesAcao = { "Usar / Equipar", "Desequipar", "Largar no Chao", "Voltar" };

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
                        // Um consumível pode restaurar Vida, Fome e/ou Sede - antes
                        // só existia Cura (Vida), então uma garrafa d'água matava
                        // ferimento e não matava sede, contrariando a própria
                        // descrição do item.
                        _jogador.Curar(consumivel.Cura);
                        _jogador.RestaurarFome(consumivel.RestauraFome);
                        _jogador.RestaurarSede(consumivel.RestauraSede);
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
                    // Só desequipa se for a ÚLTIMA unidade da pilha: largar 1 de
                    // "Faca x3" não pode deixar o jogador desarmado com 2 facas
                    // ainda na mochila.
                    if (item.Quantidade <= 1)
                        _jogador.Desequipar(item);

                    if (_mapaJogo is not null)
                    {
                        AcoesJogador.DroparItem(_jogador, _mapaJogo, item.Nome, 1);
                    }
                    // Sem mapa (inventário aberto de dentro de um local), NÃO
                    // removemos: antes o item era simplesmente apagado do jogo, o
                    // que fazia "Largar no Chão" destruir o item em vez de largá-lo.
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

            // Painel de status do personagem (SCRUM-12): antes só o HP aparecia
            // aqui, mesmo com Fome, Sede, Força e Defesa já existindo no
            // Personagem. Equipar uma arma ou armadura muda Força/Defesa na hora,
            // porque estes valores são lidos direto do personagem a cada redesenho.
            Surface.PrintTexto(2, 1, "=== INVENTARIO DO SOBREVIVENTE ===", Color.Yellow, Color.Black);
            Surface.PrintTexto(2, 2, $"NIvel {_jogador.Nivel}   HP: {_jogador.Vida}/{_jogador.VidaMaxima}", Color.LimeGreen, Color.Black);
            Surface.PrintTexto(2, 3, $"Fome: {_jogador.Fome}   Sede: {_jogador.Sede}   Dano: {_jogador.DanoBase}   Forca: {_jogador.Forca}   Defesa: {_jogador.Defesa}", Color.White, Color.Black);

            // Coluna Esquerda: Itens da Mochila
            Surface.PrintTexto(2, 4, "--- Mochila ---", Color.Cyan, Color.Black);
            var itens = _jogador.Inventario.Itens;

            if (itens.Count == 0)
            {
                Surface.PrintTexto(2, 6, "(Mochila Vazia)", Color.Gray, Color.Black);
            }
            else
            {
                for (int i = 0; i < itens.Count; i++)
                {
                    bool sel = i == _indiceItem;
                    string prefixo = sel ? "> " : "  ";
                    Color cor = sel ? Color.Yellow : Color.White;

                    string equipadosStr = _jogador.Equipamentos.Contains(itens[i]) ? " [EQUIPADO]" : "";
                    Surface.PrintTexto(2, 6 + i, $"{prefixo}{itens[i].Nome} x{itens[i].Quantidade}{equipadosStr}", cor, Color.Black);
                }
            }

            // Coluna Direita: Equipamentos Atuais
            Surface.PrintTexto(40, 4, "--- Equipados ---", Color.Cyan, Color.Black);
            Surface.PrintTexto(40, 6, $"Arma: {_jogador.ArmaEquipada?.Nome ?? "Nenhuma"}", Color.White, Color.Black);
            Surface.PrintTexto(40, 7, $"Armadura: {_jogador.ArmaduraEquipada?.Nome ?? "Nenhuma"}", Color.White, Color.Black);

            // Ícone do item selecionado (canto direito) - vem dos .xp que o colega
            // desenhou (Artes/Icones/), carregado via IconeUtils (Flyweight, igual
            // ao TileFactory).
            if (itens.Count > 0)
            {
                // Pode vir null se o .xp do ícone não carregar - nesse caso a tela
                // só não mostra ícone. Antes o null vinha disfarçado de objeto e
                // apertar 'I' fechava o jogo com NullReferenceException.
                ScreenSurface? icone = IconeUtils.ObterIcone(itens[_indiceItem]);
                if (icone is not null)
                {
                    int iconeX = Width - icone.Width - 3;
                    int iconeY = 4;
                    Surface.PrintTexto(iconeX, iconeY - 1, "Icone:", Color.Cyan, Color.Black);
                    PainelUi.DesenharPorCima(icone, Surface, iconeX, iconeY);
                }
            }

            // Painel Inferior: Sub-Menu de Ações
            if (_modo == ModoInventario.SubMenuAcoes && itens.Count > 0)
            {
                int yMenu = Height - 6;
                Surface.PrintTexto(2, yMenu - 1, $"Opcoes para '{itens[_indiceItem].Nome}':", Color.Orange, Color.Black);

                for (int i = 0; i < _opcoesAcao.Length; i++)
                {
                    bool sel = i == _indiceAcao;
                    string prefixo = sel ? "> " : "  ";
                    Color cor = sel ? Color.Yellow : Color.Gray;

                    Surface.PrintTexto(2, yMenu + i, $"{prefixo}{_opcoesAcao[i]}", cor, Color.Black);
                }
            }

            Surface.PrintTexto(2, Height - 1, "ESC ou 'I' para fechar | Enter para selecionar", Color.DarkGray, Color.Black);
        }
    }
}