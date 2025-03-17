using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Core
{
    public class PlayerController : MonoBehaviour
    {
        [FormerlySerializedAs("MoveAction")] public InputAction moveAction;
        [FormerlySerializedAs("JumpAction")] public InputAction jumpAction;
        public float speed;
        public float jumpForce;
        public float dashSpeed;
        public float dashDuration;
        private Rigidbody2D _rb;
        private bool _isGrounded;
        private int _jumpCount;
        private const int MaxJumps = 1;
        private bool _isDashing;
        private float _originalGravityScale;
        [UsedImplicitly] private float _dashTime;
        private bool _dashedSinceGrounded;
        [UsedImplicitly] private Vector2 _lastMoveDirection;

        public Transform cameraTransform;
        public float cameraSmoothSpeed = 5f;
        public float attackSpeed;
        private float _attackCooldownTimer;
        private bool _isAttacking;
        public float attackRange = 1.5f;
        public float attackDamage = 10f;
        private float _lastDashTime;
        private const float DoubleTapTime = 0.2f;
        private KeyCode _lastKeyPressed;

        // Dash cooldown variables
        public float dashCooldown = 1f; // Cooldown time in seconds
        private float _dashCooldownTimer;

        //Animation
        private Animator _animator;
        private enum PlayerState { Attack, Block, Die, Idle, Run, Walk }
        private PlayerState _currentState = PlayerState.Idle;

        private bool _facingRight = true;
        private BoxCollider2D _boxCollider;
        public Camera mainCamera;
        public Camera menuCamera;
        private PlayerHealth _playerHealth;

        private bool _isBlocking;
        public float blockDuration = 1.0f;
        public float blockCooldown = 1.5f;
        private float _blockCooldownTimer;

        private void Start()
        {
            moveAction.Enable();
            jumpAction.Enable();
            _rb = GetComponent<Rigidbody2D>();
            _rb.freezeRotation = true;
            _animator = GetComponent<Animator>();
            _originalGravityScale = _rb.gravityScale;
            _boxCollider = GetComponent<BoxCollider2D>();
            _playerHealth = GetComponent<PlayerHealth>();
        }

        private void Update()
        {
            Vector2 move = moveAction.ReadValue<Vector2>();
            if (move != Vector2.zero)
            {
                _lastMoveDirection = move.normalized;
            }

            if (!_isDashing)
            {
                _rb.linearVelocity = new Vector2(move.x * speed, _rb.linearVelocityY);
                if (_rb.linearVelocityX == 0 && !_isAttacking) ChangeState(PlayerState.Idle); else if (!_isAttacking) ChangeState(PlayerState.Walk);
            }

            if (jumpAction.WasPressedThisFrame() && _jumpCount < MaxJumps)
            {
                _rb.linearVelocity = new Vector2(_rb.linearVelocityX, jumpForce);
                _jumpCount++;
            }
            else if (_isGrounded && !IsTouchingWall())
            {
                _jumpCount = 0;
                _dashedSinceGrounded = false;
            }

            HandleDashInput();
            FollowPlayer();
            HandleBlocking();
            Attack();
            GoToMainMenu();

            // Update the dash cooldown timer

            _isGrounded = IsGrounded();
            if (_dashCooldownTimer > 0) _dashCooldownTimer -= Time.deltaTime;
            if (_attackCooldownTimer > 0) _attackCooldownTimer -= Time.deltaTime;
            if (_blockCooldownTimer > 0) _blockCooldownTimer -= Time.deltaTime;
        }

        private void FixedUpdate()
        {
            switch (_rb.linearVelocityX)
            {
                case > 0 when !_facingRight:
                case < 0 when _facingRight:
                    Flip();
                    break;
            }
        }
        private bool IsGrounded()
        {
            return _boxCollider.IsTouchingLayers(LayerMask.GetMask("Ground"));
        }

        private void GoToMainMenu()
        {
            if (!Input.GetKeyDown(KeyCode.Escape)) return;
            menuCamera.enabled = true;
            mainCamera.enabled = false;
        }

        private void Attack()
        {
            if (!(_attackCooldownTimer <= 0) || !Input.GetKeyDown(KeyCode.X)) return;
            _isAttacking = true;
            ChangeState(PlayerState.Attack);
            StartCoroutine(StopAttackAfterDelay(0.5f));
            PeformAttack();
        }

        private void PeformAttack()
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, attackRange, LayerMask.GetMask("Enemy"));
            List<Collider2D> hitEnemies = colliders.Where(collider1 => collider1 != null).ToList();

            foreach (Collider2D enemy in hitEnemies)
            {
                if (enemy.TryGetComponent(out IEnemy enemyComponent))
                {
                    enemyComponent.TakeDamage(attackDamage);
                }
            }
        }

        private IEnumerator StopAttackAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            _isAttacking = false;
            _attackCooldownTimer = attackSpeed;
            ChangeState(PlayerState.Idle);
        }


        private void HandleDashInput()
        {
            if (!(_dashCooldownTimer <= 0) || (!Input.GetKeyDown(KeyCode.A) && !Input.GetKeyDown(KeyCode.D)) ||
                _dashedSinceGrounded) return;
            KeyCode currentKey = Input.GetKeyDown(KeyCode.A) ? KeyCode.A : KeyCode.D;

            if (_lastKeyPressed == currentKey && Time.time - _lastDashTime < DoubleTapTime)
            {
                StartCoroutine(Dash(currentKey == KeyCode.A ? Vector2.left : Vector2.right));
                _dashCooldownTimer = dashCooldown; // Reset the cooldown timer
            }
            _lastDashTime = Time.time;
            _lastKeyPressed = currentKey;
        }

        private void HandleBlocking()
        {
            if (_blockCooldownTimer <= 0 && Input.GetKeyDown(KeyCode.C) && !_isBlocking) StartCoroutine(Block());
        }

        private IEnumerator Block()
        {
            _isBlocking = true;
            ChangeState(PlayerState.Block);
            yield return new WaitForSeconds(blockDuration);
            _isBlocking = false;
            _blockCooldownTimer = blockCooldown;
        }

        private IEnumerator Dash(Vector2 direction)
        {
            _isDashing = true;
            _dashTime = dashDuration;
            _rb.linearVelocity = direction * dashSpeed;
            _rb.gravityScale = 0;

            ChangeState(PlayerState.Run);
            yield return new WaitForSeconds(dashDuration);

            _rb.linearVelocity = Vector2.zero;
            _isDashing = false;
            _rb.gravityScale = _originalGravityScale;
            _dashedSinceGrounded = true;
        }

        private void FollowPlayer()
        {
            if (!cameraTransform) return;
            // Keep the z position of the camera fixed by only updating the x and y axes
            Vector3 targetPosition = new Vector3(transform.position.x, transform.position.y, cameraTransform.position.z);
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPosition, cameraSmoothSpeed * Time.deltaTime);
        }
        private void Die()
        {
            if (_playerHealth == null || _playerHealth.currentHealth > 0) return;
            ChangeState(PlayerState.Die);
            StartCoroutine(DeathCoroutine(1f));
        }
        private static IEnumerator DeathCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            Application.Quit();
        }

        private bool IsTouchingWall()
        {
            return Physics2D.Raycast(transform.position, Vector2.right, 0.7f, LayerMask.GetMask("Ground")) ||
                   Physics2D.Raycast(transform.position, Vector2.left, 0.7f, LayerMask.GetMask("Ground"));
        }

        private void ChangeState(PlayerState newState)
        {
            if (_currentState == newState) return;
            _currentState = newState;
            _animator.Play(newState.ToString());
        }

        private void Flip()
        {
            _facingRight = !_facingRight;
            Vector3 theScale = transform.localScale;
            theScale.x *= -1;
            transform.localScale = theScale;
        }
    }
}