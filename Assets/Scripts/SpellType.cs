using UnityEngine;

public class Spell : MonoBehaviour
{
    // Preenchido pelo spawner no momento da criação
    public SpellType tipo;
    public bool afetaOsDois;

    private void OnTriggerEnter2D(Collider2D outro)
    {
        // Só a bola coleta — raquete e parede passam batido
        BallControls bola = outro.GetComponent<BallControls>();
        if (bola == null) return;

        Debug.Log("Coletado: " + tipo + " | afeta os dois: " + afetaOsDois);

        // O objeto some depois de coletado
        Destroy(gameObject);
    }
}