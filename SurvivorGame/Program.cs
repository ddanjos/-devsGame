using SadConsole;
using SadConsole.Configuration;
using SadRogue.Primitives;
using SurvivorGame.Cenarios;
using SurvivorGame.Mapa;
using SurvivorGame.Ui;
using SurvivorGame.Audio;
using SurvivorGame.Utilitarios;

namespace SurvivorGame
{
    public class Program
    {
        private static IMapa? _terreno;
        private static MapaJogo? _itensNoChao;
        private static MapaInimigos? _inimigosNoMapa;
        private static Personagem? _personagem;

        private static int _larguraJanela;
        private static int _alturaJanela;

        /// <summary>Tamanho da janela em CÉLULAS (não pixels). Outras telas usam
        /// isto - em vez de Game.Instance.ScreenCellsX/Y - pra saber o tamanho
        /// da janela: desde que passamos a abrir a janela com
        /// SetWindowSizeInPixels (ver AjusteVisual.TamanhoCelula, abaixo),
        /// ScreenCellsX/Y deixou de bater com essas variáveis, porque o
        /// SadConsole calcula aquilo com a fonte NATIVA (8x16), não com a
        /// célula 16x16 que a gente realmente usa pra desenhar.</summary>
        public static int LarguraJanela => _larguraJanela;
        public static int AlturaJanela => _alturaJanela;

        public static void Main(string[] args)
        {
            _terreno = new MapaCidadeBlumenau();

            // A janela precisa caber o maior cenário do jogo, não só a cidade: os
            // mapas de interior desenhados pelo Lindomar em REXPaint são 60x60
            // (Artes/Cenarios/*.xp), mais altos que os 45 da cidade. Sem isso, o
            // andar 0 (que tem a saída lá embaixo, perto da linha 59) ficaria
            // cortado fora da janela.
            _larguraJanela = Math.Max(_terreno.Largura, 60);
            _alturaJanela = Math.Max(_terreno.Altura, 60);

            // Antes isto era SetWindowSizeInCells(_larguraJanela, _alturaJanela),
            // que dimensiona a janela usando a fonte NATIVA do SadConsole
            // (8x16 pixels por célula - bem mais alta que larga). É essa
            // proporção que deixava os cenários e sprites .xp achatados na
            // horizontal. Agora abrimos a janela já do tamanho certo pra
            // célula QUADRADA que o resto do jogo usa (ver
            // AjusteVisual.CorrigirProporcaoDeCelula, chamado em toda tela) -
            // a contagem de células continua sendo _larguraJanela x
            // _alturaJanela, só o pixel de cada uma que fica maior e mais
            // largo.
            Builder startup = new Builder()
                .SetWindowSizeInPixels(
                    _larguraJanela * AjusteVisual.TamanhoCelula.X,
                    _alturaJanela * AjusteVisual.TamanhoCelula.Y)
                .OnStart(Game_Started);
            System.Console.OutputEncoding = System.Text.Encoding.UTF8;
            Game.Create(startup);
            Game.Instance.Run();
            Game.Instance.Dispose();
        }

        private static void Game_Started(object? sender, GameHost host)
        {
            // Áudio depois do host do MonoGame subir - antes disso não existe
            // dispositivo pra criar SoundEffect. Se a máquina não tiver som, o
            // GerenciadorSom se marca indisponível e tudo vira no-op.
            SurvivorGame.Regras.Configuracao.Carregar();
            GerenciadorSom.Iniciar();
            GerenciadorSom.TocarTrilha(Trilha.Exploracao);

            MostrarMenuPrincipal();
        }

        public static void MostrarMenuPrincipal()
        {
            var menuScreen = new MenuPrincipalScreen(_larguraJanela, _alturaJanela);
            Game.Instance.Screen = menuScreen;
            Game.Instance.Screen.IsFocused = true;
        }

        /// <summary>Carrega a partida gravada (SCRUM-11) e devolve direto pro mapa.
        /// Devolve false se não houver save legível - aí o menu avisa e o jogador
        /// escolhe "Novo Jogo" em vez de cair numa tela quebrada.</summary>
        public static bool CarregarPartida()
        {
            // Zera ANTES de ler: o SaveJogo reatribui as flags que conhece, mas se
            // alguém adicionar um campo novo ao GerenciadorJogo e esquecer de
            // incluí-lo no SaveDados, sem este Reiniciar ele vazaria da partida
            // anterior em silêncio.
            global::SurvivorGame.Regras.GerenciadorJogo.Reiniciar();
            FabricaLocais.Reiniciar();

            Personagem? salvo = SurvivorGame.Regras.SaveJogo.Carregar();
            if (salvo is null) return false;

            // Mundo novo em folha: terreno, itens no chão e inimigos não guardam
            // progresso, então não vão pro arquivo - mas também não podem sobrar da
            // partida anterior (um item largado no chão em outro jogo continuaria lá).
            _terreno = new MapaCidadeBlumenau();
            _itensNoChao = new MapaJogo();
            _inimigosNoMapa = new MapaInimigos();

            // O GerenciadorJogo já foi restaurado dentro do Carregar(); aqui só
            // reconstruímos o mundo (terreno, itens no chão, inimigos) do zero -
            // eles não guardam progresso, então não precisam ir pro arquivo.
            _personagem = salvo;
            AbrirMapa();
            return true;
        }

        public static void IniciarNovaPartida()
        {
            // Partida nova: zera o progresso da missão (as 3 peças do rádio) e o
            // estado guardado dos locais. Sem isso, rodar o jogo de novo na mesma
            // execução herdaria as peças da partida anterior.
            global::SurvivorGame.Regras.GerenciadorJogo.Reiniciar();
            FabricaLocais.Reiniciar();

            _terreno = new MapaCidadeBlumenau();
            _itensNoChao = new MapaJogo();
            _inimigosNoMapa = new MapaInimigos();

            Point entrada = _terreno.PontoEntrada;
            _personagem = new Personagem("Sobrevivente", entrada.X, entrada.Y);

            if (_terreno is null || _personagem is null || _itensNoChao is null || _inimigosNoMapa is null)
                return;

            AbrirMapa();
        }

        /// <summary>Monta o mundo (se ainda não existir) e mostra o mapa da cidade.
        /// Compartilhado por "Novo Jogo" e "Continuar" - a diferença entre os dois
        /// é só de onde vem o Personagem.</summary>
        private static void AbrirMapa()
        {
            _terreno ??= new MapaCidadeBlumenau();
            _itensNoChao ??= new MapaJogo();
            _inimigosNoMapa ??= new MapaInimigos();

            if (_personagem is null) return;

            GerenciadorSom.TocarTrilha(Trilha.Exploracao);

            var mapaScreen = new MapaScreen(_terreno, _itensNoChao, _inimigosNoMapa, _personagem);
            Game.Instance.Screen = mapaScreen;
            Game.Instance.Screen.IsFocused = true;
        }

        // REMOVIDO: antes plantávamos um "Rato Selvagem" decorativo a três células
        // do ponto de partida, herdado de quando o mapa da cidade era o único lugar
        // com combate. Ele quebrava a missão principal: GerenciadorJogo concede a
        // Antena a QUALQUER vitória contra um inimigo com "Rato" no nome, então
        // derrotar esse rato da rua entregava a peça e deixava o conteúdo do andar 0
        // (o ninho atrás do balcão, ver Mapa/LocalAndarZero) sem função nenhuma -
        // ele passava a responder "o ninho está vazio agora" antes do jogador ter
        // ido lá. Os encontros agora acontecem dentro dos locais, pelas ações.
    }
}
