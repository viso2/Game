using Gameplay;
using UnityEngine;

// FlyingEnemy er en specifik fjendetype, der arver fra EnemyBase.
// Denne fjende kan flyve og bevæge sig mod spilleren.
public class FlyingEnemy : EnemyBase
{
    // Overrider Update-metoden fra EnemyBase for at tilføje logik for flyvning.
    protected override void Update()
    {
        // Kalder baseklassen' Update-metode for at bevare grundlæggende funktionalitet.
        base.Update();

        // Skifter tilstanden til "Idle", hvis fjenden ikke bevæger sig, ellers til "Flying".
        ChangeState(Rb.linearVelocityX == 0 ? EnemyState.Idle : EnemyState.Flying);
    }

    // Overrider MoveTowardsPlayer-metoden fra EnemyBase for at definere fjendens bevægelse.
    protected override void MoveTowardsPlayer()
    {
        // Beregner retningen mod spilleren og normaliserer den for at få en enhedsvektor.
        Vector2 direction = (Player.position - transform.position).normalized;

        // Sætter fjendens hastighed baseret på retningen og den definerede hastighed.
        Rb.linearVelocity = direction * speed;
    }
}