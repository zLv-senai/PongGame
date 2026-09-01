using UnityEngine;

public class ZonaDeGol : MonoBehaviour
{
    [Tooltip("Marque se a bola entrando AQUI da ponto para o jogador 1 (o da esquerda).")]
    public bool pontoParaJogador1 = true;

    private void OnTriggerEnter2D(Collider2D outro)
    {
        if (!outro.CompareTag("Ball"))
        {
            return;
        }

        if (GameManager.instancia != null)
        {
            GameManager.instancia.MarcarPonto(pontoParaJogador1);
        }
    }
}
