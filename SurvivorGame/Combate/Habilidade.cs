namespace SurvivorGame.Combate
{
    /// <summary>
    /// Uma habilidade/ataque usável em combate: nome, dano e quanto de Energia
    /// custa. CustoEnergia = 0 é o ataque básico, sempre disponível de graça.
    /// </summary>
    public class Habilidade
    {
        public string Nome { get; }
        public int Dano { get; }
        public int CustoEnergia { get; }

        public Habilidade(string nome, int dano, int custoEnergia = 0)
        {
            Nome = nome;
            Dano = dano;
            CustoEnergia = custoEnergia;
        }
    }
}
