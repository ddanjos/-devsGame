using SurvivorGame.Combate;
using SurvivorGame.Inventario;

namespace SurvivorGame
{
    internal class Personagem
    {
        public string Nome { get; private set; }
        public int Experiencia { get; private set; }
        public int Fome { get; private set; }
        public int Sede { get; private set; }
        public int Vida { get; private set; }
        public int VidaMaxima { get; private set; }
        public int X { get; set; }
        public int Y { get; set; }
        public InventarioPersonagem Inventario { get; private set; }

        /// <summary>Dano do ataque basico (desarmado). Quando existir sistema de
        /// equipar arma, isso pode virar o dano da arma equipada.</summary>
        public int DanoBase { get; set; } = 10;

        /// <summary>Ataques especiais que custam Energia em combate. Comeca com um
        /// de exemplo - podem adicionar mais, inclusive por classe de personagem.</summary>
        public List<Habilidade> HabilidadesEspeciais { get; } = new()
        {
            new Habilidade("Golpe Forte", dano: 25, custoEnergia: 3)
        };

        public Personagem(string nome, int xInicial, int yInicial)
        {
            Nome = nome;
            Experiencia = 0;
            Fome = 0;
            Sede = 0;
            VidaMaxima = 100;
            Vida = 100;
            X = xInicial;
            Y = yInicial;

            Inventario = new InventarioPersonagem(5);
        }

        /// <summary>Reduz a Vida, sem deixar passar de 0.</summary>
        public void ReceberDano(int quantidade)
        {
            if (quantidade <= 0) return;
            Vida = Math.Max(0, Vida - quantidade);
        }

        /// <summary>Aumenta a Vida, sem deixar passar de VidaMaxima.</summary>
        public void Curar(int quantidade)
        {
            if (quantidade <= 0) return;
            Vida = Math.Min(VidaMaxima, Vida + quantidade);
        }
    }
}
