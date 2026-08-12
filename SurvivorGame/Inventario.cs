namespace SurvivorGame
{
    internal class Inventario
    {
        public List<ItemInventario> item { get; private set; }
        public int capacidade { get; private set; }
    
        public Inventario()
        {
            item = new List<ItemInventario>();
            capacidade = 1;
        }
    }
}
