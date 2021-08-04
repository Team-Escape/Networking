using System;
using System.Collections.Generic;
using UnityEngine;

namespace Photon.Pun.Escape.GM
{
    public class CoreView : MonoBehaviour
    {
        public static CoreView instance;
        [SerializeField] TransitionEffect transition;

        private void Awake()
        {
            if (instance != null)
            {
                Destroy(this);
                return;
            }
            else
            {
                instance = this;
            }
        }

        public void ChangeSceneWithMask(Action callback)
        {
            transition.MaskIn(() => MaskoutWhenLoaded(callback));
        }

        private void MaskoutWhenLoaded(Action callback)
        {
            callback();
            transition.MaskOut();
        }
    }
}