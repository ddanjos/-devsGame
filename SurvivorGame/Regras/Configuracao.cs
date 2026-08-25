using System;
using System.IO;
using System.Text.Json;

namespace SurvivorGame.Regras
{
    /// <summary>
    /// Preferências da MÁQUINA, não da partida: hoje só música e efeitos ligados
    /// ou desligados (ver Ui/OpcoesScreen e Audio/GerenciadorSom).
    ///
    /// Vive num arquivo separado do savegame.json de propósito. Quem desliga o som
    /// quer que continue desligado ao começar um jogo novo, ao carregar outro save,
    /// ou sem save nenhum - misturar isso com o progresso da partida seria confundir
    /// duas coisas com ciclos de vida diferentes. É a mesma razão pela qual as
    /// configurações de um jogo de verdade não vêm dentro do arquivo de save.
    ///
    /// Se o arquivo não existir ou estiver corrompido, valem os padrões (tudo
    /// ligado) - configuração nunca pode impedir o jogo de abrir.
    /// </summary>
    internal static class Configuracao
    {
        public static bool MusicaLigada { get; set; } = true;
        public static bool EfeitosLigados { get; set; } = true;

        private static string Caminho => Path.Combine(AppContext.BaseDirectory, "config.json");

        private class Dados
        {
            public bool MusicaLigada { get; set; } = true;
            public bool EfeitosLigados { get; set; } = true;
        }

        public static void Carregar()
        {
            try
            {
                if (!File.Exists(Caminho)) return;

                Dados? d = JsonSerializer.Deserialize<Dados>(File.ReadAllText(Caminho));
                if (d is null) return;

                MusicaLigada = d.MusicaLigada;
                EfeitosLigados = d.EfeitosLigados;
            }
            catch
            {
                // Config ilegível: segue com os padrões.
            }
        }

        public static void Salvar()
        {
            try
            {
                File.WriteAllText(Caminho, JsonSerializer.Serialize(
                    new Dados { MusicaLigada = MusicaLigada, EfeitosLigados = EfeitosLigados },
                    new JsonSerializerOptions { WriteIndented = true }));
            }
            catch
            {
                // Sem permissão de escrita: a preferência vale só nesta sessão.
            }
        }
    }
}
