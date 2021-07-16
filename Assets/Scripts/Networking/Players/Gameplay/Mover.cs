using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mirror.EscapeGame.Gameplayer
{
    public class Mover : IGameplayControl
    {
        public float SetupInput
        {
            set
            {
                xInput = value;
                animator.DoAnimation("movement", Mathf.Abs(xInput));
            }
        }
        [Header("Float")]
        float xInput = 0;
        float endurance = 0;
        float jumpTimeCounter = 0;
        float wallJumpTimeCounter = 0;
        float wallJumpPos = 0;
        [Header("Bool")]
        bool isRunning = false;
        bool isAbleToRecoveryEndurance = false;
        bool isSquating = false;
        bool isWallJumping = false;
        bool isInertancing = false;

        public bool OnControl
        {
            get
            {
                if (rb.velocity.magnitude > 20 || isInertancing)
                    return true;
                return false;
            }
        }
        public bool OnGround
        {
            get
            {
                return model.Grounded || model.IceGrounded || model.SlimeGrounded;
            }
        }
        public bool OnWall
        {
            get
            {
                if (OnGround)
                    return false;
                else
                    return model.Fronted || model.IceFronted || model.SlimeFronted;
            }
        }

        public void Update()
        {
            EnduranceSystem();
            GroundControl();
            GroundControl();
            FrontControl();
        }
        public void FixedUpdate()
        {
            DoMove();
            DoJump();
        }
        public void Inertance(Vector2 force)
        {
            isInertancing = true;
            model.CurrentGroundState = GroundState.Controled;
            rb.AddForce(force, ForceMode2D.Impulse);
            AbleToDo(0.2f, () => model.CurrentGroundState = GroundState.Normal);
            AbleToDo(0.2f, () => isInertancing = false);
        }

        public void DoMove()
        {
            switch (model.CurrentGroundState)
            {
                case GroundState.Controled:
                    rb.DoControlMove(xInput * 7 + rb.velocity.x * -1.5f);
                    break;
                case GroundState.Air:
                case GroundState.Normal:
                case GroundState.Slime:
                    if (xInput != 0)
                    {
                        MoveGainHandle();
                    }
                    else rb.DoStopMoveX();
                    break;
                case GroundState.Ice:

                    if (xInput != 0)
                    {
                        IceMoveGainHandle();
                        rb.DoSlowDown();
                    }
                    else rb.DoSlowDown();
                    break;
                default:
                    break;
            }
        }
        public void Run(bool value)
        {
            switch (value)
            {
                case true:
                    isRunning = true;
                    isAbleToRecoveryEndurance = false;
                    model.AddSpeedGain = 0.2f;
                    model.AddJumpGain = 0.1f;
                    break;
                case false:
                    isRunning = false;
                    AbleToDo(1f, () => isAbleToRecoveryEndurance = true);
                    model.SpeedGain = 1f;
                    model.JumpGain = 1f;
                    break;
            }
        }
        public void EnduranceSystem()
        {
            if (endurance <= 0)
                Run(false);
            if (isRunning)
                endurance -= Time.deltaTime;
            if (isRunning == false && isAbleToRecoveryEndurance && endurance < model.Endurance)
            {
                if (model.EnergyDrink) endurance += Time.deltaTime;
                endurance += Time.deltaTime;
            }
        }
        public void MoveGainHandle()
        {
            if (isSquating && rb.velocity.x == 0) model.SlideSpeedGain = 0;
            else if (isSquating && model.SlideSpeedGain > 0) model.SlideSpeedGain -= 0.015f;
            else if (isSquating && model.SlideSpeedGain <= 0) model.SlideSpeedGain = 0;
            else model.SlideSpeedGain = 1f;
            FrontControl();
            if (OnWall)
            {
                rb.DoMoveX(0);
            }
            else
            {
                rb.DoMoveX(xInput
                    * model.WalkSpeed
                    * model.ItemSpeedGain
                    * model.GroundSpeedGain
                    * model.SpeedGain
                    * model.StateSpeedGain
                    * model.SlideSpeedGain);
            }

        }
        public void IceMoveGainHandle()
        {
            FrontControl();
            if (rb.velocity.x < 15)
            {
                if (OnWall)
                {
                    rb.DoAddforceX(0);
                }
                else
                {
                    rb.DoAddforceX(xInput * 3
                        * model.WalkSpeed
                        * model.ItemSpeedGain
                        * model.GroundSpeedGain
                        * model.SpeedGain
                        * model.StateSpeedGain
                        * model.SlideSpeedGain);
                }
            }
        }
        public void Jump(bool isJumping)
        {
            if (isJumping)
            {
                FrontControl();
                GroundControl();
                if (OnGround)
                {
                    model.CurrentJumpState = JumpState.PreJumping;
                }
                else if (OnWall)
                {
                    isWallJumping = true;
                    model.CurrentJumpState = JumpState.PreWallJumping;
                }
                else if (!OnGround && !OnWall && model.RocketShoe && !model.RocketJump)
                {
                    model.RocketJump = true;
                    jumpTimeCounter = 0f;
                    model.GroundJumpGain = 0.5f;
                    model.CurrentJumpState = JumpState.PreJumping;
                }
                animator.DoAnimation("jump");
            }
            else
            {
                if (model.CurrentJumpState == JumpState.IsJumping)
                {
                    model.CurrentJumpState = JumpState.PreFalling;
                }
            }
        }
        public void DoJump()
        {
            switch (model.CurrentJumpState)
            {
                case JumpState.PreWallSliding:
                    model.CurrentJumpState = JumpState.IsWallSliding;
                    break;
                case JumpState.PreWallJumping:
                    model.CurrentJumpState = JumpState.IsWallJumping;
                    break;
                case JumpState.PreJumping:
                    model.CurrentJumpState = JumpState.IsJumping;
                    break;
                case JumpState.PreFalling:
                    model.CurrentJumpState = JumpState.IsFalling;
                    break;
                case JumpState.PreGrounded:
                    animator.DoAnimation("exit");
                    model.CurrentJumpState = JumpState.IsGrounded;
                    break;
            }
            switch (model.CurrentJumpState)
            {
                case JumpState.IsWallSliding:
                    model.RocketJump = false;
                    //if (wallJumpTimeCounter > 0) wallJumpTimeCounter = 0;
                    rb.DoMove(Vector2.down
                        * model.ItemJumpGain
                        * (2 - model.GroundJumpGain)
                        * model.JumpGain
                        * model.WallSlideGain);
                    if (OnWall == false || OnGround) model.CurrentJumpState = JumpState.PreFalling;
                    break;
                case JumpState.IsWallJumping:
                    if (wallJumpTimeCounter < model.WallJumpTime && wallJumpPos * xInput < 0)
                    {
                        wallJumpTimeCounter += Time.deltaTime;
                        rb.DoMove(new Vector2(
                            model.WallJumpForce.x * wallJumpPos
                            * model.ItemJumpGain
                            * model.JumpGain
                            * model.StateJumpGain
                            * model.WallJumpGain
                            * (2.2f - wallJumpTimeCounter * 10)
                        , model.WallJumpForce.y
                            * model.ItemJumpGain
                            * model.JumpGain
                            * model.StateJumpGain
                            * model.WallJumpGain));
                    }
                    else if (wallJumpPos * xInput > 0)
                    {

                        isWallJumping = false;
                        model.CurrentJumpState = JumpState.IsJumping;
                    }
                    else
                    {
                        isWallJumping = false;
                        model.CurrentJumpState = JumpState.PreFalling;
                    }
                    break;
                case JumpState.IsJumping:
                    if (jumpTimeCounter < model.JumpTime)
                    {
                        jumpTimeCounter += Time.deltaTime;
                        rb.DoMoveY(Vector2.up.y * model.JumpForce
                        * model.ItemJumpGain
                        * model.GroundJumpGain
                        * model.JumpGain
                        * model.StateJumpGain);
                    }
                    else model.CurrentJumpState = JumpState.PreFalling;
                    break;
                case JumpState.IsFalling:
                    if (OnGround) model.CurrentJumpState = JumpState.PreGrounded;
                    break;
                case JumpState.IsGrounded:
                    jumpTimeCounter = 0;
                    model.RocketJump = false;
                    break;
            }
        }
        public void GroundControl()
        {
            // if (GroundState == GroundState.controled) return;
            model.Grounded = GroundCheck(model.GetGroundLayer) || GroundCheck(model.GetBoxLayer);
            model.IceGrounded = GroundCheck(model.GetIceGroundLayer);
            model.SlimeGrounded = GroundCheck(model.GetSlimeGroundLayer);

            if (OnControl)
            {
                model.CurrentGroundState = GroundState.Controled;
            }
            else if (model.Grounded)
            {
                model.GroundSpeedGain = 1;
                model.GroundJumpGain = 1f;
                model.CurrentGroundState = GroundState.Normal;
            }
            else if (model.IceGrounded)
            {
                model.GroundSpeedGain = 1;
                model.GroundJumpGain = 1;
                if (model.IceSkate == false) model.CurrentGroundState = GroundState.Ice;
                else model.CurrentGroundState = GroundState.Normal;
            }
            else if (model.SlimeGrounded)
            {
                if (model.SlimeShoe == false)
                {
                    model.GroundSpeedGain = 0.5f;
                    model.GroundJumpGain = 0.2f;
                    model.CurrentGroundState = GroundState.Slime;
                }
                else model.CurrentGroundState = GroundState.Normal;
            }
            else
            {
                model.CurrentGroundState = GroundState.Air;
            }
        }
        public void FrontControl()
        {
            if (OnWall) if (wallJumpTimeCounter > 0) wallJumpTimeCounter = 0;
            if (OnGround)
            {
                return;
            }
            // if (FrontState == FrontState.controled) return;

            model.Fronted = FrontCheck(model.GetGroundLayer);
            model.IceFronted = FrontCheck(model.GetIceGroundLayer);
            model.SlimeFronted = FrontCheck(model.GetSlimeGroundLayer);

            if (model.Fronted)
            {
                model.WallSpeedGain = 1f;
                model.WallJumpGain = 1f;
                model.WallSlideGain = 1f;
            }
            else if (model.IceFronted && OnWall)
            {
                if (model.IceSkate == false)
                {
                    model.WallSpeedGain = 1.3f;
                    model.WallJumpGain = 0f;
                    model.WallSlideGain = 10f;
                }
                else
                {
                    model.WallSpeedGain = 1f;
                    model.WallJumpGain = 1f;
                    model.WallSlideGain = 1f;
                }
            }
            else if (model.SlimeFronted && OnWall)
            {
                if (model.SlimeShoe == false)
                {
                    model.WallSpeedGain = 0.7f;
                    model.WallJumpGain = 0.7f;
                    model.WallSlideGain = 1f;
                }
                else
                {
                    model.WallSpeedGain = 1f;
                    model.WallJumpGain = 1f;
                    model.WallSlideGain = 1f;
                }
            }

            if (OnGround == false && isWallJumping == false && model.CurrentJumpState != JumpState.IsJumping && model.CurrentJumpState != JumpState.PreJumping) model.CurrentJumpState = JumpState.PreFalling;
            if (isWallJumping == false && OnWall && rb.velocity.y < 0 && model.CurrentJumpState != JumpState.PreWallJumping) model.CurrentJumpState = JumpState.PreWallSliding;
        }
        public bool GroundCheck(LayerMask mask)
        {
            bool detect = false;
            foreach (var ground in model.GetGroundCheck)
            {
                if (Physics2D.Raycast(
                    ground.position,
                    Vector2.down,
                    model.GroundCheckDistance,
                    mask
                ))
                {
                    detect = true;
                    break;
                }
            };
            return detect ? true : false;
        }
        public bool FrontCheck(LayerMask mask)
        {
            bool detect = false;
            foreach (var front in model.GetFrontCheck)
            {
                if (Physics2D.Raycast(
                    front.position,
                    Vector2.right * xInput,
                    model.GroundCheckDistance,
                    mask
                ))
                {
                    detect = true;
                    break;
                }
            };
            if (detect) wallJumpPos = -xInput;
            return detect ? true : false;
        }
        public void DOAddforceImpulse(Vector2 power)
        {
            rb.DoAddforceImpulse(power);
        }

    }
}