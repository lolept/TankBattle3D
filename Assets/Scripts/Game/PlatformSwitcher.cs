using System;
using UnityEngine;

namespace Game
{
    public class PlatformSwitcher : MonoBehaviour
    {
        [SerializeField] private GameObject androidControls;

        private void Awake()
        {
            androidControls.SetActive(Application.isMobilePlatform);
        }
    }
}
