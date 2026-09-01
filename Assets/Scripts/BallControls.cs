using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BallControls : MonoBehaviour
{
    [Header("Velocidade")]
    public float velocidadeInicial = 6f;
    public float aumentoPorRebatida = 0.4f;
    public float velocidadeMaxima = 14f;

    [Header("Angulo de saida da raquete (graus)")]
    public float anguloMaximo = 50f;

    [Header("Som")]
    public AudioClip somRebatida;

    private Rigidbody2D rb;
    private AudioSource fonteDeAudio;
    private Vector3 posicaoInicial;
    private float velocidadeAtual;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        fonteDeAudio = GetComponent<AudioSource>();
        posicaoInicial = transform.position;
        velocidadeAtual = velocidadeInicial;
    }

    // Congela a bola no centro (usado no menu e depois de cada ponto)
    public void Parar()
    {
        rb.linearVelocity = Vector2.zero;
        transform.position = posicaoInicial;
    }

    // direcaoHorizontal: -1 lanca para a esquerda, 1 para a direita
    public void Lancar(int direcaoHorizontal)
    {
        transform.position = posicaoInicial;
        velocidadeAtual = velocidadeInicial;

        float anguloEmGraus = Random.Range(-25f, 25f);
        float anguloEmRadianos = anguloEmGraus * Mathf.Deg2Rad;

        Vector2 direcao = new Vector2(
            direcaoHorizontal * Mathf.Cos(anguloEmRadianos),
            Mathf.Sin(anguloEmRadianos)
        );

        rb.linearVelocity = direcao.normalized * velocidadeAtual;
    }

    private void FixedUpdate()
    {
        // Garante que a bola nunca perca velocidade nas colisoes
        if (rb.linearVelocity != Vector2.zero)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * velocidadeAtual;
        }
    }

    private void OnCollisionEnter2D(Collision2D colisao)
    {
        if (colisao.gameObject.CompareTag("Player"))
        {
            RebaterNaRaquete(colisao);
        }
        else
        {
            // Parede: reflete usando a normal do ponto de contato
            Vector2 normal = colisao.GetContact(0).normal;
            Vector2 refletida = Vector2.Reflect(rb.linearVelocity, normal);
            rb.linearVelocity = refletida.normalized * velocidadeAtual;
        }

        TocarSom();
    }

    private void RebaterNaRaquete(Collision2D colisao)
    {
        Transform raquete = colisao.transform;
        float meiaAlturaRaquete = colisao.collider.bounds.size.y / 2f;

        // Onde na raquete a bola bateu: -1 (base), 0 (meio), 1 (topo)
        float diferenca = transform.position.y - raquete.position.y;
        float fator = Mathf.Clamp(diferenca / meiaAlturaRaquete, -1f, 1f);

        float angulo = fator * anguloMaximo * Mathf.Deg2Rad;
        float direcaoX = (transform.position.x > raquete.position.x) ? 1f : -1f;

        Vector2 novaDirecao = new Vector2(
            direcaoX * Mathf.Cos(angulo),
            Mathf.Sin(angulo)
        );

        velocidadeAtual = Mathf.Min(velocidadeAtual + aumentoPorRebatida, velocidadeMaxima);
        rb.linearVelocity = novaDirecao.normalized * velocidadeAtual;
    }

    private void TocarSom()
    {
        if (fonteDeAudio != null && somRebatida != null)
        {
            fonteDeAudio.PlayOneShot(somRebatida);
        }
    }
}
