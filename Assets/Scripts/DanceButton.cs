using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Spine.Unity.Examples {
    public class DanceButton : MonoBehaviour
    {
        [SerializeField] int danceCost;
        [SerializeField] int danceIndex;
        [SerializeField] string danceName;

        public CustomizationContoller skinsSystem;

        void Start() 
        {
            GetComponent<Button>().onClick.AddListener(() => {skinsSystem.UpdateAnimation(danceName);});
        }
    }
}
