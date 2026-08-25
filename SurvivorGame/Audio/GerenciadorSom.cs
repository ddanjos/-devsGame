using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using SurvivorGame.Regras;

namespace SurvivorGame.Audio
{
    /// <summary>Cada som do jogo, nomeado. Enum em vez de string solta pra que um
    /// erro de digitação vire erro de compilação, não silêncio em runtime.</summary>
    internal enum Efeito
    {
        MenuMover, MenuConfirmar, MenuVoltar,
        Ataque, DanoJogador, InimigoMorre,
        Item, Peca, Erro,
        Vitoria, Derrota,
    }

    internal enum Trilha { Nenhuma, Exploracao, Combate }

    /// <summary>
    /// SOM DO JOGO. Fachada única: o resto do código diz "toca o som de ataque" e
    /// não conhece OpenAL, WAV nem MonoGame.Audio.
    ///
    /// Três decisões que valem explicar:
    ///
    /// 1. FALHA SUAVE, SEMPRE. Máquina sem placa de som, driver de áudio ausente,
    ///    saída roteada pra um projetor que não existe - tudo isso faz o MonoGame
    ///    lançar exceção ao criar um SoundEffect. Num jogo apresentado numa sala de
    ///    aula, isso não pode fechar a janela. Se a inicialização falhar,
    ///    _disponivel vira false e TODO método daqui pra frente é um no-op. O jogo
    ///    roda mudo e ninguém percebe. É o padrão Null Object aplicado ao objeto
    ///    inteiro em vez de a uma instância.
    ///
    /// 2. OS ARQUIVOS SÃO WAV PCM CRU, carregados por SoundEffect.FromStream. Nada
    ///    de content pipeline (.mgcb) - seria uma etapa de build a mais no projeto
    ///    de todo mundo, pra ganhar compressão que 2 MB de áudio não precisa.
    ///
    /// 3. PREFERÊNCIAS SEPARADAS DO SAVE. Ligar/desligar música é configuração da
    ///    MÁQUINA, não progresso da partida: quem desliga o som quer que continue
    ///    desligado mesmo começando um jogo novo. Por isso vive em config.json e
    ///    não em savegame.json. Ver Regras/Configuracao.
    /// </summary>
    internal static class GerenciadorSom
    {
        private static readonly Dictionary<Efeito, SoundEffect> _efeitos = new();
        private static readonly Dictionary<Trilha, SoundEffect> _trilhas = new();

        private static SoundEffectInstance? _tocandoAgora;
        private static Trilha _trilhaAtual = Trilha.Nenhuma;

        /// <summary>False quando não há áudio nenhum disponível na máquina. Nesse
        /// caso tudo aqui vira no-op silencioso.</summary>
        public static bool Disponivel { get; private set; }

        private const float VolumeEfeitos = 0.75f;
        private const float VolumeMusica = 0.35f;

        private static readonly Dictionary<Efeito, string> Arquivos = new()
        {
            [Efeito.MenuMover] = "menu_mover.wav",
            [Efeito.MenuConfirmar] = "menu_confirmar.wav",
            [Efeito.MenuVoltar] = "menu_voltar.wav",
            [Efeito.Ataque] = "ataque.wav",
            [Efeito.DanoJogador] = "dano_jogador.wav",
            [Efeito.InimigoMorre] = "inimigo_morre.wav",
            [Efeito.Item] = "item.wav",
            [Efeito.Peca] = "peca.wav",
            [Efeito.Erro] = "erro.wav",
            [Efeito.Vitoria] = "vitoria.wav",
            [Efeito.Derrota] = "derrota.wav",
        };

        private static readonly Dictionary<Trilha, string> ArquivosTrilha = new()
        {
            [Trilha.Exploracao] = "musica_exploracao.wav",
            [Trilha.Combate] = "musica_combate.wav",
        };

        /// <summary>Carrega tudo uma vez, no começo do jogo. Um arquivo que falhar
        /// é só pulado - o jogo perde aquele som, não trava.</summary>
        public static void Iniciar()
        {
            try
            {
                foreach ((Efeito chave, string arquivo) in Arquivos)
                {
                    SoundEffect? som = Carregar(arquivo);
                    if (som is not null) _efeitos[chave] = som;
                }

                foreach ((Trilha chave, string arquivo) in ArquivosTrilha)
                {
                    SoundEffect? som = Carregar(arquivo);
                    if (som is not null) _trilhas[chave] = som;
                }

                // Só nos declaramos disponíveis se ALGUMA coisa carregou.
                Disponivel = _efeitos.Count > 0 || _trilhas.Count > 0;
            }
            catch
            {
                // Sem dispositivo de áudio na máquina. Jogo roda mudo.
                Disponivel = false;
            }
        }

        private static SoundEffect? Carregar(string arquivo)
        {
            try
            {
                string caminho = Resolver(arquivo);
                if (!File.Exists(caminho)) return null;

                using FileStream fluxo = File.OpenRead(caminho);
                return SoundEffect.FromStream(fluxo);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>Mesma história do ArteUtils: caminho relativo é resolvido contra
        /// o diretório de TRABALHO, que nem sempre é a pasta do executável.</summary>
        private static string Resolver(string arquivo)
        {
            string perto = Path.Combine("Audio", arquivo);
            return File.Exists(perto) ? perto : Path.Combine(AppContext.BaseDirectory, "Audio", arquivo);
        }

        public static void Tocar(Efeito efeito)
        {
            if (!Disponivel || !Configuracao.EfeitosLigados) return;
            if (!_efeitos.TryGetValue(efeito, out SoundEffect? som)) return;

            try { som.Play(VolumeEfeitos, 0f, 0f); }
            catch { /* um efeito que não toca não pode derrubar o jogo */ }
        }

        /// <summary>Troca a música de fundo. Pedir a trilha que JÁ está tocando não
        /// faz nada - senão a música reiniciaria do zero a cada redesenho de tela.</summary>
        public static void TocarTrilha(Trilha trilha)
        {
            if (!Disponivel) return;
            // Já está tocando essa mesma trilha: não reinicia. Sem isso a música
            // voltava pro começo a cada redesenho de tela.
            if (trilha == _trilhaAtual && _tocandoAgora is not null) return;

            _trilhaAtual = trilha;
            ReiniciarTrilha();
        }

        public static void PararTrilha()
        {
            try { _tocandoAgora?.Stop(); _tocandoAgora?.Dispose(); }
            catch { }
            _tocandoAgora = null;
        }

        /// <summary>Chamado quando o jogador liga/desliga a música nas opções: para
        /// na hora, ou volta a tocar a trilha da tela atual.</summary>
        public static void AplicarPreferencias()
        {
            if (!Disponivel) return;
            ReiniciarTrilha();
        }

        private static void ReiniciarTrilha()
        {
            PararTrilha();

            if (!Configuracao.MusicaLigada || _trilhaAtual == Trilha.Nenhuma) return;
            if (!_trilhas.TryGetValue(_trilhaAtual, out SoundEffect? som)) return;

            try
            {
                _tocandoAgora = som.CreateInstance();
                _tocandoAgora.IsLooped = true;
                _tocandoAgora.Volume = VolumeMusica;
                _tocandoAgora.Play();
            }
            catch
            {
                _tocandoAgora = null;
            }
        }
    }
}
