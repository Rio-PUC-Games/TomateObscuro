﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerPhysics))]
public class PlayerController : MonoBehaviour
{
    #region Public

    public SpriteRenderer ShadowSpriteRenderer;
    public PauseScript PauseScript;

    public float Gravity = 10;
    public float Speed = 10;
    public float Acceleration = 10;
    public float JumpHeight = 10;
    public int NumJumps = 2;

    public float HookLaunchSpeed = 100.0f;
    public float HookPullSpeed = 100.0f;
    public float HookPullFinishDistance = 2.0f;

    public float HookMaxDistance = 20.0f;
    public LayerMask HookCollisionLayerMask;
    public LayerMask BreakCollisionLayerMask;
    public LayerMask EnemyCollisionLayerMask;

    public GameObject HookPrefab;

    public ParticleSystem[] SmokeParticles;

    [HideInInspector] public int CurrentAvailableJumps;
    #endregion

    #region Private
    private float _currentSpeed;
    private float _targetSpeed;
    private Vector3 _amountToMove;
    private bool _wasGroundedLastUpdate;
    private bool _isHookOnLaunchPhase = false;
    private bool _isHookOnPullPhase = false;
    private GameObject _hookInstantiated;
    private CircleCollider2D _hookCollider;
    private SpriteRenderer mySpriteRenderer;

    private PlayerPhysics _playerPhysics;
    #endregion

    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(this.transform.position, this.HookPullFinishDistance);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(this.transform.position, this.HookMaxDistance);
    }

    private void Start()
    {
        _playerPhysics = GetComponent<PlayerPhysics>();
        this.CurrentAvailableJumps = this.NumJumps;
        mySpriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void updateCurrentSpeed()
    {
        if (_playerPhysics.IsBlockedHorizontally)
        {
            _targetSpeed = 0;
            _currentSpeed = 0;

        }

        _targetSpeed = KeyBindings.Instance.GetAxisX() * Speed;
        _currentSpeed = IncrementTowards(_currentSpeed, _targetSpeed, Acceleration);
        _amountToMove.x = _currentSpeed;

        //checa se player está tentando se mover e toca som baseado nisso
        if (_targetSpeed > float.Epsilon || _targetSpeed < -float.Epsilon)
        {
            if (!LevelManager.Instance.PlayerWalk.isPlaying)
            {
                LevelManager.Instance.PlayerWalk.PlayDelayed(LevelManager.Instance.PlayerWalkDelay);
            }
        }
        else
        {
            LevelManager.Instance.PlayerWalk.Stop();
        }
    }

    private void updateWithGravity()
    {
        if (_playerPhysics.IsBlockedVertically)
        {
            _amountToMove.y = 0.0f;
        }

        if (_playerPhysics.IsGrounded)
        {
            _amountToMove.y = Mathf.Max(0.0f, _amountToMove.y);

        }
        else
        {
            _amountToMove.y -= Gravity * Time.deltaTime;

        }
    }

    private void updateWithJump()
    {
        if (Input.GetKeyDown(KeyBindings.Instance.PlayerJump))  // Jump
        {
            if (this.CurrentAvailableJumps > 0)
            {
                _playerPhysics.EndJump = false;

                LevelManager.Instance.PlayerJump.PlayDelayed(LevelManager.Instance.PlayerJumpDelay); //toca som de pulo

                this.CurrentAvailableJumps -= 1;
                _amountToMove.y = JumpHeight;
                this.resetHook();
            }
        }

        if (_playerPhysics.IsGrounded == true && this._wasGroundedLastUpdate == false)
        {
            this.CurrentAvailableJumps = this.NumJumps;
        }

        this._wasGroundedLastUpdate = _playerPhysics.IsGrounded;
    }

    private void resetHook()
    {
        this._hookCollider = null;
        if (this._hookInstantiated != null)
        {
            Destroy(this._hookInstantiated);
        }
        this._hookInstantiated = null;
        this._isHookOnLaunchPhase = false;
        this._isHookOnPullPhase = false;
    }

    private void updateWithHook()
    {
        if (Input.GetKeyDown(KeyBindings.Instance.PlayerHook) && PauseScript.GameIsPause == false)
        {
            if (!_isHookOnPullPhase && !_isHookOnLaunchPhase)
            {
                this._isHookOnLaunchPhase = true;
                Vector3 mouseClick3dRelativeToPlayer = Camera.main.ScreenToWorldPoint(Input.mousePosition) - this.transform.position;
                Vector2 mouseClick2dRelativeToPlayer = new Vector2(mouseClick3dRelativeToPlayer.x, mouseClick3dRelativeToPlayer.y);
                Vector2 xAxis = new Vector2(1.0f, 0.0f);
                float rotationAngle = Vector2.SignedAngle(xAxis, mouseClick2dRelativeToPlayer);
                Quaternion hookRotation = new Quaternion();
                hookRotation.eulerAngles = new Vector3(0.0f, 0.0f, rotationAngle);

                this._hookInstantiated = Instantiate(this.HookPrefab, this.transform.position, hookRotation);
                this._hookCollider = this._hookInstantiated.GetComponent<CircleCollider2D>();

                // hookGo = GameObject.Find("Hook Go").GetComponent<AudioSource>();
                LevelManager.Instance.HookGo.PlayDelayed(LevelManager.Instance.HookGoDelay);
            }
        }
        if (_isHookOnLaunchPhase)
        {
            Vector3 localTranslation = new Vector3(1.0f, 0.0f, 0.0f) * this.HookLaunchSpeed * Time.deltaTime;
            this._hookInstantiated.transform.localPosition += this._hookInstantiated.transform.TransformVector(localTranslation);
            if ((this._hookInstantiated.transform.position - this.transform.position).magnitude > HookMaxDistance)
            {
                this.resetHook();
            }
            else if (this._hookCollider.IsTouchingLayers(this.BreakCollisionLayerMask))
            {
                this.resetHook();
            }
            else if (this._hookCollider.IsTouchingLayers(this.HookCollisionLayerMask))
            {
                LevelManager.Instance.HookHitLevel.PlayDelayed(LevelManager.Instance.HookHitLevelDelay); //toca som de quando a hook atinge algo
                this._isHookOnLaunchPhase = false;
                this._isHookOnPullPhase = true;
                LevelManager.Instance.HookReturns.PlayDelayed(LevelManager.Instance.HookReturnsDelay);
                this.CurrentAvailableJumps = this.NumJumps;
            }
            else if (this._hookCollider.IsTouchingLayers(this.EnemyCollisionLayerMask))
            {
                LevelManager.Instance.HookHitEnemy.PlayDelayed(LevelManager.Instance.HookHitEnemyDelay); //toca som de quando a hook atinge inimigo
                this._isHookOnLaunchPhase = false;
                this._isHookOnPullPhase = true;
                this.CurrentAvailableJumps = this.NumJumps;
            }
        }
        if (_isHookOnPullPhase)
        {
            Vector2 deltaPosition = this._hookInstantiated.transform.position - this.transform.position;
            this._amountToMove = new Vector2(0.0f, 0.0f);
            if (deltaPosition.magnitude <= this.HookPullFinishDistance)
            {
                this.resetHook();
            }
            else
            {
                this._amountToMove = deltaPosition.normalized * this.HookPullSpeed;
            }
        }
    }

    private void Update()
    {
        this.updateCurrentSpeed();
        this.updateWithGravity();
        this.updateWithJump();
        this.updateWithHook();

        if (_amountToMove.x > 0)
        {
            mySpriteRenderer.flipX = true;
            ShadowSpriteRenderer.flipX = true;

            foreach (ParticleSystem item in SmokeParticles)
            {
                ParticleSystem ps = item;
                var fo = ps.forceOverLifetime;
                fo.xMultiplier = -5;
            }
        }
        else if (_amountToMove.x < 0)
        {
            mySpriteRenderer.flipX = false;
            ShadowSpriteRenderer.flipX = false;

            foreach (ParticleSystem item in SmokeParticles)
            {
                ParticleSystem ps = item;
                var fo = ps.forceOverLifetime;
                fo.xMultiplier = 5;
            }
        }

        _playerPhysics.Move(_amountToMove * Time.deltaTime);
    }

    // Increase n towards target by a
    private float IncrementTowards(float n, float target, float a)  // n = current speed; a = acceleration
    {
        if (n == target) return n;
        else
        {
            float dir = Mathf.Sign(target - n);
            n += a * Time.deltaTime * dir;
            return (dir == Mathf.Sign(target - n)) ? n : target;
        }
    }

}