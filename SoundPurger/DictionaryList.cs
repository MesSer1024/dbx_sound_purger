using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundPurger
{
    public class DictionaryList<TValue> : Dictionary<string, List<TValue>>
    {
        public DictionaryList(int capacity = 0)
            : base(capacity)
        { }

        public void Add(string key, TValue value)
        {
            if (this.ContainsKey(key))
                this[key].Add(value);
            else
            {
                this[key] = new List<TValue>() { value };
            }
        }

        public new void Add(string key, List<TValue> values)
        {
            if (this.ContainsKey(key))
                this[key].AddRange(values);
            else
            {
                this[key] = values;
            }
        }

        internal void Add(DictionaryList<TValue> values)
        {
            foreach (var item in values)
            {
                Add(item.Key, item.Value);
            }
        }
    }
}
