using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mirror.EscapeGame.GameplayerSpace
{
    public interface ISlot
    {
        GameItemControl GameItemControl { get; set; }
        void SetGameItem(int id, Model playerModel);
        void Use();
    }

}