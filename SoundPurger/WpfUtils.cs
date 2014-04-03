using System;
using System.ComponentModel;
namespace SoundPurger
{
    static class WpfUtils
    {
        public static System.Windows.Threading.Dispatcher MainDispatcher { get; set; }
    
        public static void delayCall(Action action, int delayMS = 10 )
        {
            new System.Threading.Timer(obj =>
                {
                    action.Invoke();
                }, 
                null, 
                delayMS, 
                System.Threading.Timeout.Infinite
                );
    	
        }

        public static void toMainThread(Action action)
        {
            MainDispatcher.BeginInvoke(action);
        }

        public static void createBgThread(Action action)
        {
            var bw = new BackgroundWorker();
            bw.DoWork += (sender, args) =>
            {
                action.Invoke();
            };
            bw.RunWorkerAsync();

        }
    }
}
