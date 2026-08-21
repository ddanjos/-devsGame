namespace SurvivorGame.Combate
{
    /// <summary>
    /// Um golpe do inimigo, como pedido no SCRUM-17. Diferente da Habilidade do
    /// jogador (que custa Energia), aqui o que importa é a MENSAGEM: a referência
    /// declarada do combate é Earthbound, onde parte da graça está em ataques que
    /// não fazem nada ("{nome} ficou encarando você... nada aconteceu!").
    ///
    /// EhAcaoNula liga exatamente isso: o ataque acontece, o texto aparece no log,
    /// mas nenhum dano é aplicado. Ver SessaoCombate.TurnoInimigo.
    /// </summary>
    internal class AtaqueInimigo
    {
        public string NomeAtaque { get; }

        /// <summary>Dano do golpe em si. A força do inimigo e a defesa do jogador
        /// entram depois, na fórmula do SessaoCombate - este número é só a "base".</summary>
        public int DanoBase { get; }

        /// <summary>Texto mostrado no log. Use {0} para o dano calculado; se for
        /// ação nula, escreva a frase inteira sem nenhum {0}.</summary>
        public string MensagemAtaque { get; }

        /// <summary>Ação que não causa dano nenhum - só o texto engraçado.</summary>
        public bool EhAcaoNula { get; }

        public AtaqueInimigo(string nomeAtaque, int danoBase, string mensagemAtaque, bool ehAcaoNula = false)
        {
            NomeAtaque = nomeAtaque;
            DanoBase = danoBase;
            MensagemAtaque = mensagemAtaque;
            EhAcaoNula = ehAcaoNula;
        }
    }
}
