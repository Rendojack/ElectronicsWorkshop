using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Feature
{
    public class OrderedInitializer : MonoBehaviour
    {
        private void Start()
        {
            IEnumerable<IOrderedInit> OIOs = FindObjectsOfType<MonoBehaviour>().OfType<IOrderedInit>();
            OIOs = OIOs.OrderBy(o => o.GetInitOrder()).ToList(); // Order by priority
         
            foreach (IOrderedInit OIO in OIOs)
            {
                OIO.InitOrdered();
            }
        }
    }
}
