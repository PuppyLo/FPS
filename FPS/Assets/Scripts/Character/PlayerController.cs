using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using static Models;

public class PlayerController : MonoBehaviour
{ 
    private CharacterController _characterController; 
    private DefaultInput _defaultInput;
    private Vector2 _inputMovement;
    private Vector2 _inputView;
    
    private Vector3 _newCameraRotation;
    private Vector3 _newPlayerRotation;
    
    [Header("References")]
    public Transform cameraHolder;
    public Transform feetTransform;
   
   [Header("Settings")] 
   public PlayerSettingsModel playerSettings;
   public float viewClampYMin = -70;
   public float viewClampYMax = 80;
   public LayerMask playerMask;
   
   [Header("Gravity")] 
   public float gravityAmount;
   public float gravityMin;
   private float _playerGravity;

   public Vector3 jumpingForce;
   private Vector3 _jumpingForceVelocity;

   [Header("Stance")] 
   public PlayerStance playerStance;
   public float playerStanceSmoothing;
   public CharacterStance playerStandStance;
   public CharacterStance playerCrouchStance;
   public float stanceCheckErrorMargin = 0.05f;
   
   private float _cameraHeight;
   private float _cameraHeightVelocity;
   
   private Vector3 _stanceCapsuleCenterVelocity;
   private float _stanceCapsuleHeightVelocity;


   private bool _isSprinting;
   private void Awake()
   {
       _defaultInput = new DefaultInput();
       
       _defaultInput.Character.Movement.performed += e => _inputMovement = e.ReadValue<Vector2>();
       _defaultInput.Character.View.performed += e => _inputView = e.ReadValue<Vector2>();
       _defaultInput.Character.Jump.performed += e => Jump();
       
       _defaultInput.Character.Crouch.performed += e => Crouch();
       
       _defaultInput.Character.Sprint.performed += e => ToggleSprint();

       
       _defaultInput.Enable();

       _newCameraRotation = cameraHolder.localRotation.eulerAngles;
       _newPlayerRotation = transform.localRotation.eulerAngles;

       _characterController = GetComponent<CharacterController>();

       _cameraHeight = cameraHolder.localPosition.y;
   }

   private void Update()
   {
       CalculateView();
       CalculateMovement();
       CalculateJump();
       CalculateStance();
   }

   private void CalculateView()
   {
       _newPlayerRotation.y += playerSettings.ViewXSensitivity * (playerSettings.ViewXInverted ? -_inputView.x : _inputView.x) * Time.deltaTime;
       transform.localRotation = Quaternion.Euler(_newPlayerRotation);
       
       _newCameraRotation.x += playerSettings.ViewYSensitivity * (playerSettings.ViewYInverted ? _inputView.y : -_inputView.y) * Time.deltaTime;
       _newCameraRotation.x = Mathf.Clamp(_newCameraRotation.x, viewClampYMin, viewClampYMax);
       
       cameraHolder.localRotation = Quaternion.Euler(_newCameraRotation);
       
   }

   private void CalculateMovement()
   {
       if (_inputMovement.y <= 0.2f)
       {
           _isSprinting = false;
       }
       
       
       
       
       var verticalSpeed = playerSettings.WalkingForwardSpeed;
       var horizontalSpeed = playerSettings.WalkingStrafeSpeed;

       if (_isSprinting)
       {
           verticalSpeed = playerSettings.RunningForwardSpeed;
           horizontalSpeed = playerSettings.RunningStrafeSpeed;
       }
       
       
       
       

       var newMovementSpeed = new Vector3(horizontalSpeed * _inputMovement.x * Time.deltaTime, 0, verticalSpeed * _inputMovement.y * Time.deltaTime);
       newMovementSpeed = transform.TransformDirection(newMovementSpeed);

       if (_playerGravity >gravityMin) 
       {
           _playerGravity -= gravityAmount * Time.deltaTime;
       }
       
       if (_playerGravity < -0.1f && _characterController.isGrounded)
       {
           _playerGravity = -0.1f;
       }

       newMovementSpeed.y += _playerGravity;
       newMovementSpeed += jumpingForce * Time.deltaTime;

       _characterController.Move(newMovementSpeed);
   }

   private void CalculateJump()
   {
       jumpingForce = Vector3.SmoothDamp(jumpingForce, Vector3.zero, ref _jumpingForceVelocity, playerSettings.jumpingFalloff);
   }

   private void CalculateStance()
   {

      var currentStance = playerStandStance;

       if (playerStance == PlayerStance.Crouch)
       {
           currentStance = playerCrouchStance;
       }
       
       
       _cameraHeight = Mathf.SmoothDamp(cameraHolder.localPosition.y,currentStance.CameraHeight, ref _cameraHeightVelocity, playerStanceSmoothing);
       cameraHolder.localPosition = new Vector3(cameraHolder.localPosition.x, _cameraHeight, cameraHolder.localPosition.z);


       _characterController.height = Mathf.SmoothDamp(_characterController.height, currentStance.StanceCollider.height, ref _stanceCapsuleHeightVelocity,playerStanceSmoothing);
       _characterController.center = Vector3.SmoothDamp(_characterController.center, currentStance.StanceCollider.center, ref _stanceCapsuleCenterVelocity, playerStanceSmoothing);
   }
   
   private void Jump()
   {
       if (!_characterController.isGrounded)
       {
           return;
       }
       
       if (playerStance == PlayerStance.Crouch)
       {
           playerStance = PlayerStance.Stand;
           return;
       }
       
       //Jump
       jumpingForce = Vector3.up * playerSettings.jumpingHeight;
       _playerGravity = 0;
   }

   private void Crouch()
   {
       if (playerStance == PlayerStance.Crouch)
       {
           if (StanceCheck(playerStandStance.StanceCollider.height))
           {
               return;
           }
           
           playerStance = PlayerStance.Stand;
           return;
       }

       if (StanceCheck(playerCrouchStance.StanceCollider.height))
       {
           return;
       }
        
       playerStance = PlayerStance.Crouch;
   }

   private bool StanceCheck(float stanceCheckHeight)
   {
       var start = new Vector3(feetTransform.position.x,feetTransform.position.y + _characterController.radius + stanceCheckErrorMargin,feetTransform.position.z);
       var end = new Vector3(feetTransform.position.x,feetTransform.position.y - _characterController.radius - stanceCheckErrorMargin + stanceCheckHeight,feetTransform.position.z);
       
       return Physics.CheckCapsule(start,end, _characterController.radius, playerMask);
   }

   private void ToggleSprint()
   {
       if (_inputMovement.y <= 0.2f)
       {
           _isSprinting = false;
           return;
       }
       
       _isSprinting = !_isSprinting;
   }
    
}
