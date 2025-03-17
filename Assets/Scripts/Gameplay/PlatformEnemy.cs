using UnityEngine;

namespace Gameplay
{
    public class PlatformEnemy : EnemyBase
    {

        [SerializeField] private float jumpForce = 5f;
        private BoxCollider2D _boxCollider;
        private bool _isOnPlatform;

        protected override void Start()
        {
            base.Start();
            _boxCollider = GetComponent<BoxCollider2D>();
        }

        protected override void Update()
        {
            CheckIfOnPlatform();
            base.Update();
        }

        private void JumpToPlayer()
        {
            if (_isOnPlatform)
                Rb.linearVelocity = new Vector2(Rb.linearVelocityX, jumpForce);
        }

        protected override void MoveTowardsPlayer()
        {
            if (!_isOnPlatform) return;

            float verticalDifference = Mathf.Abs(Player.position.y - transform.position.y);
            if (verticalDifference > 1) JumpToPlayer();
            else ChasePlayer();
        }

        private void ChasePlayer()
        {
            Vector2 direction = new Vector2(Player.position.x - transform.position.x, 0).normalized;
            Rb.linearVelocity = new Vector2(direction.x * speed, Rb.linearVelocityY);
        }

        private void CheckIfOnPlatform()
        {
            _isOnPlatform = _boxCollider.IsTouchingLayers(LayerMask.GetMask("Ground"));
        }
    }
}