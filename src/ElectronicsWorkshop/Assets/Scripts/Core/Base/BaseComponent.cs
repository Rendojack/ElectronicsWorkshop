using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class BaseComponent : MonoBehaviour
    {
        public Stopwatch LiveTimer = new Stopwatch();
        public List<Tag> Tags = new List<Tag>();

        public bool HasTag(Tag tag)
        {
            return Tags.Contains(tag);
        }

        public void Awake()
        {
            LiveTimer.Start();
        }
    }
}
