using System;
using System.Collections.Generic;
using SurvivorGame.Combate;
using SurvivorGame.Inventario;
using SurvivorGame.Regras;

namespace SurvivorGame.Mapa
{
    /// <summary>
    /// Versão ponto-e-clique do andar 0 (cafeteria) - substitui o antigo
    /// MapaAndarZero/ExploracaoScreen. A arte do Lindomar (location2.xp) vira
    /// ilustração, não mapa andável.
    ///
    /// Aqui fica a ANTENA, primeira das 3 peças do rádio (ver HISTORIA.md): um
    /// rato fez ninho em cima dela atrás do balcão. Enfrentar o rato é uma ação
    /// própria e DETERMINÍSTICA - o jogador vê o ninho e decide ir lá. A peça
    /// principal do jogo não pode depender de sorte, senão a partida trava.
    /// O sorteio fica só na busca por comida, onde perder a aposta não bloqueia
    /// nada.
    /// </summary>
    internal class LocalAndarZero : ILocalExploravel
    {
        public string Nome => "Cafeteria (Andar 0)";

        public string Descricao =>
            "Mesas viradas, cadeiras espalhadas - dá pra ver que muita gente saiu " +
            "correndo daqui. Atrás do balcão, um monte de papel picado e fiapos forma " +
            "um ninho grande demais pro seu gosto - e no meio dele, o brilho de uma " +
            "antena de rádio.";

        public string? CaminhoArte => "Artes/Cenarios/location2.xp";

        public IReadOnlyList<AcaoLocal> Acoes { get; }

        private readonly ILocalExploravel _escritorio;

        public LocalAndarZero(ILocalExploravel escritorio)
        {
            _escritorio = escritorio;

            Acoes = new[]
            {
                new AcaoLocal("Mexer no ninho atrás do balcão", custoFome: 5, custoSede: 5, MexerNoNinho),

                new AcaoLocal("Procurar restos de comida", custoFome: 0, custoSede: 5, ProcurarComida),

                new AcaoLocal("Voltar pro escritório", custoFome: 0, custoSede: 0, jogador =>
                    new ResultadoAcao
                    {
                        Mensagem = "Você sobe de volta pro escritório.",
                        NovoLocal = _escritorio
                    }),

                new AcaoLocal("Seguir pela rua", custoFome: 0, custoSede: 0, jogador =>
                    new ResultadoAcao
                    {
                        Mensagem = "Você sai pra rua.",
                        VoltarParaAnterior = true
                    }),
            };
        }

        /// <summary>Caminho garantido pra Antena: o rato aparece sempre, e vencer
        /// concede a peça (ver GerenciadorJogo.ProcessarVitoriaInimigo).</summary>
        private static ResultadoAcao MexerNoNinho(Personagem jogador)
        {
            if (GerenciadorJogo.TemAntena)
                return new ResultadoAcao { Mensagem = "O ninho está vazio agora. A antena já é sua." };

            return new ResultadoAcao
            {
                Mensagem = "Você estica a mão pro ninho e um rato enorme salta de dentro, furioso!",
                IniciarCombateCom = FabricaInimigos.CriarRatoSelvagem(),
                ArteInimigo = FabricaInimigos.CarregarArteRato()
            };
        }

        /// <summary>Aqui o sorteio é seguro: 45% de achar comida, senão nada. Não
        /// bloqueia progresso nenhum - é só recurso extra pra sobreviver.</summary>
        private static ResultadoAcao ProcurarComida(Personagem jogador)
        {
            if (Random.Shared.Next(100) < 45)
            {
                var lata = new Consumivel("Lata Amassada", "Uma lata de comida, ainda dá pra comer.", 1, 'c',
                    cura: 15, restauraFome: 20);
                bool coletou = jogador.Inventario.AdicionarItem(lata);

                return new ResultadoAcao
                {
                    Mensagem = coletou
                        ? "Você achou uma lata de comida amassada atrás do balcão!"
                        : "Achou uma lata de comida, mas a mochila está cheia."
                };
            }

            return new ResultadoAcao { Mensagem = "Só poeira e cadeiras quebradas. Nada de útil dessa vez." };
        }
    }
}
