using System;
using System.Windows.Forms;

namespace FashionShop.Presentation
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            using (var login = new LoginForm())
            {
                if (login.ShowDialog() != DialogResult.OK)
                    return; // bấm X hoặc login fail => thoát
            }

            Application.Run(new MainForm());
        }


    }
}
