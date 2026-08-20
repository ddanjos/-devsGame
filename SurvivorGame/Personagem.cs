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

        private const int DanoDesarmado = 10;

        /// <summary>Dano do ataque básico. É o dano desarmado por padrão; quando uma
        /// Arma é equipada, passa a valer o Dano dela (ver Equipar/Desequipar).</summary>
        public int DanoBase { get; private set; } = DanoDesarmado;

        public Arma? ArmaEquipada { get; private set; }
        public Armadura? ArmaduraEquipada { get; private set; }

        /// <summary>Redução de dano recebido, vinda da armadura equipada (0 se nenhuma).</summary>
        public int Defesa => ArmaduraEquipada?.Defesa ?? 0;

        /// <summary>Itens do inventário que estão atualmente equipados (arma e/ou armadura).</summary>
        public IEnumerable<ItemInventario> Equipamentos
        {
            get
            {
                if (ArmaEquipada is not null) yield return ArmaEquipada;
                if (ArmaduraEquipada is not null) yield return ArmaduraEquipada;
            }
        }

        /// <summary>Equipa uma Arma (passa a valer como DanoBase) ou Armadura (passa a reduzir dano recebido).
        /// Itens que não sejam Arma/Armadura são ignorados - use outro fluxo para consumíveis.</summary>
        public void Equipar(ItemInventario item)
        {
            switch (item)
            {
                case Arma arma:
                    ArmaEquipada = arma;
                    DanoBase = arma.Dano;
                    break;
                case Armadura armadura:
                    ArmaduraEquipada = armadura;
                    break;
            }
        }

        /// <summary>Desequipa o item passado, se ele for o que está atualmente equipado nesse slot.</summary>
        public void Desequipar(ItemInventario item)
        {
            if (item is Arma && ReferenceEquals(item, ArmaEquipada))
            {
                ArmaEquipada = null;
                DanoBase = DanoDesarmado;
            }
            else if (item is Armadura && ReferenceEquals(item, ArmaduraEquipada))
            {
                ArmaduraEquipada = null;
            }
        }

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

        /// <summary>Reduz a Vida pelo dano recebido menos a Defesa da armadura equipada
        /// (nunca abaixo de 0 de dano), sem deixar a Vida passar de 0.</summary>
        public void ReceberDano(int quantidade)
        {
            if (quantidade <= 0) return;
            int danoFinal = Math.Max(0, quantidade - Defesa);
            Vida = Math.Max(0, Vida - danoFinal);
        }

        /// <summary>Aumenta a Vida, sem deixar passar de VidaMaxima.</summary>
        public void Curar(int quantidade)
        {
            if (quantidade <= 0) return;
            Vida = Math.Min(VidaMaxima, Vida + quantidade);
        }
    }
}
