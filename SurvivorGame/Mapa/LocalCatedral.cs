using System.Collections.Generic;
using SurvivorGame.Inventario;
using SurvivorGame.Regras;

namespace SurvivorGame.Mapa
{
    /// <summary>
    /// Catedral São Paulo Apóstolo - o easter egg do jogo (ideia que já estava no
    /// backlog do time, SCRUM-15: "segredinhos escondidos para tornar a exploração
    /// mais divertida e viciante").
    ///
    /// Tem classe própria (em vez de usar LocalCidade) por um motivo real: guarda
    /// ESTADO entre visitas. Cada vez que o jogador toca o sino, um dos três sinos
    /// responde; na terceira vez a melodia completa toca e revela um item escondido.
    /// Nada avisa o jogador disso - a recompensa é pra quem insiste, que é o ponto
    /// de um easter egg.
    /// </summary>
    internal class LocalCatedral : ILocalExploravel
    {
        public string Nome => "Catedral São Paulo Apóstolo";

        public string Descricao =>
            "Os vitrais coloridos ainda filtram luz estranha sobre os bancos vazios. " +
            "Às vezes, sem ninguém tocar em nada, um dos sinos eletrônicos solta uma " +
            "nota sozinho - energia residual, ou vontade própria.";

        public string? CaminhoArte => "Artes/Cenarios/catedralsaopaulo.xp";

        public IReadOnlyList<AcaoLocal> Acoes { get; }

        /// <summary>Quantas vezes o jogador já tocou o sino, e se o segredo já
        /// saiu. Antes eram campos desta classe; foram pro GerenciadorJogo quando
        /// o Save entrou, pra que todo o progresso da partida fique num lugar só e
        /// possa ser gravado e restaurado junto. Ver Regras/SaveJogo.</summary>
        private static int VezesQueTocouOSino
        {
            get => GerenciadorJogo.SinosTocados;
            set => GerenciadorJogo.SinosTocados = value;
        }

        private static bool SegredoJaRevelado
        {
            get => GerenciadorJogo.SegredoCatedralRevelado;
            set => GerenciadorJogo.SegredoCatedralRevelado = value;
        }

        public LocalCatedral()
        {
            Acoes = new[]
            {
                new AcaoLocal("Tocar o sino", custoFome: 2, custoSede: 2, TocarSino),

                new AcaoLocal("Sentar num banco e recuperar o fôlego", custoFome: 3, custoSede: 3, jogador =>
                {
                    jogador.Curar(10);
                    return new ResultadoAcao
                    {
                        Mensagem = "O silêncio aqui dentro acalma. Você recupera 10 de vida."
                    };
                }),

                new AcaoLocal("Voltar pra rua", custoFome: 0, custoSede: 0, jogador =>
                    new ResultadoAcao { Mensagem = string.Empty, VoltarParaAnterior = true }),
            };
        }

        private ResultadoAcao TocarSino(Personagem jogador)
        {
            if (SegredoJaRevelado)
                return new ResultadoAcao { Mensagem = "Os sinos já contaram o que tinham pra contar." };

            VezesQueTocouOSino++;

            if (VezesQueTocouOSino == 1)
                return new ResultadoAcao { Mensagem = "Uma nota grave ecoa pela nave vazia. Só uma. Os outros dois sinos ficam quietos." };

            if (VezesQueTocouOSino == 2)
                return new ResultadoAcao { Mensagem = "Dois sinos respondem dessa vez, quase juntos. Parece que falta um." };

            var cantil = new Consumivel("Cantil do Padre",
                "Um cantil de metal, cheio até a boca. Alguém guardou com carinho.", 1, 'u',
                cura: 30, restauraSede: 40);
            bool coletou = jogador.Inventario.AdicionarItem(cantil);

            // Só marca o segredo como revelado se o jogador REALMENTE levou o
            // cantil: antes, com a mochila cheia, o item sumia do jogo pra sempre
            // e ainda assim o jogador ganhava a sede restaurada.
            if (!coletou)
                return new ResultadoAcao
                {
                    Mensagem = "Os três sinos tocam juntos! Atrás do altar tem um cantil escondido - mas sua mochila está cheia. Volte com espaço."
                };

            SegredoJaRevelado = true;

            return new ResultadoAcao
            {
                Mensagem = "Os três sinos tocam juntos uma melodia inteira. Atrás do altar, um painel solto revela um esconderijo: o Cantil do Padre, cheio de água limpa!"
            };
        }
    }
}
