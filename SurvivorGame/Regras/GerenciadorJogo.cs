using System;

namespace SurvivorGame.Regras
{
    public static class GerenciadorJogo
    {
        public static bool TemAntena { get; set; }
        public static bool TemBateria { get; set; }
        public static bool TemFusivel { get; set; }

        public static bool PodeTransmitir => TemAntena && TemBateria && TemFusivel;

        /// <summary>Zera o progresso da missão. Necessário porque os campos são static
        /// (vivem durante todo o processo) - sem isso, um "reiniciar" após Game Over
        /// manteria os itens-chave da partida anterior e a vitória nunca mais faria sentido.</summary>
        public static void Reiniciar()
        {
            TemAntena = false;
            TemBateria = false;
            TemFusivel = false;
        }

        public static string ProcessarVitoriaInimigo(string nomeInimigo)
        {
            if (nomeInimigo.Contains("Rato", StringComparison.OrdinalIgnoreCase) && !TemAntena)
            {
                TemAntena = true;
                return " [ITEM CHAVE] Você encontrou a Antena de Rádio!";
            }

            if (nomeInimigo.Contains("Zumbi", StringComparison.OrdinalIgnoreCase) && !TemBateria)
            {
                TemBateria = true;
                return " [ITEM CHAVE] Você encontrou a Bateria!";
            }

            return string.Empty;
        }
    }
}