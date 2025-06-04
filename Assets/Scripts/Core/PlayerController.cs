// Importerer nødvendige namespaces for Unity og andre funktioner.
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

namespace Core
{
    // PlayerController-klassen styrer spillerens bevægelser, angreb, blokering, dash og andre interaktioner.
    public class PlayerController : MonoBehaviour
    {
        // InputAction-variabler til at håndtere spillerens input.
        [FormerlySerializedAs("MoveAction")] public InputAction moveAction;
        [FormerlySerializedAs("JumpAction")] public InputAction jumpAction;

        // Variabler til spillerens bevægelse og fysik.
        public float speed; // Bevægelseshastighed.
        public float jumpForce; // Kraften ved hop.
        public float dashSpeed; // Hastighed under dash.
        public float dashDuration; // Varighed af dash.
        private Rigidbody2D _rb; // Reference til spillerens Rigidbody2D.
        private bool _isGrounded; // Om spilleren er på jorden.
        private int _jumpCount; // Antal hop udført.
        private const int MaxJumps = 1; // Maksimalt antal hop.
        private bool _isDashing; // Om spilleren dashes.
        private float _originalGravityScale; // Gemmer den oprindelige tyngdekraft.
        [UsedImplicitly] private float _dashTime; // Tidsmåler for dash.
        private bool _dashedSinceGrounded; // Om spilleren har dashet siden sidste gang på jorden.
        [UsedImplicitly] private Vector2 _lastMoveDirection; // Sidste bevægelsesretning.

        // Kamera-relaterede variabler.
        public Transform cameraTransform; // Kameraets position.
        public float cameraSmoothSpeed = 5f; // Glidende bevægelse af kameraet.

        // Angrebs-relaterede variabler.
        public float attackSpeed; // Hastighed for angreb.
        private float _attackCooldownTimer; // Cooldown for angreb.
        private bool _isAttacking; // Om spilleren angriber.
        public float attackRange = 1.5f; // Rækkevidde for angreb.
        public float attackDamage = 10f; // Skade ved angreb.

        // Dash-relaterede variabler.
        private float _lastDashTime; // Tidspunkt for sidste dash.
        private const float DoubleTapTime = 0.2f; // Maksimal tid mellem dobbelttryk.
        private KeyCode _lastKeyPressed; // Sidste tast trykket.
        private bool _isDead = false; // Om spilleren er død.

        // Cooldown for dash.
        public float dashCooldown = 1f; // Cooldown-tid for dash.
        private float _dashCooldownTimer; // Tidsmåler for dash-cooldown.

        // Animation-relaterede variabler.
        private Animator _animator; // Reference til Animator-komponenten.
        private enum PlayerState { Attack, Block, Die, Idle, Run, Walk } // Mulige spiller-tilstande.
        private PlayerState _currentState = PlayerState.Idle; // Aktuel spiller-tilstand.

        // Variabler til at håndtere spillerens retning.
        private bool _facingRight = true; // Om spilleren vender mod højre.
        private BoxCollider2D _boxCollider; // Reference til spillerens BoxCollider2D.

        // Kameraer til spil og menu.
        public Camera mainCamera; // Hovedkamera.
        public Camera menuCamera; // Menukamera.

        // Reference til spillerens helbred.
        private PlayerHealth _playerHealth;

        // Blokering-relaterede variabler.
        private bool _isBlocking; // Om spilleren blokerer.
        public float blockDuration = 1.0f; // Varighed af blokering.
        public float blockCooldown = 1.5f; // Cooldown for blokering.
        private float _blockCooldownTimer; // Tidsmåler for blok-cooldown.

        // Start-metoden initialiserer komponenter og aktiverer input.
        private void Start()
        {
            moveAction.Enable();
            jumpAction.Enable();
            _rb = GetComponent<Rigidbody2D>();
            _rb.freezeRotation = true; // Forhindrer rotation af spilleren.
            _animator = GetComponent<Animator>();
            _originalGravityScale = _rb.gravityScale; // Gemmer tyngdekraftens skala.
            _boxCollider = GetComponent<BoxCollider2D>();
            _playerHealth = GetComponent<PlayerHealth>();
        }

        // Update-metoden håndterer spillerens input og opdaterer tilstande.
        private void Update()
        {
            Vector2 move = moveAction.ReadValue<Vector2>(); // Læser bevægelsesinput.
            if (move != Vector2.zero)
            {
                _lastMoveDirection = move.normalized; // Gemmer sidste bevægelsesretning.
            }

            // Håndterer bevægelse, hvis spilleren ikke dashes.
            if (!_isDashing)
            {
                _rb.linearVelocity = new Vector2(move.x * speed, _rb.linearVelocityY); // Opdaterer hastighed.
                if (_rb.linearVelocityX == 0 && !_isAttacking) ChangeState(PlayerState.Idle); // Skifter til Idle.
                else if (!_isAttacking) ChangeState(PlayerState.Walk); // Skifter til Walk.
            }

            // Håndterer hop.
            if (jumpAction.WasPressedThisFrame() && _jumpCount < MaxJumps)
            {
                _rb.linearVelocity = new Vector2(_rb.linearVelocityX, jumpForce); // Udfører hop.
                _jumpCount++;
            }
            else if (_isGrounded && !IsTouchingWall())
            {
                _jumpCount = 0; // Nulstiller hop-tæller.
                _dashedSinceGrounded = false; // Tillader dash igen.
            }

            // Kalder metoder til dash, blokering, angreb og kamera.
            HandleDashInput();
            FollowPlayer();
            HandleBlocking();
            Attack();
            GoToMainMenu();

            // Opdaterer cooldown-timere.
            _isGrounded = IsGrounded(); // Tjekker om spilleren er på jorden.
            if (_dashCooldownTimer > 0) _dashCooldownTimer -= Time.deltaTime;
            if (_attackCooldownTimer > 0) _attackCooldownTimer -= Time.deltaTime;
            if (_blockCooldownTimer > 0) _blockCooldownTimer -= Time.deltaTime;

            // Tjekker om spilleren er død.
            if (_playerHealth.currentHealth <= 0)
            {
                Die();
            }
        }

        // FixedUpdate-metoden håndterer spillerens retning.
        private void FixedUpdate()
        {
            switch (_rb.linearVelocityX)
            {
                case > 0 when !_facingRight:
                case < 0 when _facingRight:
                    Flip(); // Skifter spillerens retning.
                    break;
            }
        }

        // Tjekker om spilleren er på jorden.
        private bool IsGrounded()
        {
            return _boxCollider.IsTouchingLayers(LayerMask.GetMask("Ground"));
        }

        // Skifter til hovedmenuen.
        private void GoToMainMenu()
        {
            if (!Input.GetKeyDown(KeyCode.Escape)) return;
            menuCamera.enabled = true;
            mainCamera.enabled = false;
        }

        // Håndterer spillerens angreb.
        private void Attack()
        {
            if (!(_attackCooldownTimer <= 0) || !Input.GetKeyDown(KeyCode.X)) return;
            _isAttacking = true;
            ChangeState(PlayerState.Attack); // Skifter til angrebstilstand.
            StartCoroutine(StopAttackAfterDelay(0.5f)); // Stopper angreb efter forsinkelse.
            PeformAttack(); // Udfører angreb.
        }

        // Udfører angreb på fjender inden for rækkevidde.
        private void PeformAttack()
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, attackRange, LayerMask.GetMask("Enemy"));
            List<Collider2D> hitEnemies = colliders.Where(collider1 => collider1 != null).ToList();

            foreach (Collider2D enemy in hitEnemies)
            {
                if (enemy.TryGetComponent(out IEnemy enemyComponent))
                {
                    enemyComponent.TakeDamage(attackDamage); // Påfører skade på fjender.
                }
            }
        }

        // Stopper angreb efter en forsinkelse.
        private IEnumerator StopAttackAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            _isAttacking = false;
            _attackCooldownTimer = attackSpeed; // Nulstiller angrebscooldown.
            ChangeState(PlayerState.Idle); // Skifter til Idle-tilstand.
        }

        // Håndterer dash-input.
        private void HandleDashInput()
        {
            if (!(_dashCooldownTimer <= 0) || (!Input.GetKeyDown(KeyCode.A) && !Input.GetKeyDown(KeyCode.D)) ||
                _dashedSinceGrounded) return;
            KeyCode currentKey = Input.GetKeyDown(KeyCode.A) ? KeyCode.A : KeyCode.D;

            if (_lastKeyPressed == currentKey && Time.time - _lastDashTime < DoubleTapTime)
            {
                StartCoroutine(Dash(currentKey == KeyCode.A ? Vector2.left : Vector2.right)); // Udfører dash.
                _dashCooldownTimer = dashCooldown; // Nulstiller dash-cooldown.
            }
            _lastDashTime = Time.time;
            _lastKeyPressed = currentKey;
        }

        // Håndterer blokering.
        private void HandleBlocking()
        {
            if (_blockCooldownTimer <= 0 && Input.GetKeyDown(KeyCode.C) && !_isBlocking) StartCoroutine(Block());
        }

        // Udfører blokering.
        private IEnumerator Block()
        {
            _isBlocking = true;
            ChangeState(PlayerState.Block); // Skifter til blokeringstilstand.
            yield return new WaitForSeconds(blockDuration); // Blokerer i en given varighed.
            _isBlocking = false;
            _blockCooldownTimer = blockCooldown; // Nulstiller blok-cooldown.
        }

        // Udfører dash i en given retning.
        private IEnumerator Dash(Vector2 direction)
        {
            _isDashing = true;
            _dashTime = dashDuration;
            _rb.linearVelocity = direction * dashSpeed; // Sætter hastighed for dash.
            _rb.gravityScale = 0; // Deaktiverer tyngdekraft under dash.

            ChangeState(PlayerState.Run); // Skifter til løbetilstand.
            yield return new WaitForSeconds(dashDuration); // Dash varer en given varighed.

            _rb.linearVelocity = Vector2.zero; // Stopper dash.
            _isDashing = false;
            _rb.gravityScale = _originalGravityScale; // Gendanner tyngdekraft.
            _dashedSinceGrounded = true; // Marker dash som udført.
        }

        // Følger spillerens position med kameraet.
        private void FollowPlayer()
        {
            if (!cameraTransform) return;
            Vector3 targetPosition = new Vector3(transform.position.x, transform.position.y, cameraTransform.position.z);
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPosition, cameraSmoothSpeed * Time.deltaTime);
        }

        // Håndterer spillerens død.
        private void Die()
        {
            if (_isDead) return; // Forhindrer flere dødsudførelser.
            _isDead = true;
            Debug.Log("Deathtriggered");
            ChangeState(PlayerState.Die); // Skifter til dødstilstand.
            SceneManager.LoadScene("MainMenu"); // Skifter til hovedmenuen.

            StartCoroutine(DeathCoroutine(1f)); // Lukker spillet efter forsinkelse.
        }

        // Coroutine til at lukke spillet efter en forsinkelse.
        private static IEnumerator DeathCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            Application.Quit();
        }

        // Tjekker om spilleren rører en væg.
        private bool IsTouchingWall()
        {
            return Physics2D.Raycast(transform.position, Vector2.right, 0.7f, LayerMask.GetMask("Ground")) ||
                   Physics2D.Raycast(transform.position, Vector2.left, 0.7f, LayerMask.GetMask("Ground"));
        }

        // Skifter spillerens tilstand og opdaterer animation.
        private void ChangeState(PlayerState newState)
        {
            if (_currentState == newState) return;
            _currentState = newState;
            _animator.Play(newState.ToString());
        }

        // Skifter spillerens retning.
        private void Flip()
        {
            _facingRight = !_facingRight;
            Vector3 theScale = transform.localScale;
            theScale.x *= -1; // Inverterer skalaen for at vende spilleren.
            transform.localScale = theScale;
        }
    }
}