using SurvivorGame.Inventario;
using SurvivorGame.Mapa;
using System;
using System.Collections.Generic;
using System.Text;

namespace SurvivorGame.Regras
{
    internal static class AcoesJogador
    {
        /// <summary>
        /// Remove uma quantidade do item do inventário do jogador e cria uma instância no chão.
        /// </summary>
        public static bool DroparItem(Personagem jogador, MapaJogo mapa, string nomeItem, int quantidade = 1)
        {
            var itemMochila = jogador.Inventario.Itens
                .FirstOrDefault(i => i.Nome.Equals(nomeItem, System.StringComparison.OrdinalIgnoreCase));

            if (itemMochila == null) return false;

            ItemInventario itemParaChao = CriarCopiaItem(itemMochila, quantidade);

            bool removeu = jogador.Inventario.RemoverItem(nomeItem, quantidade);

            if (removeu)
            {
                mapa.AdicionarItens(jogador.X, jogador.Y, itemParaChao.Simbolo, itemParaChao);
                return true;
            }

            return false;
        }

        private static ItemInventario CriarCopiaItem(ItemInventario original, int quantidade)
        {
            return original switch
            {
                Consumivel c => new Consumivel(c.Nome, c.Descricao, c.Cura, c.Simbolo, quantidade),
                Arma a => new Arma(a.Nome, a.Descricao, a.Dano, a.Simbolo),
                Armadura arm => new Armadura(arm.Nome, arm.Descricao, arm.Defesa, arm.Simbolo),
                _ => throw new System.InvalidOperationException("Tipo de item desconhecido.")
            };
        }
    }
}
    }
}
