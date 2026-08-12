namespace SurvivorGame.Inventario
{
    internal class Armadura : ItemInventario
    {
        public int Defesa { get; private set; }

        public Armadura(string nome, string descricao, int quantidade, int defesa) : base(nome, descricao, quantidade)
        {
            Defesa = defesa;
        }
    }
}
