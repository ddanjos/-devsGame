namespace SurvivorGame.Mapa
{
    using SurvivorGame.Inventario;

    internal class ItemNoMapa
    {
        public int X { get; set; }
        public int Y { get; set; }
        public char Simbolo { get; private set; }
        public ItemInventario Item { get; private set; }

        public ItemNoMapa(int x, int y, char simbolo, ItemInventario item)
        {
            X = x;
            Y = y;
            Simbolo = simbolo;
            Item = item;
        }
    }
}