using System.Collections.Generic;

namespace SurvivorGame.Mapa
{
    /// <summary>
    /// Implementação genérica de ILocalExploravel pros pontos da cidade: recebe
    /// nome, descrição e ações prontos no construtor, em vez de cada museu/parque
    /// precisar de uma classe própria só pra devolver três strings diferentes.
    ///
    /// Os locais da ProWay (LocalEscritorioProway, LocalAndarZero) continuam com
    /// classe própria porque têm lógica de verdade - encadeiam entre si e sorteiam
    /// encontros. Já a Catedral tem classe separada (LocalCatedral) porque guarda
    /// ESTADO entre visitas, coisa que essa classe genérica não faz. Quem consome
    /// (LocalExploravelScreen) não sabe a diferença - é o Strategy funcionando.
    /// Ver FabricaLocais, que monta todos eles.
    /// </summary>
    internal class LocalCidade : ILocalExploravel
    {
        public string Nome { get; }
        public string Descricao { get; }
        public string? CaminhoArte { get; }
        public IReadOnlyList<AcaoLocal> Acoes { get; }

        public LocalCidade(string nome, string descricao, IReadOnlyList<AcaoLocal> acoes,
            string? caminhoArte = null)
        {
            Nome = nome;
            Descricao = descricao;
            Acoes = acoes;
            CaminhoArte = caminhoArte;
        }
    }
}
