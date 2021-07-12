using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mirror.EscapeGame
{
    public class TransitionEffect : NetworkBehaviour
    {
        bool isPlaying = false;
        [SerializeField] MaskTransitionContainer maskContainer = null;

        [Command]
        public void CmdMaskIn() => RpcMaskIn();
        [Client]
        public void RpcMaskIn() => MaskIn(null);

        public void UpdateMaskUI(bool state)
        {
            maskContainer.GetSelf.SetActive(state);
        }

        public void PlayMaskAnimation(string name)
        {
            maskContainer.GetTransitionAnimator.Play(name);
        }

        public void MaskIn(System.Action callback)
        {
            if (isPlaying) return;
            CmdMaskIn();

            string name = "MaskIn";
            UpdateMaskUI(true);
            PlayMaskAnimation(name);
            StartCoroutine(WaitForPlay(name, callback));
        }

        public void MaskOut()
        {
            string name = "MaskOut";
            PlayMaskAnimation(name);
            StartCoroutine(WaitForPlay(name, () => UpdateMaskUI(false)));
        }

        IEnumerator WaitForPlay(string name, System.Action callback)
        {
            isPlaying = true;
            AnimationClip[] clips = maskContainer.GetTransitionAnimator.runtimeAnimatorController.animationClips;
            float length = 0;

            foreach (AnimationClip clip in clips)
            {
                if (clip.name == name)
                {
                    length = clip.length;
                    break;
                }
            }
            yield return new WaitForSeconds(length);
            isPlaying = false;
            if (callback != null) callback();
        }

        private void Awake()
        {
            DontDestroyOnLoad(this);
        }
    }

    [System.Serializable]
    public class MaskTransitionContainer
    {
        public GameObject GetSelf { get { return selfObject; } }
        [SerializeField] GameObject selfObject = null;
        public Animator GetTransitionAnimator { get { return transitionAnimator; } }
        [SerializeField] Animator transitionAnimator = null;
    }
}