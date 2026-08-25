using System;
using System.Collections.Generic;

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

        /// <summary>Quantas vezes o jogador tocou o sino da Catedral, e se o
        /// segredo já foi revelado (SCRUM-15). Este estado morava dentro do
        /// LocalCatedral, mas foi trazido pra cá quando o Save entrou: assim TODO
        /// o progresso de uma partida está num lugar só, e o SaveJogo grava esta
        /// classe inteira em vez de sair caçando estado espalhado pelos locais.
        /// Ver Mapa/LocalCatedral e Regras/SaveJogo.</summary>
        public static int SinosTocados { get; set; }
        public static bool SegredoCatedralRevelado { get; set; }

        /// <summary>Condição de Vitória: com as 3 peças, o rádio pode ser
        /// consertado e a transmissão de socorro enviada.</summary>
        public static bool PodeTransmitir => TemAntena && TemBateria && TemFusivel;

        /// <summary>Quantas das 3 peças já foram encontradas - usado pelos textos
        /// de progresso que aparecem pro jogador ("2 de 3 peças").</summary>
        public static int PecasEncontradas =>
            (TemAntena ? 1 : 0) + (TemBateria ? 1 : 0) + (TemFusivel ? 1 : 0);

        /// <summary>
        /// Linha de status da missão, mostrada o tempo todo no mapa da cidade.
        /// Existe por um motivo achado em playtest: dois jogadores juntaram as três
        /// peças e não descobriram o que fazer com elas. As peças NÃO são itens de
        /// inventário (são progresso, não objeto), então não havia onde olhar - a
        /// única pista era a descrição da ProWay, que só aparece depois de entrar
        /// lá. Agora o objetivo, o que falta e ONDE terminar ficam sempre à vista.
        /// </summary>
        public static string ResumoDaMissao
        {
            get
            {
                if (PodeTransmitir)
                    return "RÁDIO 3/3 - volte ao Escritório da ProWay e conserte o rádio!";

                var faltando = new List<string>();
                if (!TemAntena) faltando.Add("Antena");
                if (!TemBateria) faltando.Add("Bateria");
                if (!TemFusivel) faltando.Add("Fusível");

                return $"RÁDIO {PecasEncontradas}/3 - falta: {string.Join(", ", faltando)}";
            }
        }

        /// <summary>Zera o progresso. Chamado ao começar uma partida nova.</summary>
        public static void Reiniciar()
        {
            TemAntena = false;
            TemBateria = false;
            TemFusivel = false;
            SinosTocados = 0;
            SegredoCatedralRevelado = false;
        }

        /// <summary>
        /// Chamado ao vencer um combate: alguns inimigos guardam uma peça do rádio.
        /// A checagem é por nome porque os inimigos são criados em vários lugares
        /// (FabricaInimigos, ações de local) e não carregam um campo de "peça que
        /// dropa" - se o roster crescer muito, vale mover isso pra dentro do
        /// próprio Inimigo.
        /// </summary>
        /// <summary>Frase extra colada na mensagem da TERCEIRA peça: é o momento
        /// exato em que o jogador precisa saber pra onde ir.</summary>
        public static string AvisoDeFinal() =>
            PodeTransmitir ? " Você tem as 3 peças - volte ao Escritório da ProWay!" : string.Empty;

        public static string ProcessarVitoriaInimigo(string nomeInimigo)
        {
            if (nomeInimigo.Contains("Rato", StringComparison.OrdinalIgnoreCase) && !TemAntena)
            {
                TemAntena = true;
                return $" [PEÇA {PecasEncontradas}/3] No ninho do rato estava a Antena de Rádio!{AvisoDeFinal()}";
            }

            if (nomeInimigo.Contains("Vira-Lata", StringComparison.OrdinalIgnoreCase) && !TemBateria)
            {
                TemBateria = true;
                return $" [PEÇA {PecasEncontradas}/3] Você alcança a Bateria industrial no armazém!{AvisoDeFinal()}";
            }

            return string.Empty;
        }
    }
}
