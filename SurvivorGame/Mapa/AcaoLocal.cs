using System;

namespace SurvivorGame.Mapa
{
    /// <summary>
    /// Uma ação clicável/selecionável dentro de um ILocalExploravel - ex: "Ir para
    /// o Prédio", "Procurar no Lixo", "Descansar" (nomes literais do SCRUM-9). Cada
    /// ação tem um custo de Fome/Sede (gasto sempre que executada, mesmo se o
    /// resultado for "não achou nada") e uma função que decide o que acontece:
    /// nada, trocar de local, ou puxar um combate. Dar item direto no inventário
    /// não precisa de um campo próprio em ResultadoAcao - a função de execução já
    /// recebe o Personagem e pode chamar jogador.Inventario.AdicionarItem(...)
    /// direto antes de devolver o resultado.
    /// </summary>
    internal class AcaoLocal
    {
        public string Texto { get; }
        public int CustoFome { get; }
        public int CustoSede { get; }

        private readonly Func<Personagem, ResultadoAcao> _executar;

        public AcaoLocal(string texto, int custoFome, int custoSede, Func<Personagem, ResultadoAcao> executar)
        {
            Texto = texto;
            CustoFome = custoFome;
            CustoSede = custoSede;
            _executar = executar;
        }

        public ResultadoAcao Executar(Personagem jogador) => _executar(jogador);
    }
}
