using System;
using System.Windows;

namespace MessagePackUnpacker
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                string _input = string.Empty;
                if (args.Length == 1 && (args[0] == "--help" || args[0] == "-h"))
                {
                    Util.ShowHelp();
                    return;
                }
                if (args.Length == 2 && (args[0] == "--input" || args[0] == "-i"))
                {
                    _input = args[1];
                }
                else if (args.Length == 1)
                {
                    _input = args[0];
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("エラー: 引数が不正です!!!");
                    Util.ShowHelp();
                    Console.WriteLine();
                    return;
                }
                Util.DeserializeInput(_input);
                return;
            }

            // GUIモード
            var app = new Application();
            var mainWindow = new MainWindow();
            app.Run(mainWindow);
        }
    }
}
