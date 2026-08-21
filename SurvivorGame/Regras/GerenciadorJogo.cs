using System;

namespace SurvivorGame.Regras
{
    /// <summary>
    /// Guarda o progresso da missão principal ("O Último Sinal", ver HISTORIA.md):
    /// o jogador precisa achar as 3 peças do rádio - Antena, Bateria e Fusível -
    /// pra poder transmitir um pedido de socorro. Reunir as três é a CONDIÇÃO DE
    /// VITÓRIA do jogo (termo pedido pela disciplina); ver PodeTransmitir e a ação
    /// condicional em Mapa/LocalEscritorioProway.
    ///
    /// É estático porque existe um progresso só por partida - mas por isso mesmo
    /// precisa de Reiniciar(): sem isso, começar um jogo novo na mesma execução
    /// herdaria as peças da partida anterior.
    /// </summary>
    public static class GerenciadorJogo
    {
        public static bool TemAntena { get; set; }
        public static bool TemBateria { get; set; }
        public static bool TemFusivel { get; set; }

        /// <summary>Condição de Vitória: com as 3 peças, o rádio pode ser
        /// consertado e a transmissão de socorro enviada.</summary>
        public static bool PodeTransmitir => TemAntena && TemBateria && TemFusivel;

        /// <summary>Quantas das 3 peças já foram encontradas - usado pelos textos
        /// de progresso que aparecem pro jogador ("2 de 3 peças").</summary>
        public static int PecasEncontradas =>
            (TemAntena ? 1 : 0) + (TemBateria ? 1 : 0) + (TemFusivel ? 1 : 0);

        /// <summary>Zera o progresso. Chamado ao começar uma partida nova.</summary>
        public static void Reiniciar()
        {
            TemAntena = false;
            TemBateria = false;
            TemFusivel = false;
        }

        /// <summary>
        /// Chamado ao vencer um combate: alguns inimigos guardam uma peça do rádio.
        /// A checagem é por nome porque os inimigos são criados em vários lugares
        /// (FabricaInimigos, ações de local) e não carregam um campo de "peça que
        /// dropa" - se o roster crescer muito, vale mover isso pra dentro do
        /// próprio Inimigo.
        /// </summary>
        public static string ProcessarVitoriaInimigo(string nomeInimigo)
        {
            if (nomeInimigo.Contains("Rato", StringComparison.OrdinalIgnoreCase) && !TemAntena)
            {
                TemAntena = true;
                return $" [PEÇA {PecasEncontradas}/3] No ninho do rato estava a Antena de Rádio!";
            }

            if (nomeInimigo.Contains("Vira-Lata", StringComparison.OrdinalIgnoreCase) && !TemBateria)
            {
                TemBateria = true;
                return $" [PEÇA {PecasEncontradas}/3] Você alcança a Bateria industrial no armazém!";
            }

            return string.Empty;
        }
    }
}
