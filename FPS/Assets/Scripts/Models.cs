using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public static class Models 
{
    #region - Player -

    public enum  PlayerStance
    {
        Stand,
        Crouch
    }
    
    
    [Serializable]
    public class PlayerSettingsModel
    {
        [Header("View Settings")]
        public float ViewXSensitivity;
        public float ViewYSensitivity;
        
        public bool ViewXInverted;
        public bool ViewYInverted;

        [Header("Movement - Walking")] 
        public float WalkingForwardSpeed;
        public float WalkingBackwardSpeed;
        public float WalkingStrafeSpeed;

        [Header("Movement - Running")] 
        public float RunningForwardSpeed;
        public float RunningStrafeSpeed;
        
        

        [Header("Jumping")] 
        public float jumpingHeight;
        public float jumpingFalloff;

    }

    [Serializable]
    public class CharacterStance
    {
        public float CameraHeight;
        public CapsuleCollider StanceCollider;
    }

    #endregion
    
}
