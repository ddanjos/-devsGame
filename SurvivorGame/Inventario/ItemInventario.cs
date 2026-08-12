namespace SurvivorGame.Inventario
{
    internal abstract class ItemInventario
    {
        public string Nome { get; private set; }
        public string Descricao { get; private set; }
        public int Quantidade { get; protected set; }

        protected ItemInventario(string nome, string descricao, int quantidade = 1)
        {
            Nome = nome;
            Descricao = descricao;
            Quantidade = quantidade;
        }
        
        public void IncrementarQuantidade(int quantidade = 1)
        {
            if (quantidade > 0)
                Quantidade += quantidade;
        }

        public void DecrementarQuantidade(int quantidade = 1)
        {
            if (quantidade > 0)
                Quantidade = Math.Max(0, Quantidade - quantidade);
        }
    }
}