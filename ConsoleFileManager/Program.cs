using System.Runtime.InteropServices;

namespace ConsoleFileManager
{
    internal class Program
    {
        private delegate bool ConsoleCtrlHandler(int sig);

        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(ConsoleCtrlHandler handler, bool add);
        

        public static void Main(string[] args)
        {
            var fm = FileManager.FileManager.GetInstance();
            
            // apply console closing event handler that will save the File Manager state
            SetConsoleCtrlHandler(
                s => {
                    fm.SaveState();
                    return false;
                }, 
                true
            );
            
            fm.Loop();
            
            fm.SaveState();
        }
    }
}