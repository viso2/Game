using UnityEngine;

namespace Gameplay
{
    // PlatformEnemy er en specifik fjendetype, der arver fra EnemyBase.
    // Denne fjende kan bevæge sig på platforme og hoppe mod spilleren.
    public class PlatformEnemy : EnemyBase
    {
        // Kraften, der bruges til at hoppe mod spilleren.
        [SerializeField] private float jumpForce = 5f;

        // Reference til fjendens BoxCollider2D for at tjekke, om den er på en platform.
        private BoxCollider2D _boxCollider;

        // Flag, der angiver, om fjenden er på en platform.
        private bool _isOnPlatform;

        // Start-metoden initialiserer fjendens komponenter og finder dens BoxCollider2D.
        protected override void Start()
        {
            base.Start();
            _boxCollider = GetComponent<BoxCollider2D>();
        }

        // Update-metoden tjekker, om fjenden er på en platform, og opdaterer dens opførsel.
        protected override void Update()
        {
            CheckIfOnPlatform(); // Tjekker, om fjenden står på en platform.
            base.Update(); // Kalder EnemyBase's Update-metode for grundlæggende funktionalitet.
        }

        // Metode til at få fjenden til at hoppe mod spilleren.
        private void JumpToPlayer()
        {
            if (_isOnPlatform) // Kun hop, hvis fjenden er på en platform.
                Rb.linearVelocity = new Vector2(Rb.linearVelocity.x, jumpForce); // Tilføjer hopkraft til fjendens hastighed.
        }

        // Overrider MoveTowardsPlayer-metoden fra EnemyBase for at definere fjendens bevægelse.
        protected override void MoveTowardsPlayer()
        {
            if (!_isOnPlatform) return; // Hvis fjenden ikke er på en platform, gør ingenting.

            // Beregner den lodrette forskel mellem fjenden og spilleren.
            float verticalDifference = Mathf.Abs(Player.position.y - transform.position.y);

            // Hvis spilleren er højere end fjenden, hop mod spilleren.
            if (verticalDifference > 1) JumpToPlayer();
            else ChasePlayer(); // Ellers jag spilleren vandret.
        }

        // Metode til at jagte spilleren vandret.
        private void ChasePlayer()
        {
            // Beregner retningen mod spilleren og normaliserer den for at få en enhedsvektor.
            Vector2 direction = new Vector2(Player.position.x - transform.position.x, 0).normalized;

            // Sætter fjendens hastighed baseret på retningen og den definerede hastighed.
            Rb.linearVelocity = new Vector2(direction.x * speed, Rb.linearVelocity.y);
        }

        // Metode til at tjekke, om fjenden står på en platform.
        private void CheckIfOnPlatform()
        {
            // Bruges til at tjekke, om fjenden rører lag, der er markeret som "Ground".
            _isOnPlatform = _boxCollider.IsTouchingLayers(LayerMask.GetMask("Ground"));
        }
    }
}