# O Último Sinal — narrativa do SurvivorGame

Documento de narrativa do jogo: a premissa, o gancho de cada ponto do mapa, e a
condição de vitória. Serve como referência pra quem for implementar os locais —
cada seção termina com uma nota do que já existe no código e do que falta.

Mantido junto do código de propósito: quando um local for implementado de
verdade, atualizem a nota técnica dele aqui no mesmo commit.

---

## Premissa

Blumenau, depois do **Evento** — a causa fica deliberadamente vaga: dá liberdade
criativa e combina com o tom que já tínhamos definido como referência (Earthbound
/ Pokémon), onde o perigo é real mas há espaço pra humor. Não é terror.

O jogador acorda sozinho no escritório da ProWay e encontra um rádio quebrado,
faltando três peças: **Antena**, **Bateria** e **Fusível**. O objetivo é explorar
Blumenau, reunir as três peças e transmitir um pedido de socorro — antes que
Fome, Sede ou HP acabem primeiro.

Isso dá sentido narrativo ao esqueleto de condição de vitória que **já existe** em
`Regras/GerenciadorJogo.cs` (`TemAntena` / `TemBateria` / `TemFusivel` /
`PodeTransmitir`), que até agora estava implementado mas sem nenhuma história por
trás e sem nenhum gatilho de fim de jogo.

---

## As três peças (condição de vitória)

| Peça | Onde | Como se consegue | Estado no código |
|---|---|---|---|
| **Antena** | Cafeteria / Andar 0 da ProWay | Derrotando o Rato Selvagem, que fez ninho nela | ✅ Funciona — `ProcessarVitoriaInimigo` já concede `TemAntena`. Falta só a mensagem citar a antena |
| **Bateria** | Museu Hering | Derrotando o Vira-Lata Alfa que guarda o armazém | ❌ Local e inimigo ainda não existem |
| **Fusível** | Prefeitura Municipal | Vasculhando a sala de manutenção elétrica (sem combate) | ❌ Local ainda não existe |

Reunidas as três, `GerenciadorJogo.PodeTransmitir` vira `true` e uma ação nova —
**"Consertar e transmitir o rádio"** — deve aparecer no `LocalEscritorioProway`.
Executá-la é a vitória do jogo. **Ainda não implementado**: hoje `PodeTransmitir`
não é consultado em lugar nenhum.

---

## ProWay (Ato 0)

### Escritório da ProWay — `Mapa/LocalEscritorioProway.cs` — hub, chave

> "Você acorda no chão, a cabeça latejando. Na mesa ao lado, um rádio antigo —
> sem antena, sem bateria, o fusível queimado. Alguém o deixou ali de propósito,
> ou foi você mesmo que tentou consertar antes de apagar?"

O rádio quebrado é o gancho central: aparece no primeiro minuto de jogo e nomeia
as três peças, dando um objetivo concreto em vez de só "explore por aí". É também
o hub — elevador desce pro Andar 0, escada desce pro porão, e é aqui que a ação
de transmissão vai aparecer no fim.

**Já existe:** as ações de elevador, escada, descansar e sair.
**Falta:** a ação condicional de transmissão (ver tabela acima).

### Cafeteria / Andar 0 — `Mapa/LocalAndarZero.cs` — chave (Antena)

> "Mesas viradas, cadeiras espalhadas — só o suficiente pra parecer que alguém
> saiu correndo, não que ninguém mais existe. Um ninho de rato ocupou o que
> sobrou de uma antena de rádio guardada atrás do balcão."

A ação "Procurar restos de comida" já sorteia entre achar comida, encontrar o
Rato Selvagem ou não achar nada. O gancho só reescreve o *motivo* do combate: o
rato não ataca à toa, está defendendo o ninho — que por acaso é a Antena.

**Já existe:** o sorteio (15% combate / 40% item / resto nada) e a concessão da
Antena na vitória.
**Falta:** mencionar a antena visível no ninho, na mensagem do encontro.

### Porão — `Mapa/LocalPoraoProway.cs` — placeholder

> "A lanterna do celular mal alcança o fim do corredor. Tem caixas empilhadas,
> algumas já mofadas — e um barulho de gotejar que não parece ter fonte nenhuma."

Hoje "vasculhar as caixas" não acha nada. O texto da lanterna fraca disfarça isso
de forma honesta enquanto o conteúdo não existe.

**Falta:** portar o conteúdo da antiga `MapaMasmorra` (que existe pronta no
projeto) pro formato ponto-e-clique, se sobrar tempo.

---

## Blumenau (Ato 1)

Onze pontos turísticos reais, cada um com uma leitura pós-apocalíptica. A ordem de
visita é livre. **Nenhum destes está implementado como local jogável ainda** — hoje
são só entradas de texto em `Mapa/MapaCidadeBlumenau.Locais` que abrem uma tela de
descrição (`CenarioLocalScreen`), sem ações.

### Prefeitura Municipal — chave (Fusível)

> "O prédio enxaimel ainda de pé, orgulhoso, enquanto tudo ao redor caiu. Uma sala
> de manutenção nos fundos guarda o painel elétrico que mantinha a cidade
> funcionando."

Ação "Vasculhar a sala de manutenção": sem inimigo, mas com risco ambiental leve
(energia residual custa um pouco de HP se insistir). Entrega o Fusível. É o mais
fácil das três peças de propósito — dá ritmo, nem tudo precisa ser luta.

### Museu Hering — chave (Bateria)

> "Fardos de tecido viraram ninho pra alguma coisa que já não é bem cachorro. No
> armazém dos fundos, entre máquinas de costura enferrujadas, uma bateria
> industrial ainda pisca uma luzinha verde fraca."

O mais difícil das três peças, de propósito. Um **Vira-Lata Alfa** guarda o
armazém — mesma classe `Inimigo`/`Habilidade` já existente, só com números mais
altos (sugestão: vida 65).

### Castelinho da Havan — combate

> "De longe parece um castelo de verdade. De perto, as vitrines quebradas e as
> prateleiras reviradas contam outra história — alguém mais chegou aqui primeiro,
> e não quer dividir."

Um **Saqueador** — outro sobrevivente desesperado, não um monstro. Combate
opcional: dá pra fugir (`SessaoCombate.Fugir` já suporta) ou lutar por um item.

### Parque São Francisco de Assis — combate

> "Sem gente pra podar, o parque cresceu rápido demais. E não veio sozinho: uma
> matilha de cães assilvestrados decidiu que esse pedaço de mato é território
> deles."

O primeiro combate "de verdade" fora da ProWay — ensina que a cidade não é segura
nem nas áreas bonitas. O inimigo mais simples da lista (sugestão: vida 30), bom
primeiro passo pra expandir o roster.

### Parque Ramiro Ruediger — descanso

> "O maior parque da cidade, e o mais silencioso. No meio da pista de corrida
> abandonada, um bebedouro público ainda jorra água limpa — ninguém sabe explicar
> por quê, e ninguém está reclamando."

Zona segura de propósito: nenhum inimigo. "Beber na fonte" restaura Sede de graça,
dando um respiro depois de combates puxados. Importante pro ritmo.

**Falta no código:** `Personagem` tem `ConsumirFome`/`ConsumirSede`, mas não tem
`RestaurarFome`/`RestaurarSede` — precisa criar, espelhando os que já existem.

### Catedral São Paulo Apóstolo — descoberta / easter egg

> "Os vitrais coloridos ainda filtram luz estranha sobre os bancos vazios. Às
> vezes, sem ninguém tocar em nada, um dos sinos eletrônicos solta uma nota
> sozinho — energia residual, ou vontade própria."

Atmosférico e opcional. Bom candidato ao easter egg do backlog (SCRUM-15): voltar
três vezes faz os sinos tocarem uma melodia completa e revelarem algo. Implementa
com um contador `int` na própria classe do local.

### Museu da Cerveja — descoberta (tom leve)

> "O cheiro de malte fermentado ainda impregna o ar, anos depois. E onde tem
> fermentação abandonada, tem enxame — um zumbido alto vem de dentro dos barris."

Cômico de propósito: "Enxame de Mosquitos da Cervejaria" é engraçado, não
assustador. Combate fácil opcional com item bobo de recompensa, ou só flavor text
sem mecânica — as duas opções funcionam, é decisão de escopo.

### Museu de Hábitos e Costumes — descoberta

> "Vitrines com objetos do dia a dia de gente que morreu há mais de um século — e
> do lado delas, sobras de comida enlatada de gente que sumiu há semanas."

Baixo risco: item garantido, sem sorteio. Dá previsibilidade em meio ao acaso dos
outros locais.

### Museu da Família Colonial — lore

> "Um diário na estante ainda está aberto na última página escrita. Não é sobre os
> colonizadores — é de alguém que se escondeu aqui nos primeiros dias do Evento,
> tentando entender o que via pela janela."

Puro lore: um trecho de diário, sem item nem combate. O lugar certo pra semear
mistério sobre o Evento sem responder tudo. Sem custo de Fome/Sede — é leitura,
não esforço físico.

### Mausoléu Dr. Blumenau — lore

> "Alguém deixou flores frescas na porta do mausoléu — frescas de verdade, de dias
> atrás, não de décadas. Tem mais alguém vivo nessa cidade, e visita esse lugar."

Tensão social, não combate: planta a ideia de que existem outros sobreviventes sem
nunca mostrar um. Só texto — o mais barato de implementar da lista inteira.

---

## Condição de derrota

Fome ou Sede em 0 por tempo demais, ou HP zerando em combate sem item de cura. As
duas coisas já são tecnicamente possíveis com o que existe hoje em `Personagem` e
`SessaoCombate` — falta só o gatilho de "fim de jogo".

---

## Como isso encaixa na arquitetura

Nada aqui pede estrutura nova. Cada ideia acima é mais uma implementação de uma
interface que já existe:

- **Strategy** — `ILocalExploravel` / `IMapa`: cada local novo é só uma classe a
  mais implementando a mesma interface. Quem consome (`LocalExploravelScreen`,
  `MapaScreen`) não muda.
- **Command (implícito)** — `AcaoLocal` encapsula uma ação como objeto: texto +
  custo + o que executar. É um contrato pequeno e estável que qualquer local
  implementa sem expor como funciona por dentro.
- **State** — `Game.Instance.Screen` continua sendo a única coisa que troca de
  tela, seja combate, exploração ou cidade.
- **Factory + Flyweight** — `TileFactory` não muda com nada disso: segue cuidando
  só do terreno da cidade.

---

## Checklist de implementação

- [x] Ação condicional "Consertar e transmitir o rádio" no `LocalEscritorioProway`,
      visível só com `PodeTransmitir == true` → tela de vitória (`FimDeJogoScreen`)
- [x] `Personagem.RestaurarFome(int)` e `RestaurarSede(int)`
- [x] Inimigos novos: Vira-Lata Alfa, Saqueador, Cão Assilvestrado, Enxame de
      Mosquitos — todos em `Combate/FabricaInimigos.cs`
- [x] Os 11 locais da cidade como `ILocalExploravel`, montados por
      `Mapa/FabricaLocais.cs`
- [x] Gatilho de derrota: HP em 0 (combate ou inanição) → `FimDeJogoScreen`
- [x] Fome/Sede em 0 causam dano contínuo, dentro e fora de combate
- [x] `Personagem.Estado` (`EstadoPersonagem`: Saudável / Debilitado / Morto) —
      fecha a terminologia pedida pela disciplina junto de Turno, Rodada,
      Iniciativa e Condição de Vitória
- [ ] Portar `MapaMasmorra` pro porão em formato ponto-e-clique (único
      placeholder que sobrou)

---

## Caminho garantido da vitória (pra testar / apresentar)

As três peças são **determinísticas** de propósito — nenhuma depende de sorteio,
justamente pra a partida nunca travar numa demo:

1. **ProWay** → "Chamar o elevador" → **Cafeteria** → "Mexer no ninho atrás do
   balcão" → vencer o Rato Selvagem → **Antena** (1/3)
2. Sair pra rua → andar até a **Prefeitura** → "Vasculhar a sala de manutenção"
   → **Fusível** (2/3), custa 8 de vida
3. Andar até o **Museu Hering** → "Entrar no armazém dos fundos" → vencer o
   Vira-Lata Alfa (o mais difícil, vida 65) → **Bateria** (3/3)
4. Voltar pra **ProWay** → a ação `*** Consertar e transmitir o rádio ***`
   agora aparece no topo da lista → **vitória**

Se a vida estiver baixa antes do Vira-Lata, o **Parque Ramiro Ruediger** é zona
segura: "Beber na fonte" (+40 Sede, de graça) e "Dormir na grama" (+35 vida).
