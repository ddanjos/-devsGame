using System.Collections.Generic;
using System.Linq;
using System.Text;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using SurvivorGame.Combate;
using SurvivorGame.Mapa;
using SurvivorGame.UI;
using SurvivorGame.Utilitarios;

namespace SurvivorGame.Cenarios
{
    /// <summary>
    /// Tela ponto-e-clique de um ILocalExploravel - o formato que o SCRUM-9
    /// ("Sistema de Mapa") pede de verdade: arte do local + nome + descrição em
    /// cima, botões de ação embaixo (setas pra navegar, Enter pra confirmar - o
    /// "clicar" do ticket, adaptado pro mesmo padrão de menu por teclado que já
    /// usamos em CombateScreen, pra manter a UI consistente). Cada ação gasta Fome/
    /// Sede do jogador (mesmo que o resultado seja "não achou nada"), e pode trocar
    /// o local atual (mostrado nesta mesma tela, sem trocar de Screen), dar item
    /// direto no inventário, ou puxar um combate.
    ///
    /// Substitui, pros locais que forem migrados, o antigo ExploracaoScreen (andar
    /// de verdade + colisão por pixel) - que continua existindo no projeto pros
    /// locais ainda não migrados, mas deixou de ser o padrão daqui pra frente.
    /// Ver Mapa/ILocalExploravel, Mapa/AcaoLocal, Mapa/ResultadoAcao.
    ///
    /// 'I' abre o inventário e 'ESC' volta pra tela anterior - "botões fixos" que
    /// NÃO gastam Fome/Sede, exatamente como o SCRUM-9 pede.
    /// </summary>
    internal class LocalExploravelScreen : ScreenSurface
    {
        private readonly Personagem _jogador;
        private readonly IScreenObject _telaAnterior;

        // Sempre atribuído no construtor via TrocarLocal antes de qualquer outro
        // método rodar - o '= null!' só existe pra calar o aviso de nullability,
        // já que o compilador não enxerga através da chamada de método.
        private ILocalExploravel _local = null!;
        // ALTERADO: Agora será uma ScreenSurface visual filha
        private ScreenSurface? _arteAtual;
        private int _indiceSelecionado;

        private string _mensagem = string.Empty;

        public LocalExploravelScreen(ILocalExploravel local, Personagem jogador, IScreenObject telaAnterior,
            int largura, int altura)
            : base(largura, altura)
        {
            _jogador = jogador;
            _telaAnterior = telaAnterior;

            UseKeyboard = true;

            // ORDEM IMPORTA: TrocarLocal precisa vir ANTES de IsFocused.
            // Atribuir IsFocused dispara OnFocused(), que redesenha a tela lendo
            // _local - e se _local ainda estiver null, o construtor lança
            // NullReferenceException e o jogo fecha sozinho ao entrar num local.
            // Foi exatamente o que aconteceu quando o OnFocused foi adicionado.
            TrocarLocal(local);

            IsFocused = true;
        }

        /// <summary>Redesenha ao reganhar o foco - é o que acontece ao voltar do
        /// combate ou do inventário. Sem isso a tela ficava com os dados de ANTES
        /// (ex: sair de uma luta com 18 de vida e a tela ainda mostrando 90), e o
        /// jogador escolhia a próxima ação com base em números falsos.
        ///
        /// A guarda de null é proposital: este método é virtual e o SadConsole pode
        /// chamá-lo durante a construção do objeto, quando os campos ainda não
        /// foram preenchidos. Sem ela, a ordem do construtor vira uma armadilha
        /// pra quem mexer aqui depois.</summary>
        public override void OnFocused()
        {
            base.OnFocused();

            if (_local is null) return;
            Redesenhar();
        }

        public override bool ProcessKeyboard(Keyboard keyboard)
        {
            if (keyboard.IsKeyPressed(Keys.I))
            {
                Game.Instance.Screen = new InventarioScreen(_jogador, this, Width, Height);
                Game.Instance.Screen.IsFocused = true;
                return true;
            }

            if (keyboard.IsKeyPressed(Keys.Escape))
            {
                Game.Instance.Screen = _telaAnterior;
                Game.Instance.Screen!.IsFocused = true;
                return true;
            }

            IReadOnlyList<AcaoLocal> acoes = _local.Acoes;
            if (acoes.Count == 0)
                return true;

            if (keyboard.IsKeyPressed(Keys.Down))
            {
                _indiceSelecionado = (_indiceSelecionado + 1) % acoes.Count;
                Redesenhar();
            }
            else if (keyboard.IsKeyPressed(Keys.Up))
            {
                _indiceSelecionado = (_indiceSelecionado - 1 + acoes.Count) % acoes.Count;
                Redesenhar();
            }
            else if (keyboard.IsKeyPressed(Keys.Enter))
            {
                ExecutarAcaoSelecionada(acoes[_indiceSelecionado]);
            }

            return true;
        }

        /// <summary>Gasta o custo de Fome/Sede da ação (sempre, mesmo se o
        /// resultado for "não achou nada" - é assim que o SCRUM-9 descreve), roda a
        /// própria ação, e reage ao resultado: combate abre uma nova tela; troca de
        /// local só atualiza o que é mostrado AQUI dentro (não empilha telas); ESC/
        /// voltar fecha a exploração.</summary>
        private void ExecutarAcaoSelecionada(AcaoLocal acao)
        {
            _jogador.ConsumirFome(acao.CustoFome);
            _jogador.ConsumirSede(acao.CustoSede);

            ResultadoAcao resultado = acao.Executar(_jogador);
            _mensagem = resultado.Mensagem ?? string.Empty;

            if (resultado.VenceuOJogo)
            {
                Game.Instance.Screen = new FimDeJogoScreen(
                    venceu: true, FimDeJogoScreen.TextoVitoria, Width, Height);
                Game.Instance.Screen.IsFocused = true;
                return;
            }

            // Fome/Sede em 0 machucam a cada ação, igual acontece por rodada dentro
            // do combate (ver SessaoCombate.IniciarTurnoJogador) - senão o jogador
            // poderia explorar a cidade inteira com os dois zerados, sem
            // consequência nenhuma, e a mecânica de sobrevivência não valeria nada.
            string aviso = AplicarDesgaste();
            if (_jogador.Estado == EstadoPersonagem.Morto)
            {
                Game.Instance.Screen = new FimDeJogoScreen(
                    venceu: false, FimDeJogoScreen.TextoDerrotaInanicao, Width, Height);
                Game.Instance.Screen.IsFocused = true;
                return;
            }

            if (!string.IsNullOrEmpty(aviso))
                _mensagem = string.IsNullOrEmpty(_mensagem) ? aviso : $"{_mensagem}  {aviso}";

            if (resultado.IniciarCombateCom is not null)
            {
                var combate = new CombateScreen(_jogador, resultado.IniciarCombateCom, this, Width, Height, resultado.ArteInimigo);
                Game.Instance.Screen = combate;
                Game.Instance.Screen.IsFocused = true;
                return;
            }

            if (resultado.VoltarParaAnterior)
            {
                Game.Instance.Screen = _telaAnterior;
                Game.Instance.Screen!.IsFocused = true;
                return;
            }

            if (resultado.NovoLocal is not null)
            {
                TrocarLocal(resultado.NovoLocal);
                return;
            }

            Redesenhar();
        }

        /// <summary>Dano por Fome/Sede zerada, aplicado depois de cada ação.
        /// Devolve o aviso pro jogador, ou string vazia se ele estiver bem.</summary>
        private string AplicarDesgaste()
        {
            const int dano = 3;

            if (_jogador.Fome <= 0 && _jogador.Sede <= 0)
            {
                _jogador.ReceberDanoDireto(dano * 2);
                return $"[Sem comida e sem água: -{dano * 2} de vida!]";
            }

            if (_jogador.Fome <= 0)
            {
                _jogador.ReceberDanoDireto(dano);
                return $"[Passando fome: -{dano} de vida!]";
            }

            if (_jogador.Sede <= 0)
            {
                _jogador.ReceberDanoDireto(dano);
                return $"[Desidratado: -{dano} de vida!]";
            }

            return string.Empty;
        }

        private void TrocarLocal(ILocalExploravel local)
        {
            _local = local;
            _indiceSelecionado = 0;

            // Se já existia uma arte desenhada na tela antes, remove ela dos filhos visuais
            if (_arteAtual is not null)
            {
                Children.Remove(_arteAtual);
                _arteAtual = null;
            }

            if (local.CaminhoArte is not null)
            {
                //
                // Criamos a superfície visual com o tamanho exato da arte do Lindomar
                // E forçamos ela a usar a fonte quadrada em tamanho 1 (ex: 8x8 pixels)
                if (local.CaminhoArte is not null)
                {
                    // Carrega a matriz de células do REXPaint
                    var dadosArte = ArteUtils.CarregarArteCenario(local.CaminhoArte);

                    // Criamos a superfície visual com o tamanho exato da arte do Lindomar
                    _arteAtual = new ScreenSurface(dadosArte.Width, dadosArte.Height);

                    // FORÇAR FONTE QUADRADA: Alteramos o tamanho de exibição em pixels de cada célula.
                    // Se a arte do REXPaint foi feita pensando em blocos de 8x8 pixels:
                    _arteAtual.FontSize = new Point(8, 8);

                    // NOTA: Se você achar que a imagem ficou muito pequena na tela, 
                    // você pode dobrar o tamanho dela mudando para 16x16 pixels assim:
                    // _arteAtual.FontSize = new Point(16, 16);

                    // Copia os dados puros do arquivo para a nossa nova superfície visual quadrada
                    dadosArte.Surface.Copy(_arteAtual.Surface, 0, 0);

                    // Adiciona essa superfície como filha desta tela para o SadConsole renderizá-la automaticamente
                    Children.Add(_arteAtual);
                }

            }

            Redesenhar();
        }


        private void Redesenhar()
        {
            Surface.Clear();

            int linha = 1;
            int alturaOcupadaPelaArte = 0;

            if (_arteAtual is not null)
            {
                // Calcula a posição centralizada horizontalmente na tela
                int posX = System.Math.Max(0, (Width / 2) - (_arteAtual.Width / 2));

                // Posiciona a camada quadrada no topo
                _arteAtual.Position = new Point(posX, linha);

                // Descobre exatamente quantas linhas a arte ocupa fisicamente na tela do jogo.
                // Como a tela principal usa uma fonte 8x16 (dobro da altura da arte 8x8),
                // dividimos a altura da arte por 2 para o texto começar logo abaixo dela.
                alturaOcupadaPelaArte = linha + (_arteAtual.Height / 2) + 1;
            }

            // Se tiver arte, o texto começa abaixo dela. Se não tiver, começa na linha 2.
            int linhaNome = alturaOcupadaPelaArte > 0 ? alturaOcupadaPelaArte : 2;
            Surface.Print(2, linhaNome, _local.Nome, Color.Gold, Color.Black);

            // A descrição começa duas linhas abaixo do nome
            int linhaDescricao = linhaNome + 2;
            foreach (string trecho in QuebrarLinhas(_local.Descricao, Width - 4))
            {
                Surface.Print(2, linhaDescricao, trecho, Color.White, Color.Black);
                linhaDescricao++;
            }

            // O status (HP, Fome, Sede) começa duas linhas abaixo do fim da descrição
            int linhaStatus = linhaDescricao + 1;
            Surface.Print(2, linhaStatus,
                $"HP: {_jogador.Vida}/{_jogador.VidaMaxima}   Fome: {_jogador.Fome}   Sede: {_jogador.Sede}",
                Color.LightGreen, Color.Black);

            // As ações começam duas linhas abaixo do status
            int linhaAcoes = linhaStatus + 2;
            IReadOnlyList<AcaoLocal> acoes = _local.Acoes;
            for (int i = 0; i < acoes.Count; i++)
            {
                bool selecionado = i == _indiceSelecionado;
                string prefixo = selecionado ? "> " : "  ";
                Color cor = selecionado ? Color.Yellow : Color.White;
                string custo = acoes[i].CustoFome > 0 || acoes[i].CustoSede > 0
                    ? $" (Fome -{acoes[i].CustoFome}, Sede -{acoes[i].CustoSede})"
                    : string.Empty;
                Surface.Print(2, linhaAcoes + i, prefixo + acoes[i].Texto + custo, cor, Color.Black);
            }

            // A mensagem do sistema continua sendo impressa de baixo para cima no rodapé fixo
            if (!string.IsNullOrEmpty(_mensagem))
            {
                List<string> linhas = QuebrarLinhas(_mensagem, Width - 4).ToList();
                int linhaInicial = Height - 2 - linhas.Count;
                for (int i = 0; i < linhas.Count; i++)
                    Surface.Print(2, linhaInicial + i, linhas[i], Color.Cyan, Color.Black);
            }

            Surface.Print(2, Height - 1, "Setas + Enter para escolher | I para inventário | ESC para voltar", Color.Gray, Color.Black);
        }


        private static IEnumerable<string> QuebrarLinhas(string texto, int larguraMaxima)
        {
            string[] palavras = texto.Split(' ');
            var linhaAtual = new StringBuilder();

            foreach (string palavra in palavras)
            {
                if (linhaAtual.Length + palavra.Length + 1 > larguraMaxima)
                {
                    yield return linhaAtual.ToString();
                    linhaAtual.Clear();
                }

                if (linhaAtual.Length > 0)
                    linhaAtual.Append(' ');

                linhaAtual.Append(palavra);
            }

            if (linhaAtual.Length > 0)
                yield return linhaAtual.ToString();
        }
    }
}
