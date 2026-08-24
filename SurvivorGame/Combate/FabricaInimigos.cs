using SadConsole;
using SurvivorGame.Inventario;
using SurvivorGame.Utilitarios;

namespace SurvivorGame.Combate
{
    /// <summary>
    /// Factory dos inimigos do jogo (mesma ideia do TileFactory, mas sem cache:
    /// cada combate precisa de uma instância NOVA, senão um inimigo derrotado
    /// continuaria com a vida zerada no próximo encontro).
    ///
    /// Centraliza os números num lugar só - balancear o jogo é mexer aqui. Cada
    /// inimigo tem Forca e Defesa (entram na fórmula de dano do SCRUM-7), uma
    /// lista de AtaqueInimigo, e opcionalmente um item que ele larga ao morrer.
    ///
    /// Sobre as "ações nulas": a referência declarada do combate é Earthbound, onde
    /// parte da graça é o inimigo fazer algo inútil. Cada inimigo tem uma dessas
    /// com a cara dele - dá personalidade sem custar mecânica nenhuma.
    /// </summary>
    internal static class FabricaInimigos
    {
        /// <summary>Andar 0 da ProWay. Guarda a Antena no ninho (ver HISTORIA.md).
        /// O mais fraco do jogo - é o primeiro combate que o jogador encontra.</summary>
        public static Inimigo CriarRatoSelvagem() =>
            new("Rato Selvagem", vidaMaxima: 40, forca: 2, defesa: 1, ataques: new[]
            {
                new AtaqueInimigo("Mordida", 8, "O Rato Selvagem crava os dentes na sua perna e tira {0} de vida."),
                new AtaqueInimigo("Arranhão", 5, "O Rato Selvagem te arranha, causando {0} de dano."),
                new AtaqueInimigo("Encarada", 0,
                    "O Rato Selvagem para e fica te encarando com aqueles olhinhos pretos... nada aconteceu!",
                    ehAcaoNula: true),
            },
            itemDrop: new Consumivel("Carne Duvidosa",
                "Você prefere não perguntar de onde veio.", 1, 'c', cura: 8, restauraFome: 12));

        /// <summary>Parque São Francisco de Assis. Combate "de verdade" mais simples
        /// fora da ProWay - ensina que a cidade não é segura nem nas áreas bonitas.</summary>
        public static Inimigo CriarCaoAssilvestrado() =>
            new("Cão Assilvestrado", vidaMaxima: 30, forca: 4, defesa: 1, ataques: new[]
            {
                new AtaqueInimigo("Dentada", 10, "O Cão Assilvestrado abocanha seu braço: {0} de dano."),
                new AtaqueInimigo("Rosnado", 4, "Um rosnado baixo te faz recuar em cima de um galho. {0} de dano."),
                new AtaqueInimigo("Coçar a Orelha", 0,
                    "O Cão Assilvestrado para no meio do ataque pra coçar a orelha com a pata traseira... nada aconteceu!",
                    ehAcaoNula: true),
            },
            itemDrop: new Consumivel("Cantil Roído",
                "Meio mastigado, mas ainda segura água.", 1, 'u', cura: 0, restauraSede: 20));

        /// <summary>
        /// Museu Hering. Guarda a Bateria - o mais difícil dos inimigos que dão
        /// peça, de propósito (recompensa maior = risco maior).
        ///
        /// Números conferidos por simulação (4000 combates por cenário) depois que
        /// a fórmula de dano do SCRUM-7 entrou: com forca 7 e sem ação nula ele
        /// ficava impossível no caminho normal da demo - o jogador chega aqui
        /// machucado do rato e do choque da Prefeitura, e perdia 82% das vezes.
        /// Com forca 5 e uma ação nula, vence 82% chegando com 75 de vida sem
        /// armadura, e 97% com o Colete de Retalhos que se acha neste mesmo local.
        /// Continua sendo, de longe, o combate mais difícil do jogo.
        /// </summary>
        public static Inimigo CriarViraLataAlfa() =>
            new("Vira-Lata Alfa", vidaMaxima: 65, forca: 5, defesa: 3, ataques: new[]
            {
                new AtaqueInimigo("Bote da Matilha", 16, "O Vira-Lata Alfa avança com tudo: {0} de dano!"),
                new AtaqueInimigo("Mordida Profunda", 12, "Uma mordida que não solta. {0} de dano."),
                new AtaqueInimigo("Uivo", 6, "O uivo ecoa pelo armazém e te desconcentra. {0} de dano."),
                new AtaqueInimigo("Sacudir o Pelo", 0,
                    "O Vira-Lata Alfa para pra se sacudir inteiro, espalhando poeira de tecido... nada aconteceu!",
                    ehAcaoNula: true),
            });

        /// <summary>Castelinho da Havan. Outro sobrevivente, não um monstro - dá
        /// pra fugir sem lutar (ver SessaoCombate.Fugir).</summary>
        public static Inimigo CriarSaqueador() =>
            new("Saqueador", vidaMaxima: 50, forca: 5, defesa: 2, ataques: new[]
            {
                new AtaqueInimigo("Golpe de Cano", 14, "O Saqueador acerta o cano no seu ombro: {0} de dano."),
                new AtaqueInimigo("Empurrão", 7, "Ele te empurra contra a prateleira. {0} de dano."),
                new AtaqueInimigo("Xingamento", 0,
                    "O Saqueador grita um palavrão criativo e cospe no chão... nada aconteceu!",
                    ehAcaoNula: true),
            },
            itemDrop: new Consumivel("Barra de Cereal",
                "Do estoque dele. Ele não vai sentir falta.", 1, 'c', cura: 10, restauraFome: 18));

        /// <summary>Museu da Cerveja. Piada de propósito (tom Earthbound) - dano
        /// baixinho, mas irritante.</summary>
        public static Inimigo CriarEnxameDeMosquitos() =>
            new("Enxame de Mosquitos", vidaMaxima: 18, forca: 1, defesa: 0, ataques: new[]
            {
                new AtaqueInimigo("Zumbido Infernal", 3, "O zumbido entra na sua orelha e você se bate sozinho: {0} de dano."),
                new AtaqueInimigo("Picada", 5, "Uma nuvem de picadas. {0} de dano."),
                new AtaqueInimigo("Voar em Círculos", 0,
                    "O enxame se distrai com um barril e fica voando em círculos... nada aconteceu!",
                    ehAcaoNula: true),
            });

        /// <summary>Arte .xp do rato (único inimigo com arte pronta no projeto).
        /// Os outros ainda não têm - o CombateScreen aceita arte nula e só mostra
        /// nome + HP, então eles funcionam normal até alguém desenhar.</summary>
        public static ScreenSurface CarregarArteRato() =>
            ArteUtils.CarregarArteInimigo("Artes/Inimigos/ratoselvagem.xp");
        public static ScreenSurface CarregarArteCao() =>
            ArteUtils.CarregarArteInimigo("Artes/Inimigos/CaoAssilvestrado.xp");
        public static ScreenSurface CarregarArteCaoAlfa() =>
            ArteUtils.CarregarArteInimigo("Artes/Inimigos/ViraLataAlfa.xp");
        public static ScreenSurface CarregarArteSaqueador() =>
            ArteUtils.CarregarArteInimigo("Artes/Inimigos/saqueador.xp");
        public static ScreenSurface CarregarArteEnxame() =>
            ArteUtils.CarregarArteInimigo("Artes/Inimigos/enxamedemosquitos.xp");
    }
}
