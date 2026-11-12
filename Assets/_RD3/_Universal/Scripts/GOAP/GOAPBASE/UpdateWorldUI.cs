using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _RD3._Universal.Scripts.GOAP.GOAPBASE
{
    public class UpdateWorldUI : MonoBehaviour
    {
        [SerializeField]private Text statesText;

        private void LateUpdate()
        {
            var worldStates = GWorld.instance.GetWorld.GetStates();
            statesText.text = "";
            foreach (var s in worldStates)
            {
                statesText.text += s.Key + ", " + s.Value + "\n";
            } 
        
        }
    }
}
