using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Feature
{
    public class Wire
    {
        public Transform H_1 { get; }
        public Transform H_2 { get; }

        public Wire(Transform H_1, Transform H_2)
        {
            this.H_1 = H_1;
            this.H_2 = H_2;
        }

        public bool WiredTo(Transform H)
        {
            return H_1 == H || H_2 == H;
        }
    }
}
