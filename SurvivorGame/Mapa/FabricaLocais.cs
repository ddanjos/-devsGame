using System;
using System.Collections.Generic;
using SurvivorGame.Combate;
using SurvivorGame.Inventario;
using SurvivorGame.Regras;

namespace SurvivorGame.Mapa
{
    /// <summary>
    /// Factory que transforma um LocalMapa (o ponto clicável no mapa da cidade -
    /// só nome, posição e descrição) no ILocalExploravel jogável correspondente,
    /// com as ações de ponto-e-clique de cada lugar.
    ///
    /// Mesmo padrão do TileFactory, e pelo mesmo motivo: o MapaScreen não precisa
    /// saber COMO cada local é montado, só pedir "me dá o local chamado X". Sem
    /// isso, o EntrarEm viraria um if/else gigante com o conteúdo do jogo inteiro
    /// dentro de uma tela.
    ///
    /// Os textos vêm do documento de narrativa (HISTORIA.md) - cada local tem um
    /// "gancho" pra dar motivo do jogador ir lá, e um tipo: combate, descoberta,
    /// descanso ou peça-chave da missão principal.
    /// </summary>
    internal static class FabricaLocais
    {
        /// <summary>
        /// Devolve o local jogável correspondente ao nome, ou null se aquele ponto
        /// ainda não tiver conteúdo (aí o MapaScreen cai na telinha de descrição
        /// simples de sempre). Cria uma instância NOVA a cada chamada de propósito
        /// nos locais com sorteio, pra cada visita ser uma tentativa nova - menos a
        /// Catedral, que precisa lembrar quantas vezes o sino já tocou.
        /// </summary>
        public static ILocalExploravel? Criar(string nomeDoLocal)
        {
            return nomeDoLocal switch
            {
                "ProWay" => new LocalEscritorioProway(),
                "Prefeitura Municipal de Blumenau" => CriarPrefeitura(),
                "Museu Hering" => CriarMuseuHering(),
                "Castelinho da Havan" => CriarCastelinho(),
                "Parque Sao Francisco de Assis" => CriarParqueSaoFrancisco(),
                "Parque Ramiro Ruediger" => CriarParqueRamiro(),
                "Catedral Sao Paulo Apostolo" => _catedral ??= new LocalCatedral(),
                "Museu da Cerveja de Blumenau" => CriarMuseuDaCerveja(),
                "Museu de Habitos e Costumes" => CriarMuseuHabitos(),
                "Museu da Familia Colonial" => CriarMuseuFamiliaColonial(),
                "Mausoleu Dr. Blumenau" => CriarMausoleu(),
                _ => null
            };
        }

        /// <summary>A Catedral é a única guardada entre visitas - o easter egg dela
        /// conta quantas vezes o sino tocou, e isso se perderia se ela fosse
        /// recriada toda vez que o jogador entrasse.</summary>
        private static LocalCatedral? _catedral;

        /// <summary>Zera o estado guardado. Chamado junto de GerenciadorJogo.Reiniciar
        /// ao começar uma partida nova.</summary>
        public static void Reiniciar() => _catedral = null;

        /// <summary>Ação de sair que todo local da cidade tem - volta pro mapa.</summary>
        private static AcaoLocal VoltarPraRua() =>
            new("Voltar pra rua", custoFome: 0, custoSede: 0,
                _ => new ResultadoAcao { VoltarParaAnterior = true });

        // ------------------------------------------------------------------
        // PEÇAS-CHAVE DA MISSÃO
        // ------------------------------------------------------------------

        /// <summary>Prefeitura - guarda o FUSÍVEL. Sem combate, só risco ambiental
        /// leve: é o mais fácil das três peças de propósito, pra dar ritmo (nem
        /// tudo precisa ser luta).</summary>
        private static ILocalExploravel CriarPrefeitura() => new LocalCidade(
            "Prefeitura Municipal de Blumenau",
            "O prédio enxaimel ainda de pé, orgulhoso, enquanto tudo ao redor caiu. " +
            "Uma sala de manutenção nos fundos guarda o painel elétrico que mantinha " +
            "a cidade inteira funcionando.",
            new[]
            {
                new AcaoLocal("Vasculhar a sala de manutenção", custoFome: 8, custoSede: 8, jogador =>
                {
                    if (GerenciadorJogo.TemFusivel)
                        return new ResultadoAcao { Mensagem = "Você já levou o que servia daqui. O resto do painel está torrado." };

                    // Risco ambiental: o painel ainda tem energia residual. Custa
                    // vida, mas não é combate - variedade de desafio.
                    jogador.ReceberDanoDireto(8);
                    GerenciadorJogo.TemFusivel = true;

                    return new ResultadoAcao
                    {
                        Mensagem = $"Um estalo azul salta do painel e queima sua mão (-8 de vida) - mas você consegue arrancar o Fusível industrial inteiro! [PEÇA {GerenciadorJogo.PecasEncontradas}/3]"
                    };
                }),

                new AcaoLocal("Procurar suprimentos nas salas", custoFome: 5, custoSede: 5, jogador =>
                {
                    if (Random.Shared.Next(100) < 50)
                    {
                        var agua = new Consumivel("Garrafa de Água", "Água engarrafada, ainda lacrada.", 1, 'u',
                            cura: 5, restauraSede: 25);
                        bool coletou = jogador.Inventario.AdicionarItem(agua);
                        return new ResultadoAcao
                        {
                            Mensagem = coletou
                                ? "Numa copa esquecida, uma caixa de garrafas de água lacradas!"
                                : "Tem água aqui, mas sua mochila está cheia."
                        };
                    }

                    return new ResultadoAcao { Mensagem = "Só papelada molhada e gavetas vazias." };
                }),

                VoltarPraRua(),
            });

        /// <summary>Museu Hering - guarda a BATERIA, protegida pelo Vira-Lata Alfa.
        /// O mais difícil das três peças, de propósito.</summary>
        private static ILocalExploravel CriarMuseuHering() => new LocalCidade(
            "Museu Hering",
            "Fardos de tecido viraram ninho pra alguma coisa que já não é bem cachorro. " +
            "No armazém dos fundos, entre máquinas de costura enferrujadas, uma bateria " +
            "industrial ainda pisca uma luzinha verde fraca.",
            new[]
            {
                new AcaoLocal("Entrar no armazém dos fundos", custoFome: 8, custoSede: 8, jogador =>
                {
                    if (GerenciadorJogo.TemBateria)
                        return new ResultadoAcao { Mensagem = "O armazém está quieto agora. A bateria já é sua." };

                    return new ResultadoAcao
                    {
                        Mensagem = "Um vira-lata enorme se levanta de cima dos fardos, rosnando. Ele não vai deixar você chegar perto da bateria.",
                        IniciarCombateCom = FabricaInimigos.CriarViraLataAlfa()
                    };
                }),

                new AcaoLocal("Revirar as prateleiras de tecido", custoFome: 4, custoSede: 4, jogador =>
                {
                    var pano = new Armadura("Colete de Retalhos",
                        "Camadas de tecido costuradas às pressas. Não é bonito, mas segura.", 1, defesa: 3, simbolo: 'a');
                    bool coletou = jogador.Inventario.AdicionarItem(pano);
                    return new ResultadoAcao
                    {
                        Mensagem = coletou
                            ? "Você improvisa um colete com os retalhos mais grossos. (Equipe pelo inventário!)"
                            : "Dava pra fazer um colete com isso, mas a mochila está cheia."
                    };
                }),

                VoltarPraRua(),
            });

        // ------------------------------------------------------------------
        // COMBATE
        // ------------------------------------------------------------------

        /// <summary>Castelinho da Havan - o Saqueador é outro sobrevivente, não um
        /// monstro. Dá pra ir embora sem lutar.</summary>
        private static ILocalExploravel CriarCastelinho() => new LocalCidade(
            "Castelinho da Havan",
            "De longe parece um castelo de verdade. De perto, as vitrines quebradas e as " +
            "prateleiras reviradas contam outra história - alguém mais chegou aqui " +
            "primeiro, e não quer dividir.",
            new[]
            {
                new AcaoLocal("Entrar mesmo assim", custoFome: 6, custoSede: 6, jogador =>
                    new ResultadoAcao
                    {
                        Mensagem = "Um homem magro sai de trás de uma prateleira com um cano na mão. \"Esse aqui é meu\", ele diz.",
                        IniciarCombateCom = FabricaInimigos.CriarSaqueador()
                    }),

                new AcaoLocal("Chamar e tentar conversar", custoFome: 3, custoSede: 3, jogador =>
                {
                    // Recompensa por não partir pra briga - variedade de solução.
                    var lata = new Consumivel("Ração Trocada", "Ganha de um estranho. Vale mais do que parece.", 1, 'c',
                        cura: 20, restauraFome: 20);
                    bool coletou = jogador.Inventario.AdicionarItem(lata);

                    return new ResultadoAcao
                    {
                        Mensagem = coletou
                            ? "Ele te encara por um tempo longo demais. Depois joga uma lata na sua direção: \"Pega e vai embora.\" Você não discute."
                            : "Ele te oferece uma lata, mas sua mochila está cheia. Ele dá de ombros e volta pras sombras."
                    };
                }),

                VoltarPraRua(),
            });

        /// <summary>Parque São Francisco - primeiro combate "de verdade" fora da
        /// ProWay, com o inimigo mais simples do roster.</summary>
        private static ILocalExploravel CriarParqueSaoFrancisco() => new LocalCidade(
            "Parque São Francisco de Assis",
            "Sem gente pra podar, o parque cresceu rápido demais. E não veio sozinho: " +
            "uma matilha de cães assilvestrados decidiu que esse pedaço de mato agora " +
            "é território deles.",
            new[]
            {
                new AcaoLocal("Atravessar o mato fechado", custoFome: 6, custoSede: 6, jogador =>
                {
                    if (Random.Shared.Next(100) < 70)
                    {
                        return new ResultadoAcao
                        {
                            Mensagem = "Um cão magro salta do meio do mato, dentes à mostra!",
                            IniciarCombateCom = FabricaInimigos.CriarCaoAssilvestrado()
                        };
                    }

                    var fruta = new Consumivel("Fruta do Mato", "Pequena e azeda, mas é comida.", 1, 'c',
                        cura: 8, restauraFome: 15);
                    bool coletou = jogador.Inventario.AdicionarItem(fruta);
                    return new ResultadoAcao
                    {
                        Mensagem = coletou
                            ? "Você atravessa sem ser notado e ainda acha um pé de fruta carregado."
                            : "Você atravessa sem ser notado. Tem fruta aqui, mas a mochila está cheia."
                    };
                }),

                VoltarPraRua(),
            });

        // ------------------------------------------------------------------
        // DESCANSO
        // ------------------------------------------------------------------

        /// <summary>Parque Ramiro Ruediger - zona segura de propósito, sem nenhum
        /// inimigo. A fonte restaura Sede de graça: é o respiro do ritmo do jogo,
        /// pra quem saiu machucado de um combate puxado.</summary>
        private static ILocalExploravel CriarParqueRamiro() => new LocalCidade(
            "Parque Ramiro Ruediger",
            "O maior parque da cidade, e o mais silencioso. No meio da pista de corrida " +
            "abandonada, um bebedouro público ainda jorra água limpa - ninguém sabe " +
            "explicar por quê, e ninguém está reclamando.",
            new[]
            {
                new AcaoLocal("Beber na fonte", custoFome: 0, custoSede: 0, jogador =>
                {
                    jogador.RestaurarSede(40);
                    return new ResultadoAcao { Mensagem = "Água fria, limpa, sem gosto de nada. Você bebe até não conseguir mais. (+40 de Sede)" };
                }),

                new AcaoLocal("Dormir algumas horas na grama", custoFome: 10, custoSede: 5, jogador =>
                {
                    jogador.Curar(35);
                    return new ResultadoAcao { Mensagem = "Você dorme de verdade pela primeira vez em dias e acorda inteiro. (+35 de vida, mas dá fome)" };
                }),

                VoltarPraRua(),
            });

        // ------------------------------------------------------------------
        // DESCOBERTA / LORE
        // ------------------------------------------------------------------

        /// <summary>Museu da Cerveja - tom cômico de propósito (referência Earthbound
        /// que o time já tinha definido). O enxame é fraco e ridículo.</summary>
        private static ILocalExploravel CriarMuseuDaCerveja() => new LocalCidade(
            "Museu da Cerveja de Blumenau",
            "O cheiro de malte fermentado ainda impregna o ar, anos depois. E onde tem " +
            "fermentação abandonada, tem enxame - um zumbido alto vem de dentro dos barris.",
            new[]
            {
                new AcaoLocal("Abrir um dos barris", custoFome: 4, custoSede: 4, jogador =>
                    new ResultadoAcao
                    {
                        Mensagem = "Você abre o barril. Uma nuvem zumbindo sai de dentro, furiosa e minúscula.",
                        IniciarCombateCom = FabricaInimigos.CriarEnxameDeMosquitos()
                    }),

                new AcaoLocal("Ler os painéis da exposição", custoFome: 0, custoSede: 0, jogador =>
                    new ResultadoAcao
                    {
                        Mensagem = "\"A cerveja acompanha Blumenau desde os primeiros imigrantes.\" Alguém riscou embaixo, com caneta: \"e vai sobreviver a todos nós\"."
                    }),

                VoltarPraRua(),
            });

        /// <summary>Museu de Hábitos e Costumes - descoberta garantida, sem sorteio.
        /// Dá previsibilidade num jogo cheio de aleatoriedade.</summary>
        private static ILocalExploravel CriarMuseuHabitos() => new LocalCidade(
            "Museu de Hábitos e Costumes",
            "Vitrines com objetos do dia a dia de gente que morreu há mais de um século - " +
            "e do lado delas, sobras de comida enlatada de gente que sumiu há só algumas semanas.",
            new[]
            {
                new AcaoLocal("Recolher as latas deixadas", custoFome: 3, custoSede: 3, jogador =>
                {
                    var conserva = new Consumivel("Conserva de Museu",
                        "Enlatada, fora da validade, mas intacta.", 1, 'c',
                        cura: 18, restauraFome: 25);
                    bool coletou = jogador.Inventario.AdicionarItem(conserva);
                    return new ResultadoAcao
                    {
                        Mensagem = coletou
                            ? "Quem passou por aqui deixou comida pra trás com pressa. Boa pra você."
                            : "Tem comida aqui, mas sua mochila está cheia."
                    };
                }),

                VoltarPraRua(),
            });

        /// <summary>Museu da Família Colonial - puro lore sobre o Evento. Sem custo:
        /// é leitura, não esforço físico.</summary>
        private static ILocalExploravel CriarMuseuFamiliaColonial() => new LocalCidade(
            "Museu da Família Colonial",
            "Um diário na estante ainda está aberto na última página escrita. Não é sobre " +
            "os colonizadores - é de alguém que se escondeu aqui nos primeiros dias do " +
            "Evento, tentando entender o que via pela janela.",
            new[]
            {
                new AcaoLocal("Ler o diário", custoFome: 0, custoSede: 0, jogador =>
                    new ResultadoAcao
                    {
                        Mensagem = "\"Terceiro dia. O rádio não pega mais nada, nem chiado. Vi um grupo descendo a XV de Novembro de manhã, mas não paravam pra ninguém. Se alguém ler isso: tem uma torre de transmissão no centro. Alguém precisa fazer ela funcionar.\""
                    }),

                VoltarPraRua(),
            });

        /// <summary>Mausoléu - tensão social sem combate: planta a ideia de que
        /// existem outros sobreviventes, sem nunca mostrar nenhum.</summary>
        private static ILocalExploravel CriarMausoleu() => new LocalCidade(
            "Mausoléu Dr. Blumenau",
            "Alguém deixou flores frescas na porta do mausoléu - frescas de verdade, de " +
            "dias atrás, não de décadas. Tem mais alguém vivo nessa cidade, e visita " +
            "esse lugar.",
            new[]
            {
                new AcaoLocal("Examinar as flores", custoFome: 0, custoSede: 0, jogador =>
                    new ResultadoAcao
                    {
                        Mensagem = "Colhidas há dois, três dias no máximo. Quem fez isso ainda está por aqui - e sabe se esconder melhor que você."
                    }),

                VoltarPraRua(),
            });
    }
}
