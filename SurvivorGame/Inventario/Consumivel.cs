namespace SurvivorGame.Inventario
{
    /// <summary>
    /// Item que se gasta ao usar. Restaura Vida (Cura), Fome e/ou Sede - os três
    /// separados, como pede o SCRUM-12 ("recupera Vida, Fome ou Sede"). Antes só
    /// existia Cura, então uma garrafa d'água curava ferimento e não matava a sede,
    /// contrariando a descrição do próprio item.
    ///
    /// RestauraFome/RestauraSede têm valor padrão 0 para não quebrar os itens
    /// antigos, que só curavam Vida.
    /// </summary>
    internal class Consumivel : ItemInventario
    {
        public int Cura { get; private set; }
        public int RestauraFome { get; private set; }
        public int RestauraSede { get; private set; }

        public Consumivel(string nome, string descricao, int quantidade, char simbolo,
            int cura, int restauraFome = 0, int restauraSede = 0)
            : base(nome, descricao, quantidade, simbolo)
        {
            Cura = cura;
            RestauraFome = restauraFome;
            RestauraSede = restauraSede;
        }
    }
}
