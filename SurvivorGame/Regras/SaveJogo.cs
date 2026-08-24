using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using SurvivorGame.Inventario;

namespace SurvivorGame.Regras
{
    /// <summary>
    /// SISTEMA DE SAVE (SCRUM-11). Grava a partida num arquivo JSON ao lado do
    /// executável e consegue reconstruir o jogo a partir dele.
    ///
    /// Duas decisões de projeto que valem ser explicadas:
    ///
    /// 1. NÃO serializamos o Personagem nem os itens direto. Eles são objetos de
    ///    domínio, com propriedades de set privado, herança (ItemInventario ->
    ///    Arma / Armadura / Consumivel) e regras próprias. Jogar um serializador em
    ///    cima disso amarraria o formato do arquivo à forma interna das classes:
    ///    qualquer refatoração quebraria todos os saves. Em vez disso existe uma
    ///    camada de DTO (SaveDados / ItemSalvo) - classes burras, só com dados,
    ///    cuja única função é ser o "contrato" do arquivo. É o padrão Data Transfer
    ///    Object, e é o mesmo motivo pelo qual uma API não devolve a entidade do
    ///    banco direto pro cliente.
    ///
    /// 2. Salvar só é permitido no MAPA DA CIDADE (ver Ui/PauseScreen). Assim o
    ///    save nunca precisa descrever "estou no meio de um combate na rodada 4" ou
    ///    "estou dentro do armazém do Museu Hering" - só posição no mapa, atributos,
    ///    mochila e progresso da missão. É uma restrição de escopo consciente:
    ///    menos estado pra representar, zero chance de carregar num estado
    ///    impossível.
    /// </summary>
    internal static class SaveJogo
    {
        /// <summary>Versão do formato do arquivo. Se um dia a estrutura mudar, um
        /// save antigo pode ser detectado aqui em vez de estourar erro estranho.</summary>
        public const int VersaoAtual = 1;

        private static readonly JsonSerializerOptions Opcoes = new() { WriteIndented = true };

        /// <summary>Fica ao lado do .exe de propósito: dá pra abrir o arquivo e ler
        /// o JSON, o que é ótimo pra demonstrar que o save funciona de verdade.</summary>
        public static string Caminho => Path.Combine(AppContext.BaseDirectory, "savegame.json");

        public static bool ExisteSave() => File.Exists(Caminho);

        /// <summary>Data/hora do save existente, pra mostrar no menu. Null se não há save.</summary>
        public static DateTime? QuandoFoiSalvo() =>
            ExisteSave() ? File.GetLastWriteTime(Caminho) : null;

        public static void Apagar()
        {
            if (ExisteSave()) File.Delete(Caminho);
        }

        // ------------------------------------------------------------------
        // SALVAR
        // ------------------------------------------------------------------

        /// <summary>Grava a partida. Devolve false e preenche 'erro' em vez de
        /// lançar exceção: um save que falha não pode derrubar o jogo - a tela de
        /// pause só mostra a mensagem e a partida continua.</summary>
        public static bool Salvar(Personagem jogador, out string erro)
        {
            try
            {
                var dados = new SaveDados
                {
                    Versao = VersaoAtual,
                    Nome = jogador.Nome,
                    X = jogador.X,
                    Y = jogador.Y,
                    Vida = jogador.Vida,
                    VidaMaxima = jogador.VidaMaxima,
                    Fome = jogador.Fome,
                    Sede = jogador.Sede,
                    Experiencia = jogador.Experiencia,
                    Forca = jogador.Forca,
                    DefesaBase = jogador.DefesaBase,
                    Capacidade = jogador.Inventario.Capacidade,
                    Itens = jogador.Inventario.Itens.Select(ItemSalvo.De).ToList(),
                    NomeArmaEquipada = jogador.ArmaEquipada?.Nome,
                    NomeArmaduraEquipada = jogador.ArmaduraEquipada?.Nome,
                    TemAntena = GerenciadorJogo.TemAntena,
                    TemBateria = GerenciadorJogo.TemBateria,
                    TemFusivel = GerenciadorJogo.TemFusivel,
                    SinosTocados = GerenciadorJogo.SinosTocados,
                    SegredoCatedralRevelado = GerenciadorJogo.SegredoCatedralRevelado,
                };

                File.WriteAllText(Caminho, JsonSerializer.Serialize(dados, Opcoes));
                erro = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                erro = ex.Message;
                return false;
            }
        }

        // ------------------------------------------------------------------
        // CARREGAR
        // ------------------------------------------------------------------

        /// <summary>Lê o save e reconstrói o Personagem, além de restaurar o
        /// progresso da missão no GerenciadorJogo. Devolve null se não há save, se
        /// o arquivo está corrompido ou se a versão não bate - nesses casos quem
        /// chamou simplesmente começa uma partida nova.</summary>
        public static Personagem? Carregar()
        {
            try
            {
                if (!ExisteSave()) return null;

                SaveDados? dados = JsonSerializer.Deserialize<SaveDados>(File.ReadAllText(Caminho));
                if (dados is null || dados.Versao != VersaoAtual) return null;

                var jogador = new Personagem(dados.Nome, dados.X, dados.Y,
                    dados.Capacidade > 0 ? dados.Capacidade : 5);
                jogador.CarregarEstado(dados.Vida, dados.VidaMaxima, dados.Fome, dados.Sede,
                    dados.Experiencia, dados.Forca, dados.DefesaBase);

                foreach (ItemSalvo salvo in dados.Itens)
                {
                    ItemInventario? item = salvo.Reconstruir();
                    if (item is null) continue;

                    jogador.Inventario.AdicionarItem(item);

                    // Reequipa depois de adicionar: equipar um item que não está na
                    // mochila deixaria o painel de status mentindo.
                    if (item.Nome == dados.NomeArmaEquipada || item.Nome == dados.NomeArmaduraEquipada)
                        jogador.Equipar(item);
                }

                GerenciadorJogo.TemAntena = dados.TemAntena;
                GerenciadorJogo.TemBateria = dados.TemBateria;
                GerenciadorJogo.TemFusivel = dados.TemFusivel;
                GerenciadorJogo.SinosTocados = dados.SinosTocados;
                GerenciadorJogo.SegredoCatedralRevelado = dados.SegredoCatedralRevelado;

                return jogador;
            }
            catch
            {
                // Save corrompido não pode impedir o jogo de abrir.
                return null;
            }
        }
    }

    // ----------------------------------------------------------------------
    // DTOs - o "contrato" do arquivo. Propriedades públicas com get/set porque
    // o System.Text.Json precisa disso; são classes de dados, não de domínio.
    // ----------------------------------------------------------------------

    internal class SaveDados
    {
        public int Versao { get; set; }
        public string Nome { get; set; } = "Sobrevivente";
        public int X { get; set; }
        public int Y { get; set; }
        public int Vida { get; set; }
        public int VidaMaxima { get; set; }
        public int Fome { get; set; }
        public int Sede { get; set; }
        public int Experiencia { get; set; }
        public int Forca { get; set; }
        public int DefesaBase { get; set; }
        public int Capacidade { get; set; }
        public List<ItemSalvo> Itens { get; set; } = new();
        public string? NomeArmaEquipada { get; set; }
        public string? NomeArmaduraEquipada { get; set; }
        public bool TemAntena { get; set; }
        public bool TemBateria { get; set; }
        public bool TemFusivel { get; set; }
        public int SinosTocados { get; set; }
        public bool SegredoCatedralRevelado { get; set; }
    }

    /// <summary>
    /// Um item da mochila, achatado. O campo Tipo é o "discriminador": guarda qual
    /// subclasse de ItemInventario aquele registro era, porque JSON não tem
    /// herança. É o que permite reconstruir o objeto certo na volta.
    /// </summary>
    internal class ItemSalvo
    {
        public string Tipo { get; set; } = "";
        public string Nome { get; set; } = "";
        public string Descricao { get; set; } = "";
        public int Quantidade { get; set; }
        public char Simbolo { get; set; }

        public int Dano { get; set; }          // Arma
        public int Defesa { get; set; }        // Armadura
        public int Cura { get; set; }          // Consumivel
        public int RestauraFome { get; set; }  // Consumivel
        public int RestauraSede { get; set; }  // Consumivel

        /// <summary>Domínio -> DTO. O switch por tipo é polimorfismo na ida.</summary>
        public static ItemSalvo De(ItemInventario item)
        {
            var salvo = new ItemSalvo
            {
                Nome = item.Nome,
                Descricao = item.Descricao,
                Quantidade = item.Quantidade,
                Simbolo = item.Simbolo,
            };

            switch (item)
            {
                case Arma arma:
                    salvo.Tipo = nameof(Arma);
                    salvo.Dano = arma.Dano;
                    break;
                case Armadura armadura:
                    salvo.Tipo = nameof(Armadura);
                    salvo.Defesa = armadura.Defesa;
                    break;
                case Consumivel consumivel:
                    salvo.Tipo = nameof(Consumivel);
                    salvo.Cura = consumivel.Cura;
                    salvo.RestauraFome = consumivel.RestauraFome;
                    salvo.RestauraSede = consumivel.RestauraSede;
                    break;
            }

            return salvo;
        }

        /// <summary>DTO -> domínio. Devolve null se o Tipo for desconhecido (save
        /// de uma versão futura, por exemplo) - aí aquele item é ignorado em vez de
        /// derrubar o carregamento inteiro.</summary>
        public ItemInventario? Reconstruir() => Tipo switch
        {
            nameof(Arma) => new Arma(Nome, Descricao, Quantidade, Dano, Simbolo),
            nameof(Armadura) => new Armadura(Nome, Descricao, Quantidade, Defesa, Simbolo),
            nameof(Consumivel) => new Consumivel(Nome, Descricao, Quantidade, Simbolo, Cura, RestauraFome, RestauraSede),
            _ => null,
        };
    }
}
