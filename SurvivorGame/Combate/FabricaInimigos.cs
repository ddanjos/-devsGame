using SadConsole;
using SurvivorGame.Utilitarios;

namespace SurvivorGame.Combate
{
    /// <summary>
    /// Factory dos inimigos do jogo (mesma ideia do TileFactory, mas sem cache:
    /// cada combate precisa de uma instância NOVA, senão um inimigo derrotado
    /// continuaria com a vida zerada no próximo encontro).
    ///
    /// Centraliza os números num lugar só - antes cada local criava o seu
    /// "new Inimigo(...)" na mão, e o Rato Selvagem já estava duplicado em dois
    /// arquivos com valores repetidos. Balancear o jogo agora é mexer aqui.
    /// </summary>
    internal static class FabricaInimigos
    {
        /// <summary>Andar 0 da ProWay. Guarda a Antena no ninho (ver HISTORIA.md).
        /// O mais fraco do jogo - é o primeiro combate que o jogador encontra.</summary>
        public static Inimigo CriarRatoSelvagem() =>
            new("Rato Selvagem", vidaMaxima: 40, habilidades: new[]
            {
                new Habilidade("Mordida", dano: 8),
                new Habilidade("Arranhão", dano: 5)
            });

        /// <summary>Parque São Francisco de Assis. Combate "de verdade" mais simples
        /// fora da ProWay - ensina que a cidade não é segura nem nas áreas bonitas.</summary>
        public static Inimigo CriarCaoAssilvestrado() =>
            new("Cão Assilvestrado", vidaMaxima: 30, habilidades: new[]
            {
                new Habilidade("Dentada", dano: 10),
                new Habilidade("Rosnado", dano: 4)
            });

        /// <summary>Museu Hering. Guarda a Bateria - o mais difícil dos três
        /// inimigos que dão peça, de propósito (recompensa maior = risco maior).</summary>
        public static Inimigo CriarViraLataAlfa() =>
            new("Vira-Lata Alfa", vidaMaxima: 65, habilidades: new[]
            {
                new Habilidade("Bote da Matilha", dano: 16),
                new Habilidade("Mordida Profunda", dano: 12),
                new Habilidade("Uivo", dano: 6)
            });

        /// <summary>Castelinho da Havan. Outro sobrevivente, não um monstro - dá
        /// pra fugir sem lutar (ver SessaoCombate.Fugir).</summary>
        public static Inimigo CriarSaqueador() =>
            new("Saqueador", vidaMaxima: 50, habilidades: new[]
            {
                new Habilidade("Golpe de Cano", dano: 14),
                new Habilidade("Empurrão", dano: 7)
            });

        /// <summary>Museu da Cerveja. Piada de propósito (tom Earthbound) - dano
        /// baixinho, mas irritante.</summary>
        public static Inimigo CriarEnxameDeMosquitos() =>
            new("Enxame de Mosquitos", vidaMaxima: 18, habilidades: new[]
            {
                new Habilidade("Zumbido Infernal", dano: 3),
                new Habilidade("Picada", dano: 5)
            });

        /// <summary>Arte .xp do rato (único inimigo com arte pronta no projeto).
        /// Os outros ainda não têm - o CombateScreen aceita arte nula e só mostra
        /// nome + HP, então eles funcionam normal até alguém desenhar.</summary>
        public static ScreenSurface CarregarArteRato() =>
            ArteUtils.CarregarArteInimigo("Artes/Inimigos/ratoselvagem.xp");
    }
}
