using System;
using UnityEngine;

namespace Mirror.EscapeGame.Gameplayer
{
    public class IGameplayControl
    {
        /// <summary>
        /// Delay sec and do callback.
        /// </summary>
        internal Action<float, Action> AbleToDo;

        internal Model model;
        internal Animator animator;
        internal Rigidbody2D rb;

        public virtual void Init(Model model, Animator animator, Rigidbody2D rb, Action<float, Action> delayAction)
        {
            this.rb = rb;
            this.model = model;
            this.animator = animator;
            this.AbleToDo = delayAction;
            Debug.Log(rb);
        }
    }
}