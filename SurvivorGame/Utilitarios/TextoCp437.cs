using System.Collections.Generic;
using System.Text;
using SadConsole;
using SadRogue.Primitives;

namespace SurvivorGame.Utilitarios
{
    /// <summary>
    /// Corrige os acentos na tela.
    ///
    /// O PROBLEMA: a fonte padrão do SadConsole é a IBM CP437 - a mesma dos
    /// terminais DOS - e o Print pega o código do char e usa direto como índice do
    /// glifo. Em Unicode, 'ç' é o código 231; no CP437, o glifo 231 é 'τ'. Por isso
    /// "coração" aparecia como "coraτπo": não é bug de encoding do arquivo .cs, é
    /// o índice do glifo batendo em outro desenho.
    ///
    /// A SOLUÇÃO: traduzir a string pro CP437 ANTES de imprimir. 'ç' vira o índice
    /// 135, que é o desenho certo. Fica correto ç, á, é, í, ó, ú, â, ê, ô, à, ü, ñ
    /// e as maiúsculas Ç e É.
    ///
    /// A EXCEÇÃO HONESTA: o CP437 é de 1981 e simplesmente NÃO TEM 'ã' nem 'õ' -
    /// não existe glifo pra desenhar. Esses dois são rebaixados pra 'a' e 'o'
    /// ("coração" -> "coraçao"). Resolver isso de verdade exigiria trocar a fonte
    /// do jogo por uma com suporte a Latin-1 completo, o que é um trabalho de arte,
    /// não de código. Fica registrado como dívida técnica.
    ///
    /// Uso: chame Surface.PrintTexto(...) no lugar do Print do SadConsole. A assinatura
    /// é a mesma, então é troca direta.
    /// </summary>
    internal static class TextoCp437
    {
        /// <summary>Unicode -> índice do glifo no CP437, só pros caracteres que a
        /// gente realmente usa em português. O resto do texto (ASCII) já bate.</summary>
        private static readonly Dictionary<char, char> Mapa = new()
        {
            ['Ç'] = (char)128, ['ü'] = (char)129, ['é'] = (char)130, ['â'] = (char)131,
            ['ä'] = (char)132, ['à'] = (char)133, ['å'] = (char)134, ['ç'] = (char)135,
            ['ê'] = (char)136, ['ë'] = (char)137, ['è'] = (char)138, ['ï'] = (char)139,
            ['î'] = (char)140, ['ì'] = (char)141, ['Ä'] = (char)142, ['Å'] = (char)143,
            ['É'] = (char)144, ['ô'] = (char)147, ['ö'] = (char)148, ['ò'] = (char)149,
            ['û'] = (char)150, ['ù'] = (char)151, ['Ö'] = (char)153, ['Ü'] = (char)154,
            ['á'] = (char)160, ['í'] = (char)161, ['ó'] = (char)162, ['ú'] = (char)163,
            ['ñ'] = (char)164, ['Ñ'] = (char)165, ['ª'] = (char)166, ['º'] = (char)167,
            ['¿'] = (char)168, ['¡'] = (char)173, ['«'] = (char)174, ['»'] = (char)175,

            // Sem glifo no CP437 - rebaixados pro caractere base, de propósito.
            ['ã'] = 'a', ['Ã'] = 'A', ['õ'] = 'o', ['Õ'] = 'O',
            ['Á'] = 'A', ['Â'] = 'A', ['À'] = 'A', ['Í'] = 'I',
            ['Ó'] = 'O', ['Ô'] = 'O', ['Ú'] = 'U', ['Ê'] = 'E',

            // Aspas e travessões "inteligentes" que o Word/VS às vezes injetam.
            ['‘'] = '\'', ['’'] = '\'', ['“'] = '"', ['”'] = '"',
            ['–'] = '-', ['—'] = '-', ['…'] = '.',
        };

        /// <summary>Traduz a string pros índices de glifo do CP437.</summary>
        public static string Converter(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return texto;

            var saida = new StringBuilder(texto.Length);
            foreach (char c in texto)
                saida.Append(Mapa.TryGetValue(c, out char glifo) ? glifo : c);

            return saida.ToString();
        }

        // Sobrecargas que espelham as do Print do SadConsole, pra troca ser direta.

        public static void PrintTexto(this ICellSurface superficie, int x, int y, string texto) =>
            superficie.Print(x, y, Converter(texto));

        public static void PrintTexto(this ICellSurface superficie, int x, int y, string texto, Color frente) =>
            superficie.Print(x, y, Converter(texto), frente);

        public static void PrintTexto(this ICellSurface superficie, int x, int y, string texto, Color frente, Color fundo) =>
            superficie.Print(x, y, Converter(texto), frente, fundo);
    }
}
