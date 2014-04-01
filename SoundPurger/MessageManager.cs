using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundPurger
{
    public interface IMessage
    {
    }

    public interface IMessageListener
    {
        void onMessage(IMessage msg);
    }

    static class MessageManager
    {
        public static List<IMessageListener> _listeners = new List<IMessageListener>();

        public static void addListener(IMessageListener listener)
        {
            _listeners.Add(listener);
        }

        public static void removeListener(IMessageListener listener)
        {
            _listeners.Remove(listener);
        }

        public static void sendMessage(IMessage msg)
        {
            var items = _listeners.ToArray();
            foreach (var item in items)
            {
                item.onMessage(msg);
            }
        }
    }
}
