using System;
using System.Collections.Generic;
using System.Text;

namespace SurvivorGame
{
    internal class ItemInventario
    {
        public string Nome { get; private set; }
        public int Dano { get; private set; }

        public string Descricao { get; private set; }

        public int Quantidade { get; private set; }
        public CategoriaItem categoria { get; private set; }
        public ItemInventario() { }
    }
}
