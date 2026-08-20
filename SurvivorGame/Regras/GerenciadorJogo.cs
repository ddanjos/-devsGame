using System;

namespace SurvivorGame.Regras
{
    public static class GerenciadorJogo
    {
        public static bool TemAntena { get; set; }
        public static bool TemBateria { get; set; }
        public static bool TemFusivel { get; set; }

        public static bool PodeTransmitir => TemAntena && TemBateria && TemFusivel;

        public static string ProcessarVitoriaInimigo(string nomeInimigo)
        {
            if (nomeInimigo.Contains("Rato", StringComparison.OrdinalIgnoreCase) && !TemAntena)
            {
                TemAntena = true;
                return " [ITEM CHAVE] Você encontrou a Antena de Rádio!";
            }

            return string.Empty;
        }
    }
}