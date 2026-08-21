using SadConsole;
using SurvivorGame.Combate;

namespace SurvivorGame.Mapa
{
    /// <summary>
    /// O que aconteceu depois de executar uma AcaoLocal. Mensagem é sempre exibida
    /// (mesmo vazia = nada a dizer). NovoLocal != null troca o local atual dentro da
    /// mesma LocalExploravelScreen (ex: "Chamar o elevador" leva pro andar 0).
    /// VoltarParaAnterior fecha a exploração e volta pra tela de onde veio (ex: a
    /// cidade) - equivalente ao "Sair pro prédio"/ESC. IniciarCombateCom != null
    /// abre o CombateScreen contra esse inimigo (a "porcentagem de chance de puxar
    /// uma batalha" do SCRUM-9 é decidida DENTRO da função da AcaoLocal, sorteando
    /// antes de montar esse resultado - aqui já chega decidido).
    /// </summary>
    internal class ResultadoAcao
    {
        public string? Mensagem { get; init; }
        public ILocalExploravel? NovoLocal { get; init; }
        public bool VoltarParaAnterior { get; init; }
        public Inimigo? IniciarCombateCom { get; init; }

        /// <summary>Arte do inimigo (opcional) pra mostrar no CombateScreen, quando
        /// IniciarCombateCom != null - mesma ideia do InimigoNoMapa.ArteXP.</summary>
        public ScreenSurface? ArteInimigo { get; init; }

        /// <summary>Condição de Vitória atingida - a ação transmitiu o pedido de
        /// socorro e o jogo acabou (ver LocalEscritorioProway.AcaoTransmitir e
        /// Cenarios/FimDeJogoScreen).</summary>
        public bool VenceuOJogo { get; init; }

        public static readonly ResultadoAcao Vazio = new();
    }
}
