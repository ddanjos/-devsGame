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

        /// <summary>Fome/Sede são recursos que se GASTAM (SCRUM-9: "cada ação vai
        /// gastar uma quantidade de Fome e Sede"; Henrique no combate: "a cada
        /// turno... consome 1 ponto de Fome e Sede, se algum chegar a 0 o
        /// personagem começa a tomar dano") - por isso começam CHEIOS (100), não
        /// em 0 como estava antes. Zero = passando fome/sede de verdade.</summary>
        public const int FomeMaxima = 100;
        public const int SedeMaxima = 100;

        /// <summary>Dano do ataque básico. É o dano desarmado por padrão; quando uma
        /// Arma é equipada, passa a valer o Dano dela (ver Equipar/Desequipar).</summary>
        public int DanoBase { get; private set; } = DanoDesarmado;

        public Arma? ArmaEquipada { get; private set; }
        public Armadura? ArmaduraEquipada { get; private set; }

        /// <summary>
        /// Força do personagem (SCRUM-7, pedido do Henrique): soma ao dano de cada
        /// golpe, ponto a ponto. Fórmula completa em SessaoCombate.Atacar:
        ///     Dano Final = (dano do golpe + Forca) - Defesa do alvo
        /// É um atributo do PERSONAGEM, separado do dano da arma - a arma entra
        /// como "dano do golpe" via DanoBase. Fica com set público pra um sistema
        /// de nível/experiência poder aumentá-la depois.
        /// </summary>
        public int Forca { get; set; } = ForcaInicial;

        /// <summary>Defesa própria do personagem, sem contar equipamento. Separada
        /// pra um buff temporário ou ganho de nível poder mexer nela sem afetar a
        /// armadura.</summary>
        public int DefesaBase { get; set; }

        private const int ForcaInicial = 5;

        /// <summary>
        /// Nível do personagem, derivado da Experiencia (SCRUM-12 pede que ele
        /// apareça no painel de status, "preparando pro sistema futuro de
        /// experiência"). A cada 100 de experiência sobe um nível. Nada concede
        /// experiência ainda - por isso fica em 1 até alguém implementar isso,
        /// mas a conta já está pronta e o painel já mostra.
        /// </summary>
        public int Nivel => 1 + (Experiencia / 100);

        /// <summary>Redução de dano recebido: a defesa própria mais a da armadura
        /// equipada. Cada ponto tira 1 de dano (regra do Henrique).</summary>
        public int Defesa => DefesaBase + (ArmaduraEquipada?.Defesa ?? 0);

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
            Fome = FomeMaxima;
            Sede = SedeMaxima;
            VidaMaxima = 100;
            Vida = 100;
            X = xInicial;
            Y = yInicial;

            Inventario = new InventarioPersonagem(5);
        }

        /// <summary>
        /// Aplica dano JÁ CALCULADO (a Defesa entra na fórmula do SessaoCombate,
        /// não aqui - descontar duas vezes deixaria o personagem quase imune).
        /// Mantido separado de ReceberDanoDireto por clareza de intenção: este é o
        /// dano "normal" de combate; o outro é inanição, que ignora armadura.
        /// </summary>
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

        /// <summary>Gasta Fome (ex: ação do SCRUM-9, ou 1 ponto por rodada de
        /// combate), sem deixar passar de 0. Ver comentário em FomeMaxima.</summary>
        public void ConsumirFome(int quantidade)
        {
            if (quantidade <= 0) return;
            Fome = Math.Max(0, Fome - quantidade);
        }

        /// <summary>Mesma ideia de ConsumirFome, pra Sede.</summary>
        public void ConsumirSede(int quantidade)
        {
            if (quantidade <= 0) return;
            Sede = Math.Max(0, Sede - quantidade);
        }

        /// <summary>Recupera Fome (comida), sem passar de FomeMaxima.</summary>
        public void RestaurarFome(int quantidade)
        {
            if (quantidade <= 0) return;
            Fome = Math.Min(FomeMaxima, Fome + quantidade);
        }

        /// <summary>Recupera Sede (água), sem passar de SedeMaxima.</summary>
        public void RestaurarSede(int quantidade)
        {
            if (quantidade <= 0) return;
            Sede = Math.Min(SedeMaxima, Sede + quantidade);
        }

        /// <summary>Dano que IGNORA a Defesa da armadura - usado pela inanição e
        /// desidratação (Fome/Sede em 0). Uma armadura não protege de passar fome,
        /// então esse caminho não passa pelo desconto de Defesa do ReceberDano.</summary>
        public void ReceberDanoDireto(int quantidade)
        {
            if (quantidade <= 0) return;
            Vida = Math.Max(0, Vida - quantidade);
        }

        /// <summary>Estado do personagem (termo pedido pela disciplina): vivo,
        /// passando fome/sede, ou morto. Derivado, não guardado - assim nunca fica
        /// dessincronizado dos valores reais.</summary>
        public EstadoPersonagem Estado
        {
            get
            {
                if (Vida <= 0) return EstadoPersonagem.Morto;
                if (Fome <= 0 || Sede <= 0) return EstadoPersonagem.Debilitado;
                return EstadoPersonagem.Saudavel;
            }
        }
    }
}
