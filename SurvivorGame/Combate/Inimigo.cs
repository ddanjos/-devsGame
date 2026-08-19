using System;
using System.Collections.Generic;
using System.Linq;

namespace SurvivorGame.Combate
{
    /// <summary>
    /// Um inimigo de combate: nome, vida, e as habilidades que ele pode usar
    /// contra o jogador no contra-ataque.
    /// </summary>
    public class Inimigo
    {
        public string Nome { get; }
        public int VidaMaxima { get; }
        public int VidaAtual { get; private set; }
        public IReadOnlyList<Habilidade> Habilidades { get; }
        public bool EstaVivo => VidaAtual > 0;

        public Inimigo(string nome, int vidaMaxima, IEnumerable<Habilidade> habilidades)
        {
            Nome = nome;
            VidaMaxima = vidaMaxima;
            VidaAtual = vidaMaxima;
            Habilidades = habilidades.ToList();
        }

        /// <summary>Reduz a VidaAtual, sem deixar passar de 0.</summary>
        public void ReceberDano(int quantidade)
        {
            if (quantidade <= 0) return;
            VidaAtual = Math.Max(0, VidaAtual - quantidade);
        }
    }
}
