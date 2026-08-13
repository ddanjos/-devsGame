namespace SurvivorGame.Inventario
{
    internal class Armadura : ItemInventario
    {
        public int Defesa { get; private set; }

        public Armadura(string nome, string descricao, int quantidade, int defesa, char simbolo) : base(nome, descricao, quantidade, simbolo)
        {
            Defesa = defesa;
        }
    }
}
