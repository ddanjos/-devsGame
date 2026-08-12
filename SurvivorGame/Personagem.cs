using System;
using System.Collections.Generic;
using System.Text;

namespace SurvivorGame
{
    internal class Personagem
    {
        public string Nome { get; private set; }
        public int Experiencia { get; private set; }
        public int Fome { get; private set; }
        public int Sede { get; private set; }
        public int Vida { get; private set; }
        public Inventario Inventario { get; private set; }

        public Personagem(string nome)
        {
            Nome = nome;
            Experiencia = 0;
            Fome = 0;
            Sede = 0;
            Vida = 100;
            Inventario = new Inventario();
        }

    }
}
