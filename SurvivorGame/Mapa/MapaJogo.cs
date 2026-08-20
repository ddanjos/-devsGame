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

        // Bug corrigido: antes o retorno era "ItemNoMapa" (não-anulável), mas
        // FirstOrDefault devolve null quando não há item na posição - quem chamasse
        // sem checar null tomaria NullReferenceException. Agora o tipo deixa isso
        // explícito e o compilador cobra a checagem de quem usa (ver ExploracaoScreen).
        public ItemNoMapa? ObterItensNaPosicao(int x, int y)
        {
            return ItensNoChao.FirstOrDefault(i => i.X == x && i.Y == y);
        }

        public void RemoverItem(ItemNoMapa item)
        {
            ItensNoChao.Remove(item);
        }
    }
}
