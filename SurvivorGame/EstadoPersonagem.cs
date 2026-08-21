namespace SurvivorGame
{
    /// <summary>
    /// "Estado" no sentido que a disciplina pede (Turno, Rodada, Iniciativa,
    /// Estado, Condição de Vitória). Ver Personagem.Estado - é derivado de
    /// Vida/Fome/Sede, não guardado num campo, pra nunca ficar dessincronizado.
    /// </summary>
    internal enum EstadoPersonagem
    {
        /// <summary>Vivo, sem passar fome nem sede.</summary>
        Saudavel,

        /// <summary>Vivo, mas com Fome ou Sede zerada - perde vida a cada ação.</summary>
        Debilitado,

        /// <summary>Vida chegou a zero - condição de derrota.</summary>
        Morto
    }
}
