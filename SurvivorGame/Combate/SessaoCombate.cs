using System;
using SurvivorGame.Regras;

namespace SurvivorGame.Combate
{
    /// <summary>
    /// O "motor" de UMA batalha: guarda a Energia acumulada, se o jogador está
    /// defendendo, e aplica as regras (dano, cura, vitória/derrota). Não sabe
    /// nada de tela/SadConsole - isso fica no CombateScreen.
    ///
    /// Importante: como essa classe é criada do zero a cada combate (veja o
    /// construtor de CombateScreen) e descartada ao sair, a Energia NUNCA
    /// sobrevive entre batalhas - exatamente o que foi pedido na spec.
    /// </summary>
    internal class SessaoCombate
    {
        public Personagem Jogador { get; }
        public Inimigo Inimigo { get; }
        public int Energia { get; private set; }
        public bool Defendendo { get; private set; }
        public Habilidade AtaqueBasico { get; }

        public SessaoCombate(Personagem jogador, Inimigo inimigo)
        {
            Jogador = jogador;
            Inimigo = inimigo;
            Energia = 0;
            Defendendo = false;
            AtaqueBasico = new Habilidade("Ataque", jogador.DanoBase);
        }

        /// <summary>Chamem no início de cada turno do jogador (inclusive o primeiro): ganha 1 de Energia e encerra o buff de Defender.</summary>
        public void IniciarTurnoJogador()
        {
            Energia++;
            Defendendo = false;
        }

        /// <summary>Usa uma habilidade (ataque básico ou especial) contra o inimigo.</summary>
        public string Atacar(Habilidade habilidade)
        {
            if (habilidade.CustoEnergia > Energia)
                return $"Energia insuficiente para usar {habilidade.Nome}.";

            Energia -= habilidade.CustoEnergia;
            Inimigo.ReceberDano(habilidade.Dano);
            return $"{Jogador.Nome} usou {habilidade.Nome} causando {habilidade.Dano} de dano.";
        }

        /// <summary>Reduz o próximo dano do inimigo pela metade. O buff acaba quando o próximo turno do jogador começa.</summary>
        public string Defender()
        {
            Defendendo = true;
            return $"{Jogador.Nome} se defendeu.";
        }

        /// <summary>Usa um item consumível do próprio inventário do jogador.</summary>
        public string UsarItem(string nomeItem)
        {
            var consumivel = AcoesJogador.UsarItem(Jogador, nomeItem);
            return consumivel != null
                ? $"{Jogador.Nome} usou {consumivel.Nome} e recuperou {consumivel.Cura} de vida."
                : $"Não foi possível usar {nomeItem}.";
        }

        /// <summary>Tenta fugir da batalha. Por enquanto sempre funciona.</summary>
        public string Fugir()
        {
            // Se quiserem chance de falha: sorteiem aqui e, se falhar, deixem o
            // CombateScreen chamar TurnoInimigo() em vez de encerrar o combate.
            return $"{Jogador.Nome} fugiu da batalha!";
        }

        /// <summary>O inimigo escolhe uma habilidade aleatória e ataca. Dano reduzido pela metade se o jogador estiver defendendo.</summary>
        public string TurnoInimigo()
        {
            if (Inimigo.Habilidades.Count == 0)
                return $"{Inimigo.Nome} não fez nada.";

            Habilidade habilidade = Inimigo.Habilidades[Random.Shared.Next(Inimigo.Habilidades.Count)];
            int dano = Defendendo ? habilidade.Dano / 2 : habilidade.Dano;

            Jogador.ReceberDano(dano);

            return $"{Inimigo.Nome} usou {habilidade.Nome} dando {dano} de dano.";
        }

        public ResultadoCombate VerificarResultado()
        {
            if (Jogador.Vida <= 0) return ResultadoCombate.Derrota;
            if (!Inimigo.EstaVivo) return ResultadoCombate.Vitoria;
            return ResultadoCombate.EmAndamento;
        }
    }
}