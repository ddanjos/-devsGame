using SadRogue.Primitives;

namespace SurvivorGame.Mapa
{
    /// <summary>
    /// Um lugar "clicável" no mapa: nome, posição no grid e uma descrição curta.
    /// Usado tanto pelos pontos turísticos quanto pelo ProWay (ponto de partida).
    /// Ao clicar num desses lugares, o jogo troca de tela pro cenário dele.
    /// </summary>
    internal class LocalMapa
    {
        public string Nome { get; }
        public Point Posicao { get; }
        public string Descricao { get; }

        public LocalMapa(string nome, Point posicao, string descricao)
        {
            Nome = nome;
            Posicao = posicao;
            Descricao = descricao;
        }
    }
}
