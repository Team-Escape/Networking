using System;
using UnityEngine;

namespace Mirror.EscapeGame.GameplayerSpace
{
    public class HealthPotion : GameItemControl
    {
        public override void Use()
        {
            if (model.health >= model.maxHealth)
                return;
            model.health++;
        }
    }
}