using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Returns the index of the first element in the sequence 
        /// that satisfies a condition.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source"/>.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IEnumerable{T}"/> that contains
        /// the elements to apply the predicate to.
        /// </param>
        /// <param name="predicate">
        /// A function to test each element for a condition.
        /// </param>
        /// <returns>
        /// The zero-based index position of the first element of <paramref name="source"/>
        /// for which <paramref name="predicate"/> returns <see langword="true"/>;
        /// or -1 if <paramref name="source"/> is empty
        /// or no element satisfies the condition.
        /// </returns>
        public static int IndexOf<TSource>(this IEnumerable<TSource> source,
            Func<TSource, bool> predicate)
        {
            int i = 0;

            foreach (TSource element in source)
            {
                if (predicate(element))
                    return i;

                i++;
            }

            return -1;
        }
    }
}
