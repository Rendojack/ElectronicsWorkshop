using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Feature
{
    public interface IOrderedInit
    {
        void InitOrdered();
        int GetInitOrder();
    }
}
