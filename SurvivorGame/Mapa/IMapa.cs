using System;
using System.Collections.Generic;
using SadConsole;
using SadRogue.Primitives;

namespace SurvivorGame.Mapa
{
    /// <summary>
    /// Contrato comum a qualquer mapa de TERRENO do jogo (a masmorra do desenho, a
    /// cidade de Blumenau, ou qualquer mapa futuro). Isso é diferente do MapaJogo
    /// de vocês, que guarda os itens largados no chão - IMapa é só o terreno
    /// (paredes, chão, água, ruas...). Program.cs usa os dois juntos.
    ///
    /// Strategy pattern: quem consome um IMapa não precisa saber qual implementação
    /// está ativa, só que ele sabe se desenhar e informar se uma posição é bloqueada.
    /// </summary>
    internal interface IMapa
    {
        int Largura { get; }
        int Altura { get; }
        Point PontoEntrada { get; }

        bool EhBloqueado(int x, int y);
        void DesenharEm(ScreenSurface superficie);

        /// <summary>
        /// Tile na posição (x, y). Já existia como método público em
        /// MapaCidadeBlumenau/MapaMasmorra; subiu pra interface porque o
        /// ExploracaoScreen (novo) precisa saber o TIPO da célula onde o jogador
        /// pisou, não só se ela bloqueia passagem (ex: pisou num elevador?).
        /// </summary>
        Tile ObterTile(int x, int y);

        /// <summary>
        /// Se (x, y) for um gatilho de transição pra OUTRO mapa (ex: o elevador do
        /// escritório leva pro andar 0), devolve o IMapa de destino; caso contrário
        /// null. Tem implementação padrão porque a maioria dos mapas (cidade,
        /// masmorra) não tem transição nenhuma - só os mapas de interior
        /// encadeados (MapaEscritorioProway) precisam sobrescrever isso.
        /// </summary>
        IMapa? MapaDestino(int x, int y) => null;

        /// <summary>
        /// Caminho de um arquivo .xp (REXPaint) pra mostrar como uma "tela de
        /// entrada" antes de liberar o movimento (ver ExploracaoScreen). null = sem
        /// arte, entra direto no mapa. Implementação padrão null pelo mesmo motivo
        /// de MapaDestino acima.
        /// </summary>
        string? CaminhoArte => null;

        /// <summary>
        /// Dica curta mostrada assim que o jogador entra neste mapa, pra ele
        /// aprender jogando sem precisar de manual - ex: "ande até o indicador
        /// azul pra chamar o elevador". null = sem dica (mapas onde o objetivo já
        /// é óbvio, como a cidade). Ver ExploracaoScreen.Redesenhar.
        /// </summary>
        string? Dica => null;

        /// <summary>
        /// Pontos especiais deste mapa que reagem à tecla E quando o jogador chega
        /// perto - elevador, escada, saída pro andar de cima etc. Cada item tem a
        /// posição e um texto curto pro prompt (ex: "chamar o elevador"). Isso
        /// generaliza, pros gatilhos de DENTRO de um interior, o mesmo padrão
        /// "aproximou -> apareceu prompt -> aperta E" que já existia só pra entrar
        /// nos Locais do mapa da cidade (ver MapaScreen.AtualizarLocalProximo).
        /// Antes disso, cada saída/elevador só funcionava pisando exatamente numa
        /// única célula - fácil de errar por um pixel e, com corredores cheios de
        /// móveis/entulho parecidos com parede, dava a sensação de labirinto sem
        /// saída visível. Implementação padrão vazia porque a maioria dos mapas
        /// (cidade, masmorra) não tem esse tipo de gatilho. Ver
        /// ExploracaoScreen.AtualizarPontoProximo/InteragirComPontoProximo.
        /// </summary>
        IReadOnlyList<(Point Posicao, string Rotulo)> PontosInteresse => Array.Empty<(Point, string)>();
    }
}
