using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControls : MonoBehaviour
{
    [Header("Movimento")]
    public float velocidade = 8f;
    public float limiteY = 4f;

    [Header("Qual jogador e este")]
    public bool jogador1 = true;

    private Vector3 posicaoInicial;

    private void Awake()
    {
        posicaoInicial = transform.position;
    }

    private void Update()
    {
        // Nao deixa mover enquanto o menu esta aberto ou alguem ja venceu
        if (GameManager.instancia != null && !GameManager.instancia.JogoRodando)
        {
            return;
        }

        float direcao = LerDirecao();
        transform.Translate(Vector2.up * direcao * velocidade * Time.deltaTime);
        AplicarLimites();
    }

    private float LerDirecao()
    {
        if (Keyboard.current == null)
        {
            return 0f;
        }

        float direcao = 0f;

        if (jogador1)
        {
            if (Keyboard.current.wKey.isPressed) direcao += 1f;
            if (Keyboard.current.sKey.isPressed) direcao -= 1f;
        }
        else
        {
            if (Keyboard.current.upArrowKey.isPressed) direcao += 1f;
            if (Keyboard.current.downArrowKey.isPressed) direcao -= 1f;
        }

        return direcao;
    }

    private void AplicarLimites()
    {
        Vector3 posicao = transform.position;
        posicao.y = Mathf.Clamp(posicao.y, -limiteY, limiteY);
        transform.position = posicao;
    }

    public void VoltarAoInicio()
    {
        transform.position = posicaoInicial;
    }
}
