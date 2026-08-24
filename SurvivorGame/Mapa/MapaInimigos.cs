using SadRogue.Primitives;

namespace SurvivorGame.Mapa;

public class MapaInimigos
{
    private readonly List<InimigoNoMapa> _inimigos = new();
    public IReadOnlyList<InimigoNoMapa> Inimigos => _inimigos;

    public void AdicionarInimigo(InimigoNoMapa inimigo)
    {
        _inimigos.Add(inimigo);
    }

    public InimigoNoMapa? ObterInimigoNaPosicao(Point posicao)
    {
        return _inimigos.Find(i => i.X == posicao.X && i.Y == posicao.Y);
    }

    public void RemoverInimigo(InimigoNoMapa inimigo)
    {
        _inimigos.Remove(inimigo);
    }
}