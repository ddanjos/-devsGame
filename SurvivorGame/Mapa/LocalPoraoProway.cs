using System;
using System.Collections.Generic;
using System.Linq;
using SurvivorGame.Inventario;

namespace SurvivorGame.Mapa
{
    /// <summary>
    /// Versão ponto-e-clique (mínima, ainda sem arte própria) do porão da ProWay -
    /// antes era MapaMasmorra, andável, acessada pela escada do escritório. Ainda
    /// não convertemos o conteúdo da masmorra pra esse novo formato (fica pra uma
    /// próxima rodada); por enquanto é só o elo da cadeia (escritório -> porão)
    /// pra não deixar a escada sem destino nenhum.
    /// </summary>
    internal class LocalPoraoProway : ILocalExploravel
    {
        public string Nome => "Porão da ProWay";

        public string Descricao =>
            "Escuro e úmido. Caixas empilhadas contra a parede, algumas já mofadas. " +
            "Dá pra ouvir um gotejar distante.";

        public IReadOnlyList<AcaoLocal> Acoes { get; }

        private readonly ILocalExploravel _escritorio;

        public LocalPoraoProway(ILocalExploravel escritorio)
        {
            _escritorio = escritorio;

            Acoes = new[]
            {
                new AcaoLocal("Vasculhar as caixas", custoFome: 0, custoSede: 5, VasculharCaixas),

                new AcaoLocal("Subir de volta pro escritório", custoFome: 0, custoSede: 0, jogador =>
                    new ResultadoAcao
                    {
                        Mensagem = "Você sobe de volta pro escritório.",
                        NovoLocal = _escritorio
                    }),
            };
        }

        /// <summary>Aqui, e não num local de combate, é de propósito: o porão não
        /// tem nenhum inimigo (é só o elo escritório->porão), então é um lugar
        /// seguro pro jogador sair armado ANTES de encarar qualquer bicho lá fora -
        /// inclusive o Rato do Andar 0. Dano 16 fica acima do desarmado (10) sem
        /// tornar o Vira-Lata Alfa trivial (ver contas em FabricaInimigos). 40% de
        /// achar, igual ao padrão de sorteio já usado em ProcurarComida - não acha
        /// sempre, mas também não é raro a ponto de travar quem depende disso.</summary>
        private static ResultadoAcao VasculharCaixas(Personagem jogador)
        {
            if (jogador.Inventario.Itens.Any(i => i is Arma) || jogador.ArmaEquipada is not null)
                return new ResultadoAcao { Mensagem = "Só caixas vazias e mofo por aqui. O resto você já pegou." };

            if (Random.Shared.Next(100) < 40)
            {
                var cano = new Arma("Cano de Ferro", "Um pedaço de cano pesado, dá pra bater com força.", 1, dano: 16, simbolo: '/');
                bool coletou = jogador.Inventario.AdicionarItem(cano);

                return new ResultadoAcao
                {
                    Mensagem = coletou
                        ? "Debaixo de uma caixa mofada, um cano de ferro pesado. Melhor que ir com as mãos vazias. (Equipe pelo inventário!)"
                        : "Tem um cano de ferro aqui, mas sua mochila está cheia."
                };
        }

            return new ResultadoAcao { Mensagem = "Só caixas vazias e mofo. Nada de útil por aqui - ainda." };
        }
    }
}
