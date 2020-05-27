using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.NonGameTools
{
    class PrefabIconGrabber : MonoBehaviour
    {
        [SerializeField] private GameObject _prefab;
        [SerializeField] private string _savePath;
        [SerializeField] private string _saveName;

        public bool Execute = false;

        public void Update()
        {
            if (Execute)
            {
                Execute = false;
                SavePrefabIcon();
            }
        }
        public void SavePrefabIcon()
        {
            #if UNITY_EDITOR
            {
                Texture2D texture = UnityEditor.AssetPreview.GetAssetPreview(_prefab);

                byte[] bytes = texture.EncodeToPNG();
                File.WriteAllBytes(Path.Combine(_savePath, _saveName), bytes);
            }
            #endif
        }
    }
}
