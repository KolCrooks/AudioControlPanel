using System.Windows.Forms;

namespace AudioControllerDevice
{
    class Program
    {



        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            UI ui = new UI();

            Application.Run(ui);
        }


        public static void nextPrevSong(int dir = 1)
        {
            
            KeyBoard.SendKeyPress(0xB0);
        }
    }
}
