using SurvivorGame.Inventario;

namespace SurvivorGame.Mapa
{
    internal class MapaJogo
    {
        public List<ItemNoMapa> ItensNoChao { get; set; }

        public MapaJogo() 
        {
            ItensNoChao = new List<ItemNoMapa>();

        }

        public void AdicionarItens(int x, int y, char simbolo, ItemInventario item)
        {
            ItensNoChao.Add(new ItemNoMapa(x, y, simbolo, item));
        }

        public ItemNoMapa ObterItensNaPosicao(int x, int y)
        {
            return ItensNoChao.FirstOrDefault(i => i.X == x && i.Y == y);

        }

        public void RemoverItem(ItemNoMapa item)
        {
            ItensNoChao.Remove(item);
        }
    }
}
