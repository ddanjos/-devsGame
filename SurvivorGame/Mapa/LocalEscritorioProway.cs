using System.Collections.Generic;
using System.Linq;
using SurvivorGame.Regras;

namespace SurvivorGame.Mapa
{
    /// <summary>
    /// Versão ponto-e-clique do escritório da ProWay (SCRUM-9) - substitui o antigo
    /// MapaEscritorioProway/ExploracaoScreen (andar de verdade + colisão por pixel),
    /// que não é o que a equipe especificou. A arte do Lindomar
    /// (mapa_inicio_teste.xp) continua sendo usada - só que agora como ILUSTRAÇÃO
    /// do local, não como terreno andável.
    ///
    /// Constrói o andar 0 e o porão no próprio construtor (mesma ideia do
    /// MapaEscritorioProway antigo) e se passa a si mesmo (this) pra eles, pra eles
    /// saberem voltar - é seguro em C# passar 'this' num construtor pra outro
    /// objeto guardar como referência, desde que ele não chame nada de volta nesse
    /// meio-tempo (e ele não chama, só guarda).
    /// </summary>
    internal class LocalEscritorioProway : ILocalExploravel
    {
        public string Nome => "Escritório da ProWay";

        /// <summary>A descrição muda conforme a missão avança: o rádio quebrado na
        /// mesa é o gancho inicial da história (ver HISTORIA.md) e vira o lembrete
        /// constante do que falta. Um jogador que voltar aqui sempre sabe quantas
        /// peças ainda precisa, sem precisar abrir menu nenhum.</summary>
        public string Descricao
        {
            get
            {
                string baseTexto =
                    "Mesas viradas, poeira grossa no ar, papéis espalhados pelo chão. Na mesa " +
                    "ao lado de onde você acordou, um rádio antigo - sem antena, sem bateria, " +
                    "o fusível queimado. ";

                if (GerenciadorJogo.PodeTransmitir)
                    return baseTexto + "Você tem as três peças na mochila. É agora.";

                var faltando = new List<string>();
                if (!GerenciadorJogo.TemAntena) faltando.Add("a Antena");
                if (!GerenciadorJogo.TemBateria) faltando.Add("a Bateria");
                if (!GerenciadorJogo.TemFusivel) faltando.Add("o Fusível");

                return baseTexto +
                    $"({GerenciadorJogo.PecasEncontradas}/3 peças) Ainda falta encontrar {string.Join(" e ", faltando)} " +
                    "em algum lugar da cidade.";
            }
        }

        public string? CaminhoArte => "Artes/Cenarios/mapa_inicio_teste.xp";

        /// <summary>A lista de ações é montada a cada leitura (não guardada num
        /// campo) porque a ação de transmitir só existe depois que as 3 peças forem
        /// encontradas - ver GerenciadorJogo.PodeTransmitir.</summary>
        public IReadOnlyList<AcaoLocal> Acoes =>
            GerenciadorJogo.PodeTransmitir
                ? new[] { AcaoTransmitir() }.Concat(_acoesBase).ToArray()
                : _acoesBase;

        private readonly AcaoLocal[] _acoesBase;

        /// <summary>A CONDIÇÃO DE VITÓRIA do jogo: com as 3 peças, consertar e usar
        /// o rádio encerra a partida com a tela de vitória.</summary>
        private static AcaoLocal AcaoTransmitir() =>
            new("*** Consertar e transmitir o rádio ***", custoFome: 0, custoSede: 0,
                _ => new ResultadoAcao { VenceuOJogo = true });

        private readonly ILocalExploravel _andarZero;
        private readonly ILocalExploravel _porao;

        public LocalEscritorioProway()
        {
            _andarZero = new LocalAndarZero(this);
            _porao = new LocalPoraoProway(this);

            _acoesBase = new[]
            {
                new AcaoLocal("Chamar o elevador (descer pro andar 0)", custoFome: 5, custoSede: 5, jogador =>
                    new ResultadoAcao
                    {
                        Mensagem = "Você desce de elevador até o andar 0.",
                        NovoLocal = _andarZero
                    }),

                new AcaoLocal("Descer a escada pro porão", custoFome: 5, custoSede: 5, jogador =>
                    new ResultadoAcao
                    {
                        Mensagem = "Você desce as escadas com cuidado - está escuro lá embaixo.",
                        NovoLocal = _porao
                    }),

                new AcaoLocal("Descansar um pouco", custoFome: 10, custoSede: 10, jogador =>
                {
                    jogador.Curar(15);
                    return new ResultadoAcao
                    {
                        Mensagem = "Você descansa encostado na parede e recupera um pouco de vida."
                    };
                }),

                new AcaoLocal("Sair pra rua", custoFome: 0, custoSede: 0, jogador =>
                    new ResultadoAcao
                    {
                        Mensagem = "Você volta pra rua.",
                        VoltarParaAnterior = true
                    }),
            };
        }
    }
}
