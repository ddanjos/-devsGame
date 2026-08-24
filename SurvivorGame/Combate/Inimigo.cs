using System;
using System.Collections.Generic;
using System.Linq;
using SurvivorGame.Inventario;

namespace SurvivorGame.Combate
{
    /// <summary>
    /// Um inimigo de combate, na estrutura pedida pelo SCRUM-17: nome, vida,
    /// Forca e Defesa (que entram na fórmula de dano do Henrique), a lista de
    /// ataques que ele pode usar, e o item que ele pode dropar ao morrer.
    ///
    /// Antes daqui, o inimigo usava a mesma classe Habilidade do jogador (que tem
    /// custo de Energia, coisa que inimigo não usa) e o drop era decidido por
    /// nome dentro do GerenciadorJogo. Agora o drop é dado do próprio inimigo -
    /// ver ItemDrop e CombateScreen.Finalizar.
    /// </summary>
    public class Inimigo
    {
        public string Nome { get; }
        public int VidaMaxima { get; }
        public int VidaAtual { get; private set; }

        /// <summary>Soma ao dano de cada golpe. Ver SessaoCombate.TurnoInimigo.</summary>
        public int Forca { get; }

        /// <summary>Reduz o dano recebido, ponto a ponto. Ver SessaoCombate.Atacar.</summary>
        public int Defesa { get; }

        internal IReadOnlyList<AtaqueInimigo> Ataques { get; }

        /// <summary>Item de sobrevivência que este inimigo larga ao ser derrotado
        /// (null = não larga nada). Vai direto pro inventário do jogador.</summary>
        internal ItemInventario? ItemDrop { get; }

        public bool EstaVivo => VidaAtual > 0;

        internal Inimigo(string nome, int vidaMaxima, IEnumerable<AtaqueInimigo> ataques,
            int forca = 0, int defesa = 0, ItemInventario? itemDrop = null)
        {
            Nome = nome;
            VidaMaxima = vidaMaxima;
            VidaAtual = vidaMaxima;
            Ataques = ataques.ToList();
            Forca = forca;
            Defesa = defesa;
            ItemDrop = itemDrop;
        }

        /// <summary>Reduz a VidaAtual, sem deixar passar de 0. A Defesa NÃO é
        /// descontada aqui - ela já entra na fórmula do SessaoCombate, e aplicar
        /// duas vezes deixaria o inimigo praticamente imortal.</summary>
        public void ReceberDano(int quantidade)
        {
            if (quantidade <= 0) return;
            VidaAtual = Math.Max(0, VidaAtual - quantidade);
        }
    }
}
