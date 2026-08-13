namespace SurvivorGame.Inventario
{
    internal class InventarioPersonagem
    {
        private readonly List<ItemInventario> _itens;
        public IReadOnlyCollection<ItemInventario> Itens => _itens.AsReadOnly();
        public int Capacidade { get; private set; }

        public InventarioPersonagem(int capacidade)
        {
            _itens = new List<ItemInventario>();
            Capacidade = capacidade;
        }

        public bool EstaCheio()
        {
            return _itens.Count >= Capacidade;
        }

        public bool AdicionarItem(ItemInventario novoItem)
        {
            if (novoItem == null) return false;

            var itemExistente = _itens.FirstOrDefault(i => i.Nome.Equals(novoItem.Nome, StringComparison.OrdinalIgnoreCase));

            if (itemExistente != null)
            {
                itemExistente.IncrementarQuantidade(novoItem.Quantidade);
                return true;
            }

            if (EstaCheio())
            {
                return false;
            }

            _itens.Add(novoItem);
            return true;
        }

        /// <summary>
        /// Remove uma quantidade especificada do item. Libera o slot no inventário caso a quantidade chegue a zero.
        /// </summary>
        /// <param name="nomeItem">Nome do item a ser removido.</param>
        /// <param name="quantidade">Quantidade a ser removida (padrão 1).</param>
        /// <returns>True se o item existia e foi processado, False se não foi encontrado.</returns>
        public bool RemoverItem(string nomeItem, int quantidade = 1)
        {
            if (string.IsNullOrWhiteSpace(nomeItem) || quantidade <= 0) return false;

            var item = _itens.FirstOrDefault(i => i.Nome.Equals(nomeItem, StringComparison.OrdinalIgnoreCase));

            if (item == null) return false;

            if (quantidade >= item.Quantidade)
            {
                _itens.Remove(item);
            }
            else
            {
                item.DecrementarQuantidade(quantidade);
            }

            return true;
        }
    }
}