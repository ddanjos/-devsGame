using System;
using System.Collections.Generic;
using System.Linq;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using SurvivorGame.Inventario;
using SurvivorGame.Mapa;
using SurvivorGame.Regras;

namespace SurvivorGame.Combate
{
    internal class CombateScreen : ScreenSurface
    {
        private enum Fase { MenuPrincipal, MenuAtaques, MenuItens, VendoStatus, Mensagem, FimDeCombate }

        private readonly IScreenObject _telaAnterior;
        private readonly SessaoCombate _sessao;
        private readonly ScreenSurface? _arteXP;
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
            _telaAnterior = telaAnterior;
            _arteXP = arteXP;
            _sessao = new SessaoCombate(jogador, inimigo);

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
                                venceu: false, Cenarios.FimDeJogoScreen.TextoDerrota, Width, Height);
                            Game.Instance.Screen.IsFocused = true;
                            return true;
                        }

                        // Redesenha a tela do overworld para limpar o sprite do inimigo
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
            string mensagemJogador = acao();

            if (_sessao.VerificarResultado() != ResultadoCombate.EmAndamento)
            {
                MostrarMensagens(new[] { mensagemJogador }, () => Finalizar(_sessao.VerificarResultado()));
                return;
            }

            string mensagemInimigo = _sessao.TurnoInimigo();

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
            }

            _fase = Fase.FimDeCombate;
            Redesenhar();
        }

        private void Redesenhar()
        {
            Surface.Clear();

            Surface.Print(2, 2, _sessao.Inimigo.Nome, Color.OrangeRed, Color.Black);
            Surface.Print(2, 3, $"HP: {_sessao.Inimigo.VidaAtual}/{_sessao.Inimigo.VidaMaxima}", Color.White, Color.Black);

            if (_arteXP is not null)
            {
                int posX = (Width / 2) - (_arteXP.Width / 2);
                int posY = 4;
                _arteXP.Surface.Copy(this.Surface, posX, posY);
            }

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
                    Surface.Print(2, Height - 1, "ESC para voltar", Color.Gray, Color.Black);
                    break;

                case Fase.MenuItens:
                    DesenharStatusJogador();
                    string[] nomesItens = _itensDisponiveis.Count > 0
                        ? _itensDisponiveis.Select(i => $"{i.Nome} x{i.Quantidade} (cura {i.Cura})").ToArray()
                        : new[] { "(nenhum item disponível)" };
                    DesenharMenu(nomesItens, Height - 7);
                    Surface.Print(2, Height - 1, "ESC para voltar", Color.Gray, Color.Black);
                    break;

                case Fase.VendoStatus:
                    Surface.Print(2, 5, $"Rodada {_sessao.Rodada}  |  Iniciativa: {_sessao.Iniciativa}", Color.Cyan, Color.Black);
                    Surface.Print(2, 6, $"{_sessao.Inimigo.Nome} - HP {_sessao.Inimigo.VidaAtual}/{_sessao.Inimigo.VidaMaxima}", Color.White, Color.Black);
                    Surface.Print(2, 8, $"{_sessao.Jogador.Nome} - HP {_sessao.Jogador.Vida}/{_sessao.Jogador.VidaMaxima}", Color.White, Color.Black);
                    Surface.Print(2, 9, $"Fome: {_sessao.Jogador.Fome}   Sede: {_sessao.Jogador.Sede}   Energia: {_sessao.Energia}", Color.Cyan, Color.Black);
                    Surface.Print(2, Height - 1, "Pressione qualquer tecla para voltar (seu turno continua)", Color.Gray, Color.Black);
                    break;

                case Fase.Mensagem:
                    DesenharStatusJogador();
                    if (_filaMensagens.Count > 0)
                        Surface.Print(2, Height - 6, _filaMensagens.Peek(), Color.Yellow, Color.Black);
                    Surface.Print(2, Height - 1, "Pressione qualquer tecla para continuar", Color.Gray, Color.Black);
                    break;

                case Fase.FimDeCombate:
                    string textoFinal = _resultadoFinal switch
                    {
                        ResultadoCombate.Vitoria => $"Você derrotou {_sessao.Inimigo.Nome}!{_mensagemRecompensa}",
                        ResultadoCombate.Derrota => $"{_sessao.Jogador.Nome} foi derrotado...",
                        ResultadoCombate.Fugiu => "Você fugiu da batalha.",
                        _ => ""
                    };
                    Surface.Print(2, Height / 2, textoFinal, Color.White, Color.Black);
                    Surface.Print(2, Height - 1, "Pressione qualquer tecla para continuar", Color.Gray, Color.Black);
                    break;
            }
        }

        private void DesenharStatusJogador()
        {
            int y = Height - 10;
            Surface.Print(2, y, $"{_sessao.Jogador.Nome}   Rodada {_sessao.Rodada}   Iniciativa: {_sessao.Iniciativa}", Color.LimeGreen, Color.Black);
            Surface.Print(2, y + 1, $"HP: {_sessao.Jogador.Vida}/{_sessao.Jogador.VidaMaxima}   Fome: {_sessao.Jogador.Fome}   Sede: {_sessao.Jogador.Sede}   Energia: {_sessao.Energia}", Color.White, Color.Black);
        }

        private void DesenharMenu(IReadOnlyList<string> opcoes, int yInicial)
        {
            for (int i = 0; i < opcoes.Count; i++)
            {
                bool selecionado = i == _indiceSelecionado;
                string prefixo = selecionado ? "> " : "  ";
                Color cor = selecionado ? Color.Yellow : Color.White;
                Surface.Print(2, yInicial + i, prefixo + opcoes[i], cor, Color.Black);
            }
        }
    }
}