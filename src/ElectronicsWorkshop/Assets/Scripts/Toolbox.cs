using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Feature
{
    public class Toolbox : MonoBehaviour, IOrderedInit
    {
        [SerializeField] private GameObject _wiringTool;
        [SerializeField] private GameObject _cuttingTool;

        public void PopulateInventory()
        {
            GameEvents.current.FireEvent_InventoryItemAdd(_wiringTool.GetComponent<Image>().sprite, _wiringTool, 
                "Jungimo įrankis - naudojamas sujungti prototipavimo lentos mazgus arba\n" +
                "prototipavimo lentos mazgą su elektronikos komponentu\n\n" +
                "Spauskite kairį pelės mygtuką ant prototipavimo lentos mazgų");

            GameEvents.current.FireEvent_InventoryItemAdd(_cuttingTool.GetComponent<Image>().sprite, _cuttingTool, 
                "Atjungimo/šalinimo įrankis - pašalina laidus ir elektronikos komponentus\n\n" +
                "Spauskite kairį pelės mygtuką ant prototipavimo lentos mazgų arba\n" +
                "elektronikos komponentų");
        }

        public void InitOrdered()
        {
            PopulateInventory();
        }

        public int GetInitOrder()
        {
            return 2;
        }
    }
}

