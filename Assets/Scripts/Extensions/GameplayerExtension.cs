using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mirror.EscapeGame
{
    public static class GameplayerExtension
    {
        public static void DoMove(this Rigidbody2D rb, Vector2 force)
        {
            rb.velocity = force;
        }
        public static void DoAddMove(this Rigidbody2D rb, Vector2 force)
        {
            rb.velocity += force;
        }
        public static void DoMoveX(this Rigidbody2D rb, float force)
        {
            rb.velocity = new Vector2(force, rb.velocity.y);
        }
        public static void DoMoveY(this Rigidbody2D rb, float force)
        {
            rb.velocity = new Vector2(rb.velocity.x, force);
        }
        public static void DoControlMove(this Rigidbody2D rb, float force)
        {
            rb.DoAddforce(new Vector2(force, rb.velocity.y));
        }
        public static void DoAddforce(this Rigidbody2D rb, Vector2 force)
        {
            rb.AddForce(force);
        }
        public static void DoAddforceImpulse(this Rigidbody2D rb, Vector2 force)
        {
            rb.AddForce(force, ForceMode2D.Impulse);
        }
        public static void DoAddforceX(this Rigidbody2D rb, float force)
        {
            rb.AddForce(new Vector2(force, 0));
        }
        public static void DoAddforceY(this Rigidbody2D rb, float force)
        {
            rb.AddForce(new Vector2(0, force));
        }
        public static void DoStopMoveX(this Rigidbody2D rb)
        {
            rb.DoMoveX(0);
        }
        public static void DoSlowDown(this Rigidbody2D rb)
        {
            rb.DoAddMove(rb.velocity * -0.05f);
        }
        public static void DlIceSlide(this Rigidbody2D rb, float posNForce)
        {
            rb.DoAddforceImpulse(Vector2.right * posNForce);
        }
        public static void DoSlide(this Rigidbody2D rb)
        {
            rb.DoAddMove(rb.velocity * -0.1f);
        }
    }

}