namespace SurvivorGame.Mapa
{
    /// <summary>
    /// Tipos de terreno possíveis nos mapas do jogo.
    /// Chao/Parede -> usados no mapa de masmorra (desenho original).
    /// Os demais -> usados no mapa da cidade de Blumenau (baseado no mapa real).
    /// </summary>
    internal enum TileType
    {
        Chao,
        Parede,
        Agua,       // rio Itajaí-Açu
        Ponte,      // travessia sobre o rio
        Floresta,   // mata/encosta
        Parque,     // area verde urbana (ex: Parque Sao Francisco de Assis)
        Rua,        // area urbana caminhavel (ruas, calcadas, pracas)
        Predio,     // quarteirao/edificacao (bloqueia passagem)
        Rodovia,    // via principal (ex: Rod. Jorge Lacerda)
        Inicio,     // ponto de partida do personagem (ProWay)
        PontoTuristico // marco turistico (museu, mausoleu, ponto historico...)
    }
}
