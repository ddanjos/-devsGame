using System.Collections.Generic;

namespace SurvivorGame.Mapa
{
    /// <summary>
    /// Versão ponto-e-clique (mínima, ainda sem arte própria) do porão da ProWay -
    /// antes era MapaMasmorra, andável, acessada pela escada do escritório. Ainda
    /// não convertemos o conteúdo da masmorra pra esse novo formato (fica pra uma
    /// próxima rodada); por enquanto é só o elo da cadeia (escritório -> porão)
    /// pra não deixar a escada sem destino nenhum.
    /// </summary>
    internal class LocalPoraoProway : ILocalExploravel
    {
        public string Nome => "Porão da ProWay";

        public string Descricao =>
            "Escuro e úmido. Caixas empilhadas contra a parede, algumas já mofadas. " +
            "Dá pra ouvir um gotejar distante.";

        public IReadOnlyList<AcaoLocal> Acoes { get; }

        private readonly ILocalExploravel _escritorio;

        public LocalPoraoProway(ILocalExploravel escritorio)
        {
            _escritorio = escritorio;

            Acoes = new[]
            {
                new AcaoLocal("Vasculhar as caixas", custoFome: 0, custoSede: 5, jogador =>
                    new ResultadoAcao { Mensagem = "Só caixas vazias e mofo. Nada de útil por aqui - ainda." }),

                new AcaoLocal("Subir de volta pro escritório", custoFome: 0, custoSede: 0, jogador =>
                    new ResultadoAcao
                    {
                        Mensagem = "Você sobe de volta pro escritório.",
                        NovoLocal = _escritorio
                    }),
            };
        }
    }
}
