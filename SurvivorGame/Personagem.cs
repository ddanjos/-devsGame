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
        public int X { get; set; }
        public int Y { get; set; }
        public InventarioPersonagem Inventario { get; private set; }

        public Personagem(string nome, int xInicial, int yInicial)
        {
            Nome = nome;
            Experiencia = 0;
            Fome = 0;
            Sede = 0;
            Vida = 100;
            X = xInicial;
            Y = yInicial;

            Inventario = new InventarioPersonagem(5);
        }

    }
}
