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

            this.Surface.Print(width / 2 - titulo.Length / 2, height / 3, titulo, Color.Red);
            this.Surface.Print(width / 2 - subTitulo.Length / 2, (height / 3) + 2, subTitulo, Color.Gray);

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
