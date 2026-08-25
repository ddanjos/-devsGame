using System;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;

namespace SurvivorGame.Ui
{
    // Mudamos para ScreenSurface para acabar com o conflito com System.Console
    public class MenuPrincipalScreen : ScreenSurface
    {
        public MenuPrincipalScreen(int width, int height) : base(width, height)
        {
            // Desenha os textos na tela usando a propriedade Surface nativa
            string titulo = "Survivor Blu";
            string subTitulo = "Blumenau Apocaliptica";
            string instrucao1 = "[ ENTER ] Iniciar Novo Jogo";
            string instrucao2 = "[ ESC ] Sair do Jogo";

<<<<<<< HEAD
            // 1. Criamos uma superfície dedicada ao título com metade da largura necessária (pois a fonte vai dobrar)
            // E com altura 1 (que virará 2 linhas de altura na tela)
            var surfaceTitulo = new ScreenSurface(titulo.Length, 1);

            // 2. Multiplicamos o tamanho da fonte por 4 (X: 4, Y: 4)
            surfaceTitulo.FontSize = surfaceTitulo.Font.GetFontSize(SadConsole.IFont.Sizes.Four);

            // 3. Desenhamos o título nesta superfície filha (na posição 0, 0 dela)
            surfaceTitulo.Surface.Print(0, 0, titulo, new Color(52, 235, 143));

            // 4. Posicionamos a superfície do título de forma centralizada na tela principal
            // Como a fonte dobrou, o título ocupará (titulo.Length * 2) de largura na tela principal
            int tituloLarguraReal = titulo.Length * 2;
            surfaceTitulo.Position = new SadRogue.Primitives.Point((width - tituloLarguraReal) / 12, height / 12);

            // 5. Adicionamos a superfície do título como filha desta tela para que ela seja renderizada
            this.Children.Add(surfaceTitulo);


            // --- O restante do seu texto continua na superfície principal ---

            // O subtítulo precisa descer um pouco mais, já que o título agora ocupa 2 linhas de altura
            this.Surface.Print(width / 2 - subTitulo.Length / 2, (height / 3) + 3, subTitulo, Color.Gray);
=======
    // 1. Criamos uma superfície dedicada ao título com metade da largura necessária (pois a fonte vai dobrar)
    // E com altura 1 (que virará 2 linhas de altura na tela)
    var surfaceTitulo = new ScreenSurface(titulo.Length, 1);
>>>>>>> 9945afb614f6613d19b9add0766260c7a5c5e823

            // 2. Multiplicamos o tamanho da fonte por 4 (X: 4, Y: 4)
            surfaceTitulo.FontSize = surfaceTitulo.Font.GetFontSize(SadConsole.IFont.Sizes.Four);

    // 3. Desenhamos o título nesta superfície filha (na posição 0, 0 dela)
    surfaceTitulo.Surface.Print(0, 0, titulo, new Color(52, 235, 143));

    // 4. Posicionamos a superfície do título de forma centralizada na tela principal
    // Como a fonte dobrou, o título ocupará (titulo.Length * 2) de largura na tela principal
    int tituloLarguraReal = titulo.Length * 2;
    surfaceTitulo.Position = new SadRogue.Primitives.Point((width - tituloLarguraReal) / 12, height / 12);

    // 5. Adicionamos a superfície do título como filha desta tela para que ela seja renderizada
    this.Children.Add(surfaceTitulo);


    // --- O restante do seu texto continua na superfície principal ---
    
    // O subtítulo precisa descer um pouco mais, já que o título agora ocupa 2 linhas de altura
    this.Surface.Print(width / 2 - subTitulo.Length / 2, (height / 3) + 3, subTitulo, Color.Gray);

    this.Surface.Print(width / 2 - instrucao1.Length / 2, (height / 2), instrucao1, Color.White);
    this.Surface.Print(width / 2 - instrucao2.Length / 2, (height / 2) + 2, instrucao2, Color.White);
        }

        // Este método captura com precisão as teclas pressionadas na tela do Menu
        public override bool ProcessKeyboard(Keyboard keyboard)
        {
            // Se pressionar Enter, inicia a partida chamando o Program
            if (keyboard.IsKeyPressed(Keys.Enter))
            {
                Program.IniciarNovaPartida();
                return true;
            }

            // Se pressionar Esc, fecha o aplicativo com segurança
            if (keyboard.IsKeyPressed(Keys.Escape))
            {
                Game.Instance.MonoGameInstance.Exit();
                return true;
            }

            return base.ProcessKeyboard(keyboard);
        }
    }
}
