using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mirror.EscapeGame.Gameplayer
{
    public class Control : MonoBehaviour
    {
        #region Classes
        View view;
        Model model;
        Mover mover;
        Combat combat;
        #endregion

        #region  UnityComponents
        Rigidbody2D rb;
        Animator animator;
        #endregion

        private void Awake()
        {
            view = GetComponent<View>();
            model = GetComponent<Model>();
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
        }

        private void OnEnable()
        {
            mover = new Mover();
            mover.Init(model, animator, rb, this.AbleToDo);
            combat = new Combat();
            combat.Init(model, animator, rb, this.AbleToDo);
        }

        bool isAttacking = false;
        bool isHurting = false;

        bool OnDisable
        {
            get
            {
                if (mover == null || combat == null) return true;
                else return false;
            }
        }

        public void ChangePlayerState(PlayerState state) => model.CurrentPlayerState = state;

        public void GameSetup(int id, int currentEscaperCount)
        {
            switch (id)
            {
                case 0:
                    model.CurrentPlayerState = PlayerState.Escaper;
                    model.MaxHealth = 3;
                    break;
                case 1:
                    model.CurrentPlayerState = PlayerState.Hunter;
                    model.MaxHealth = 10;
                    model.CurrentHealth = 10;
                    model.StateSpeedGain = 1.1f + currentEscaperCount * 0.1f;
                    model.StateJumpGain = 0.9f + currentEscaperCount * 0.1f;
                    break;
                default:
                    Debug.Log("Player teamID errors");
                    break;
            }
        }

        public void hunterDebuff(int currentEscaperCount)
        {
            model.StateSpeedGain = 1.1f + currentEscaperCount * 0.1f;
            model.StateJumpGain = 0.9f + currentEscaperCount * 0.1f;
        }
        public void Move(float value)
        {
            if (OnDisable) return;
            transform.localScale = new Vector2(
                transform.localScale.x >= 0 ?
                value >= 0 ? model.CharacterSize : model.CharacterSize * -1 :
                value <= 0 ? model.CharacterSize * -1 : model.CharacterSize
            , model.CharacterSize);
            mover.SetupInput = value;
        }
        public void Jump(bool isJumping)
        {
            if (OnDisable) return;
            mover.Jump(isJumping);
        }
        public void Run(bool value)
        {
            if (OnDisable) return;
            mover.Run(value);
        }
        public void GetStartItem(string name, System.Action callback)
        {
            if (model.IsGetStartItem == false)
            {
                model.IsGetStartItem = true;

                switch (name)
                {
                    case "IceSkate":
                        model.IceSkate = true;
                        break;
                    case "SlimeShoe":
                        model.SlimeShoe = true;
                        break;
                    case "Shield":
                        model.Shielding = true;
                        model.Shield = true;
                        break;
                    case "EnergyDrink":
                        model.EnergyDrink = true;
                        break;
                    case "Crucifixion":

                        model.Crucifixion = true;
                        break;
                    case "Armor":
                        model.AddItemSpeedGain = -0.1f;
                        model.AddItemJumpGain = -0.1f;
                        model.MaxHealth += 2;
                        model.CurrentHealth += 2;
                        model.Armor = true;
                        break;
                    case "LightnessShoe":
                        model.LightnessShoe = true;
                        model.AddItemSpeedGain = 0.1f;
                        model.AddItemJumpGain = 0.1f;
                        break;
                    case "RocketShoe":

                        model.RocketShoe = true;
                        break;
                    case "DeveloperObsession":

                        model.DeveloperObsession = true;
                        break;
                    case "Immortal":

                        model.Immortal = true;
                        break;
                    case "Balloon":
                        model.Balloon = true;
                        break;
                    case "Trophy":

                        model.Trophy = true;
                        break;
                    case "Detector":
                        model.Detector = true;
                        break;
                    default:
                        break;
                }
                callback();
            }
        }
        public void ItemReceived(GameObject target)
        {
            if (model.IsGetStartItem == false)
                target.SetActive(false);
        }
        public void CancelItem()
        {
            model.IsGetStartItem = false;
            model.IceSkate = false;
            model.SlimeShoe = false;
            model.Shield = false;
            model.EnergyDrink = false;
            model.Balloon = false;
            model.Armor = false;
            model.LightnessShoe = false;
            model.Crucifixion = false;
            model.RocketShoe = false;
            model.DeveloperObsession = false;
            model.Immortal = false;
            model.Trophy = false;
            model.Detector = false;
            model.Shielding = false;
            if (model.LightnessShoe)
            {
                model.AddItemSpeedGain = -0.1f;
                model.AddItemJumpGain = -0.1f;
            }
            if (model.Armor)
            {
                model.AddItemSpeedGain = 0.1f;
                model.AddItemJumpGain = 0.1f;
                model.MaxHealth += 2;
            }
        }
        public void SizeAdjust(float size)
        {
            model.CharacterSize *= size;
            //model.CharacterSize = size;

        }
        public void DODash(Vector2 value)
        {
            Debug.Log(value);
            mover.DOAddforceImpulse(value * model.DashPower);
            Debug.Log(model.DashPower);
        }

        private void FixedUpdate()
        {
            if (OnDisable) return;
            mover.FixedUpdate();
        }
        private void Update()
        {
            mover.Update();
            //transform.localScale = new Vector3(model.CharacterSize, model.CharacterSize, 1);
        }
    }
}