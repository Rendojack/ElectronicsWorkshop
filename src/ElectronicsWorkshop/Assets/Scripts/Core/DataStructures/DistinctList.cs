using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    [Serializable]
    public class DistinctList<T> : List<T> 
    {
        [SerializeField] private List<T> _items = new List<T>();

        public new void Add(T item)
        {
            if(!_items.Contains(item))
            {
                _items.Add(item);
            }
        }
    }
}
