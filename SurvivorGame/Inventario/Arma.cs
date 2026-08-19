namespace SurvivorGame.Inventario
{
    internal class Arma : ItemInventario
    {
        public int Dano { get; private set; }

        public Arma(string nome, string descricao, int quantidade, int dano, char simbolo) : base(nome, descricao, quantidade, simbolo)
        {
            Dano = dano;
        }

    }
}
