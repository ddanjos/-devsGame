using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using SurvivorGame.Inventario;
using SurvivorGame.Mapa;
using SurvivorGame.Regras;
using SurvivorGame.Utilitarios;
using SurvivorGame.Audio;

namespace SurvivorGame.Combate
{
    internal class CombateScreen : ScreenSurface
    {
        /// <summary>
        /// Diretivas e Variaveis.
        /// </summary>
        private enum Fase { MenuPrincipal, MenuAtaques, MenuItens, VendoStatus, Mensagem, FimDeCombate }

        private readonly IScreenObject _telaAnterior;
        private readonly SessaoCombate _sessao;
        private ScreenSurface? _arteXP;

        /// <summary>Plano de fundo da batalha (Lindomar, 25/08). Carregado uma vez
        /// por combate; se o arquivo sumir, fica null e a tela desenha em preto
        /// como antes - arte é enfeite, não pode derrubar o combate.</summary>
        private readonly ScreenSurface? _fundo;

        /// <summary>Faixas de texto: nome/HP do inimigo em cima, status e menu
        /// embaixo. Ver Utilitarios/PainelUi.</summary>
        private const int AlturaFaixaSuperior = 4;
        private const int AlturaFaixaInferior = 12;
        private readonly InimigoNoMapa? _inimigoNoMapa;
        private readonly MapaInimigos? _mapaInimigos;
        private readonly Action? _aoSairDoCombate;

        private Fase _fase = Fase.MenuPrincipal;
        private int _indiceSelecionado;

        private readonly string[] _opcoesPrincipais = { "Atacar", "Defender", "Usar Item", "Ver Status", "Fugir" };
        private List<Habilidade> _habilidadesDisponiveis = new();
        private List<Consumivel> _itensDisponiveis = new();

        private Queue<string> _filaMensagens = new();
        private Action? _aoTerminarMensagens;
        private ResultadoCombate _resultadoFinal = ResultadoCombate.EmAndamento;
        private string _mensagemRecompensa = string.Empty;

        // Construtores e entrada do teclado

                // Sobrecarga Principal: Recebe InimigoNoMapa e MapaInimigos para poder removê-lo
        public CombateScreen(Personagem jogador, InimigoNoMapa inimigoNoMapa, MapaInimigos mapaInimigos, IScreenObject telaAnterior, int largura, int altura, Action? aoSairDoCombate = null)
            : this(jogador, inimigoNoMapa.DadosCombate, telaAnterior, largura, altura, inimigoNoMapa.ArteXP)
        {
            _inimigoNoMapa = inimigoNoMapa;
            _mapaInimigos = mapaInimigos;
            _aoSairDoCombate = aoSairDoCombate;
        }

        public CombateScreen(Personagem jogador, Inimigo inimigo, IScreenObject telaAnterior, int largura, int altura, ScreenSurface? arteXP = null)
    : base(largura, altura)
        {
            this.CorrigirProporcaoDeCelula();

            _telaAnterior = telaAnterior;
            _sessao = new SessaoCombate(jogador, inimigo);

            // CORREÇÃO: Passa o nome puro do inimigo sem remover espaços ou forçar minúsculas,
            // deixando o ArteUtils testar os caminhos originais em disco e resolver os conflitos.
            if (arteXP is null)
            {
                try
                {
                    // Transforma o nome do inimigo em minúsculo e remove todos os espaços e acentos
                    string nomeArquivo = inimigo.Nome.ToLower().Replace(" ", "");

                    // Remove acentuações comuns para bater com os arquivos renomeados
                    nomeArquivo = nomeArquivo
                        .Replace("ã", "a").Replace("á", "a")
                        .Replace("é", "e").Replace("ó", "o");

                    string caminhoProposto = Path.Combine("Artes", "Inimigos", $"{nomeArquivo}.xp");
                    _arteXP = ArteUtils.CarregarArteInimigo(caminhoProposto);
                }
                catch
                {
                    _arteXP = null;
                }
            }
            else
            {
                _arteXP = arteXP;
            }

            _fundo = ArteUtils.CarregarArteCenario("Artes/Cenarios/batalhafundo.xp");

            GerenciadorSom.TocarTrilha(Trilha.Combate);

            UseKeyboard = true;
            IsFocused = true;

            _sessao.IniciarTurnoJogador();


            // O primeiro turno já consome Fome/Sede e pode matar por inanição.
            // Sem esta checagem o combate começava com o jogador em 0 de vida e
            // seguia normalmente, e o aviso de inanição era descartado em silêncio.
            if (!string.IsNullOrEmpty(_sessao.AvisoDeEstado))
            {
                MostrarMensagens(new[] { _sessao.AvisoDeEstado }, () =>
                {
                    if (_sessao.VerificarResultado() != ResultadoCombate.EmAndamento)
                        Finalizar(_sessao.VerificarResultado());
                    else
                        VoltarParaMenuPrincipal();
                });
                return;
            }

            Redesenhar();
        }

        public override bool ProcessKeyboard(Keyboard keyboard)
        {
            bool confirmar = keyboard.IsKeyPressed(Keys.Enter);
            bool voltar = keyboard.IsKeyPressed(Keys.Escape);
            bool qualquerTecla = confirmar || voltar || keyboard.IsKeyPressed(Keys.Space);

            switch (_fase)
            {
                case Fase.MenuPrincipal:
                    ProcessarMenu(keyboard, _opcoesPrincipais.Length, ConfirmarMenuPrincipal);
                    break;

                case Fase.MenuAtaques:
                    if (voltar) VoltarParaMenuPrincipal();
                    else ProcessarMenu(keyboard, Math.Max(_habilidadesDisponiveis.Count, 1), ConfirmarAtaque);
                    break;

                case Fase.MenuItens:
                    if (voltar) VoltarParaMenuPrincipal();
                    else ProcessarMenu(keyboard, Math.Max(_itensDisponiveis.Count, 1), ConfirmarItem);
                    break;

                case Fase.VendoStatus:
                    if (qualquerTecla)
                    {
                        _fase = Fase.MenuPrincipal;
                        Redesenhar();
                    }
                    break;

                case Fase.Mensagem:
                    if (qualquerTecla) AvancarMensagem();
                    break;

                case Fase.FimDeCombate:
                    if (qualquerTecla)
                    {
                        // Derrota = fim de jogo de verdade. Antes disso, perder um
                        // combate só devolvia o jogador ao mapa com a vida zerada,
                        // e o jogo continuava normalmente - não existia derrota.
                        if (_resultadoFinal == ResultadoCombate.Derrota)
                        {
                            Game.Instance.Screen = new Cenarios.FimDeJogoScreen(
                                venceu: false, Cenarios.FimDeJogoScreen.TextoDerrota,
                                Program.LarguraJanela, Program.AlturaJanela);
                            Game.Instance.Screen.IsFocused = true;
                            return true;
                        }

                        _aoSairDoCombate?.Invoke();
                        Game.Instance.Screen = _telaAnterior;
                        Game.Instance.Screen!.IsFocused = true;
                    }
                    break;
            }

            return true;
        }

        private void VoltarParaMenuPrincipal()
        {
            _fase = Fase.MenuPrincipal;
            _indiceSelecionado = 0;
            Redesenhar();
        }

        private void ProcessarMenu(Keyboard keyboard, int totalOpcoes, Action confirmar)
        {
            if (keyboard.IsKeyPressed(Keys.Down))
            {
                _indiceSelecionado = (_indiceSelecionado + 1) % totalOpcoes;
                Redesenhar();
            }
            else if (keyboard.IsKeyPressed(Keys.Up))
            {
                _indiceSelecionado = (_indiceSelecionado - 1 + totalOpcoes) % totalOpcoes;
                Redesenhar();
            }
            else if (keyboard.IsKeyPressed(Keys.Enter))
            {
                confirmar();
            }
        }
        // Logica de combate, Menus e renderizacao
        private void ConfirmarMenuPrincipal()
        {
            switch (_indiceSelecionado)
            {
                case 0:
                    _habilidadesDisponiveis = new List<Habilidade> { _sessao.AtaqueBasico };
                    _habilidadesDisponiveis.AddRange(_sessao.Jogador.HabilidadesEspeciais);
                    _fase = Fase.MenuAtaques;
                    _indiceSelecionado = 0;
                    Redesenhar();
                    break;

                case 1:
                    ExecutarAcaoDoJogador(() => _sessao.Defender());
                    break;

                case 2:
                    _itensDisponiveis = _sessao.Jogador.Inventario.Itens.OfType<Consumivel>().ToList();
                    _fase = Fase.MenuItens;
                    _indiceSelecionado = 0;
                    Redesenhar();
                    break;

                case 3:
                    _fase = Fase.VendoStatus;
                    Redesenhar();
                    break;

                case 4:
                    string mensagemFuga = _sessao.Fugir();
                    // Fugir não pode ressuscitar: se a vida já chegou a 0 (inanição),
                    // o resultado é derrota. Antes dava pra fugir com 0 de vida e
                    // continuar jogando morto pelo mapa da cidade.
                    MostrarMensagens(new[] { mensagemFuga }, () =>
                        Finalizar(_sessao.VerificarResultado() == ResultadoCombate.Derrota
                            ? ResultadoCombate.Derrota
                            : ResultadoCombate.Fugiu));
                    break;
            }
        }

        private void ConfirmarAtaque()
        {
            if (_habilidadesDisponiveis.Count == 0) return;

            Habilidade escolhida = _habilidadesDisponiveis[_indiceSelecionado];

            // Sem energia suficiente a habilidade não sai - e antes o jogador ainda
            // PERDIA o turno (o inimigo contra-atacava mesmo assim), porque o menu
            // deixava escolher e o resultado "energia insuficiente" seguia o mesmo
            // caminho de um ataque válido. Agora avisa e devolve pro menu.
            if (escolhida.CustoEnergia > _sessao.Energia)
            {
                MostrarMensagens(
                    new[] { $"Energia insuficiente para {escolhida.Nome}: precisa de {escolhida.CustoEnergia}, você tem {_sessao.Energia}." },
                    () => { _fase = Fase.MenuAtaques; Redesenhar(); });
                return;
            }

            ExecutarAcaoDoJogador(() => _sessao.Atacar(escolhida));
        }

        private void ConfirmarItem()
        {
            if (_itensDisponiveis.Count == 0)
            {
                VoltarParaMenuPrincipal();
                return;
            }

            string nomeItem = _itensDisponiveis[_indiceSelecionado].Nome;
            ExecutarAcaoDoJogador(() => _sessao.UsarItem(nomeItem));
        }

        private void ExecutarAcaoDoJogador(Func<string> acao)
        {
            int vidaAntes = _sessao.Jogador.Vida;
            string mensagemJogador = acao();

            // O som do golpe do jogador sai aqui e não dentro da SessaoCombate: a
            // sessão é a REGRA do combate e não deve conhecer áudio nenhum. Quem
            // apresenta o combate é esta tela, e é ela quem faz barulho.
            GerenciadorSom.Tocar(Efeito.Ataque);

            if (_sessao.VerificarResultado() != ResultadoCombate.EmAndamento)
            {
                MostrarMensagens(new[] { mensagemJogador }, () => Finalizar(_sessao.VerificarResultado()));
                return;
            }

            string mensagemInimigo = _sessao.TurnoInimigo();

            // Só toca o som de dor se o inimigo REALMENTE tirou vida - senão a ação
            // nula dele (a piada à la Earthbound) soaria como uma pancada.
            if (_sessao.Jogador.Vida < vidaAntes)
                GerenciadorSom.Tocar(Efeito.DanoJogador);

            MostrarMensagens(new[] { mensagemJogador, mensagemInimigo }, () =>
            {
                if (_sessao.VerificarResultado() != ResultadoCombate.EmAndamento)
                {
                    Finalizar(_sessao.VerificarResultado());
                }
                else
                {
                    _sessao.IniciarTurnoJogador();

                    // A inanição/desidratação acontece no início do turno e PODE
                    // matar - por isso checa o resultado de novo aqui, senão o
                    // jogador continuaria jogando com a vida em 0.
                    if (!string.IsNullOrEmpty(_sessao.AvisoDeEstado))
                    {
                        MostrarMensagens(new[] { _sessao.AvisoDeEstado }, () =>
                        {
                            if (_sessao.VerificarResultado() != ResultadoCombate.EmAndamento)
                                Finalizar(_sessao.VerificarResultado());
                            else
                                VoltarParaMenuPrincipal();
                        });
                        return;
                    }

                    VoltarParaMenuPrincipal();
                }
            });
        }

        private void MostrarMensagens(IEnumerable<string> mensagens, Action aoTerminar)
        {
            _filaMensagens = new Queue<string>(mensagens);
            _aoTerminarMensagens = aoTerminar;
            _fase = Fase.Mensagem;
            Redesenhar();
        }

        private void AvancarMensagem()
        {
            if (_filaMensagens.Count > 0)
                _filaMensagens.Dequeue();

            if (_filaMensagens.Count == 0)
            {
                Action? callback = _aoTerminarMensagens;
                _aoTerminarMensagens = null;
                callback?.Invoke();
            }
            else
            {
                Redesenhar();
            }
        }

        private void Finalizar(ResultadoCombate resultado)
        {
            _resultadoFinal = resultado;

            GerenciadorSom.Tocar(resultado switch
            {
                ResultadoCombate.Vitoria => Efeito.InimigoMorre,
                ResultadoCombate.Derrota => Efeito.Derrota,
                _ => Efeito.MenuVoltar,
            });

            // A recompensa de missão precisa valer pra QUALQUER vitória, não só
            // combate contra um inimigo que já estava desenhado no mapa
            // (_inimigoNoMapa) - antes disso ficava preso atrás do "is not null" e
            // um encontro aleatório (ex: LocalAndarZero, ver Mapa/PontosInteresse/
            // ResultadoAcao) nunca dava a recompensa. Só a remoção do sprite do
            // mapa (que só existe se ele veio de lá) continua condicional.
            if (resultado == ResultadoCombate.Vitoria)
            {
                if (_inimigoNoMapa is not null)
                    _mapaInimigos?.RemoverInimigo(_inimigoNoMapa);

                _mensagemRecompensa = GerenciadorJogo.ProcessarVitoriaInimigo(_sessao.Inimigo.Nome);

                // Drop de item do próprio inimigo (SCRUM-17). Diferente das peças
                // do rádio, que são progresso de missão, este é só recurso de
                // sobrevivência - e pode falhar se a mochila estiver cheia.
                if (_sessao.Inimigo.ItemDrop is not null)
                {
                    bool coletou = _sessao.Jogador.Inventario.AdicionarItem(_sessao.Inimigo.ItemDrop);
                    _mensagemRecompensa += coletou
                        ? $" Encontrou: {_sessao.Inimigo.ItemDrop.Nome}."
                        : $" {_sessao.Inimigo.ItemDrop.Nome} ficou pra trás - mochila cheia.";
                }
            }

            _fase = Fase.FimDeCombate;
            Redesenhar();
        }

        private void Redesenhar()
        {
            Surface.Clear();

            // Ordem: cenário -> inimigo por cima dele -> faixas de texto por cima
            // de tudo. O inimigo precisa do DesenharPorCima (e não do Copy) porque
            // o fundo dele é transparente e o Copy abriria um buraco no cenário -
            // ver Utilitarios/PainelUi.
            if (_fundo is not null)
                PainelUi.DesenharPorCima(_fundo, Surface, (Width / 2) - (_fundo.Width / 2), 0);

            if (_arteXP is not null)
            {
                // Centralizado na area livre entre as duas faixas - os sprites tem
                // alturas bem diferentes (o rato tem 14 linhas, o enxame tem 35) e
                // ancorar todos no topo deixava os menores flutuando.
                int alturaCena = Height - AlturaFaixaSuperior - AlturaFaixaInferior;
                int posX = (Width / 2) - (_arteXP.Width / 2);
                int posY = AlturaFaixaSuperior + System.Math.Max(0, (alturaCena - _arteXP.Height) / 2);
                PainelUi.DesenharPorCima(_arteXP, Surface, posX, posY);
            }

            PainelUi.DesenharFaixaSuperior(Surface, AlturaFaixaSuperior);
            PainelUi.DesenharFaixa(Surface, AlturaFaixaInferior);

            Surface.PrintTexto(2, 1, _sessao.Inimigo.Nome, Color.OrangeRed, Color.Black);
            Surface.PrintTexto(2, 2, $"HP: {_sessao.Inimigo.VidaAtual}/{_sessao.Inimigo.VidaMaxima}", Color.White, Color.Black);

            switch (_fase)
            {
                case Fase.MenuPrincipal:
                    DesenharStatusJogador();
                    DesenharMenu(_opcoesPrincipais, Height - 7);
                    break;

                case Fase.MenuAtaques:
                    DesenharStatusJogador();
                    string[] nomesHabilidades = _habilidadesDisponiveis
                        .Select(h => h.CustoEnergia > 0 ? $"{h.Nome} ({h.CustoEnergia} energia)" : h.Nome)
                        .ToArray();
                    DesenharMenu(nomesHabilidades, Height - 7);
                    Surface.PrintTexto(2, Height - 1, "ESC para voltar", Color.Gray, Color.Black);
                    break;

                case Fase.MenuItens:
                    DesenharStatusJogador();
                    string[] nomesItens = _itensDisponiveis.Count > 0
                        ? _itensDisponiveis.Select(i => $"{i.Nome} x{i.Quantidade} (cura {i.Cura})").ToArray()
                        : new[] { "(nenhum item disponível)" };
                    DesenharMenu(nomesItens, Height - 7);
                    Surface.PrintTexto(2, Height - 1, "ESC para voltar", Color.Gray, Color.Black);
                    break;

                case Fase.VendoStatus:
                    // Ficha completa: precisa da tela limpa, senao o texto sai por
                    // cima do cenario e do sprite. Antes da arte entrar o fundo ja
                    // era preto e isso nao fazia falta.
                    Surface.Clear();
                    PainelUi.DesenharFaixaSuperior(Surface, 13);
                    Surface.PrintTexto(2, 1, "FICHA DO COMBATE", Color.Gold, Color.Black);
                    Surface.PrintTexto(2, 5, $"Rodada {_sessao.Rodada}  |  Iniciativa: {_sessao.Iniciativa}", Color.Cyan, Color.Black);
                    Surface.PrintTexto(2, 6, $"{_sessao.Inimigo.Nome} - HP {_sessao.Inimigo.VidaAtual}/{_sessao.Inimigo.VidaMaxima}", Color.White, Color.Black);
                    Surface.PrintTexto(2, 7, $"   Forca: {_sessao.Inimigo.Forca}   Defesa: {_sessao.Inimigo.Defesa}", Color.Gray, Color.Black);
                    Surface.PrintTexto(2, 9, $"{_sessao.Jogador.Nome} - HP {_sessao.Jogador.Vida}/{_sessao.Jogador.VidaMaxima}", Color.White, Color.Black);
                    Surface.PrintTexto(2, 10, $"   Forca: {_sessao.Jogador.Forca}   Defesa: {_sessao.Jogador.Defesa}", Color.Gray, Color.Black);
                    Surface.PrintTexto(2, 11, $"Fome: {_sessao.Jogador.Fome}   Sede: {_sessao.Jogador.Sede}   Energia: {_sessao.Energia}", Color.Cyan, Color.Black);
                    Surface.PrintTexto(2, Height - 1, "Pressione qualquer tecla para voltar (seu turno continua)", Color.Gray, Color.Black);
                    break;

                case Fase.Mensagem:
                    DesenharStatusJogador();
                    if (_filaMensagens.Count > 0)
                        ImprimirQuebrando(_filaMensagens.Peek(), Height - 6, Color.Yellow);
                    Surface.PrintTexto(2, Height - 1, "Pressione qualquer tecla para continuar", Color.Gray, Color.Black); break;
                case Fase.FimDeCombate: string textoFinal = _resultadoFinal switch { ResultadoCombate.Vitoria => $"Você derrotou {_sessao.Inimigo.Nome}!{_mensagemRecompensa}", ResultadoCombate.Derrota => $"{_sessao.Jogador.Nome} foi derrotado...", ResultadoCombate.Fugiu => "Você fugiu da batalha.", _ => "" }; ImprimirQuebrando(textoFinal, Height - 8, Color.White); Surface.PrintTexto(2, Height - 1, "Pressione qualquer tecla para continuar", Color.Gray, Color.Black); break;
            }
        }
        /// <summary>
        /// Imprime quebrando em linhas. O Print do SadConsole escreve num buffer
        /// plano: texto maior que a largura da tela transborda pra linha de baixo,
        /// começando na coluna 0. A fala mais longa de inimigo tem 100 caracteres e
        /// a mensagem de vitória com a recompensa chega a 109 - as duas vazavam.
        /// A primeira linha fica em 'linha'; as seguintes descem.
        /// </summary>
        private void ImprimirQuebrando(string texto, int linha, Color cor)
        {
            if (string.IsNullOrEmpty(texto)) return;

            var atual = new System.Text.StringBuilder();
            int y = linha;

            foreach (string palavra in texto.Split(' '))
            {
                if (atual.Length + palavra.Length + 1 > Width - 4)
                {
                    if (y < Height - 1) Surface.PrintTexto(2, y, atual.ToString(), cor, Color.Black);
                    atual.Clear();
                    y++;
                }

                if (atual.Length > 0) atual.Append(' ');
                atual.Append(palavra);
            }

            if (atual.Length > 0 && y < Height - 1)
                Surface.PrintTexto(2, y, atual.ToString(), cor, Color.Black);
        }

        private void DesenharStatusJogador() { int y = Height - 10; Surface.PrintTexto(2, y, $"{_sessao.Jogador.Nome}   Rodada {_sessao.Rodada}   Iniciativa: {_sessao.Iniciativa}", Color.LimeGreen, Color.Black); Surface.PrintTexto(2, y + 1, $"HP: {_sessao.Jogador.Vida}/{_sessao.Jogador.VidaMaxima}   Fome: {_sessao.Jogador.Fome}   Sede: {_sessao.Jogador.Sede}   Energia: {_sessao.Energia}", Color.White, Color.Black); }
        private void DesenharMenu(System.Collections.Generic.IReadOnlyList<string> opcoes, int yInicial)
        {
            for (int i = 0; i < opcoes.Count; i++)
            {
                bool selecionado = i == _indiceSelecionado;
                string prefixo = selecionado ? "> " : "  ";
                Color cor = selecionado ? Color.Yellow : Color.White;
                Surface.PrintTexto(2, yInicial + i, prefixo + opcoes[i], cor, Color.Black);
            }
        }

    }
}
