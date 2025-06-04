namespace Core
{
    // IEnemy er en interface, der definerer grundlæggende funktionalitet for fjender i spillet.
    public interface IEnemy 
    {
        // Metode, der skal implementeres af klasser, der repræsenterer fjender.
        // Denne metode håndterer, hvordan fjenden tager skade.
        void TakeDamage(float damage);
    }
}