using System.Collections.Generic;
using UnityEngine;

namespace _RD3._Universal.Scripts.GOAP.GOAPBASE
{
    public class GIventory
    {
        private List<GameObject> _items = new List<GameObject>();

        public void AddItem(GameObject i)
        {
            _items.Add(i);
        }
        
        public GameObject FindItemWithTag(string tag)
        {
            foreach (var item in _items)
            {
                if (item.CompareTag(tag))
                    return item;
            }
            return null;
        }
        
        public void RemoveItem(GameObject i)
        {
            _items.Remove(i);
        }
    }
}
