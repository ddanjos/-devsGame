namespace SurvivorGame.Inventario
{
    internal class Consumivel : ItemInventario
    {
        public int Cura { get; private set; }
        public Consumivel(string nome, string descricao, int quantidade, char simbolo, int cura) : base(nome, descricao, quantidade, simbolo)
        {
            Cura = cura;
        }
    }
}
