using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mirror.EscapeGame.Gameplayer
{
    public class Combat : IGameplayControl
    {
        public PlayerState CurrenPlayerState { get { return model.CurrentPlayerState; } set { model.CurrentPlayerState = value; } }

        public PlayerState AsWaiting { get { return PlayerState.Waiting; } }
        public PlayerState AsEscaper { get { return PlayerState.Escaper; } }
        public PlayerState AsHunter { get { return PlayerState.Hunter; } }
        public PlayerState AsSpectator { get { return PlayerState.Spectator; } }


        public float GetHealthAmount
        {
            get
            {
                return (float)model.CurrentHealth / (float)model.MaxHealth;
            }
        }

        public void Attack()
        {
            if (model.CurrentPlayerState == PlayerState.Hunter)
                animator.DoAnimation("attack");
        }
        public void Hurt(System.Action callback)
        {
            if (model.CurrentPlayerState == PlayerState.Spectator) return;
            if (model.Shielding == false)
            {
                switch (model.CurrentPlayerState)
                {
                    case PlayerState.Invincible:
                        animator.DoAnimation("invincible");
                        return;
                    case PlayerState.Lockblood:
                        if (model.CurrentHealth > 1)
                            model.CurrentHealth--;
                        animator.DoAnimation("hurt");
                        break;
                    default:
                        model.CurrentHealth--;
                        if (model.CurrentHealth <= 0)
                        {
                            model.CurrentHealth = 0;
                            callback();
                            return;
                        }
                        animator.DoAnimation("hurt");
                        break;
                }
            }
            else model.Shielding = false;
        }
        public void Dead()
        {
            CurrenPlayerState = PlayerState.Dead;
            animator.DoAnimation("dead");
        }
        public void Reborn()
        {
            if (CurrenPlayerState != PlayerState.Dead) return;
            CurrenPlayerState = PlayerState.Reborn;
            AbleToDo(0.5f,
                () => CurrenPlayerState = model.teamID == 1
                ? PlayerState.Escaper
                : PlayerState.Hunter
            );
            model.CurrentHealth = model.MaxHealth;
            animator.DoAnimation("reborn");
        }
        public void Mutate(Transform transform)
        {
            CurrenPlayerState = PlayerState.Spectator;

            int playerLayer = LayerMask.NameToLayer("Player");
            int specatorLayer = LayerMask.NameToLayer("Invisible");

            // Open all layer on culling mask.
            Camera cam = transform.parent.GetChild(1).GetComponent<Camera>();
            cam.cullingMask = -1;

            animator.DoAnimation("reborn");

            // Set all layers to invisible
            SearchForAllChild(transform.parent, playerLayer, specatorLayer);
        }

        void SearchForAllChild(Transform t, int layerToBeChanged, int layerToChange)
        {
            if (t == null) return;
            foreach (Transform c in t)
            {
                SearchForAllChild(c, layerToBeChanged, layerToChange);
                if (c.gameObject.layer == layerToBeChanged)
                {
                    c.gameObject.layer = layerToChange;
                }
            }
        }
    }
}