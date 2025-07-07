

using Huffman.data;

namespace Huffman
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            CommandLineArgs parsed = CommandLineArgs.Parse(args);
            //MessageBox.Show(parsed.IsCompress.ToString());
            //MessageBox.Show(parsed.Paths[0]);
            Form1 form = new Form1();
            form.Tag = parsed;

            Application.Run(form);
        }
    }
}