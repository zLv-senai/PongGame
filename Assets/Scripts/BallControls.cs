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

    [Tooltip("Componente horizontal minima. Evita a bola ficar quicando so entre as paredes.")]
    public float minimoHorizontal = 0.35f;

    [Header("Som")]
    public AudioClip somRebatida;

    private Rigidbody2D rb;
    private AudioSource fonteDeAudio;
    private Vector3 posicaoInicial;
    private float velocidadeAtual;

    // A velocidade que a bola tinha ANTES da fisica processar a colisao deste passo.
    // Sem isso o rebote nas paredes nao funciona (ver OnCollisionEnter2D).
    private Vector2 velocidadeAntesDaColisao;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        fonteDeAudio = GetComponent<AudioSource>();
        posicaoInicial = transform.position;
        velocidadeAtual = velocidadeInicial;
    }

    public void Parar()
    {
        rb.linearVelocity = Vector2.zero;
        velocidadeAntesDaColisao = Vector2.zero;
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
        velocidadeAntesDaColisao = rb.linearVelocity;
    }

    private void FixedUpdate()
    {
        Vector2 atual = rb.linearVelocity;

        // Mantem a velocidade constante (a fisica sempre rouba um pouco nas colisoes)
        if (atual != Vector2.zero)
        {
            atual = atual.normalized * velocidadeAtual;
            rb.linearVelocity = atual;
        }

        // O FixedUpdate roda ANTES da simulacao deste passo, entao aqui a velocidade
        // ainda esta intacta. E dela que o rebote precisa.
        velocidadeAntesDaColisao = atual;
    }

    private void OnCollisionEnter2D(Collision2D colisao)
    {
        if (colisao.gameObject.GetComponent<PlayerControls>() != null)
        {
            RebaterNaRaquete(colisao);
        }
        else
        {
            RebaterNaParede(colisao);
        }

        TocarSom();
    }

    private void RebaterNaParede(Collision2D colisao)
    {
        Vector2 normal = colisao.GetContact(0).normal;

        // NAO usar rb.linearVelocity aqui: quando este metodo roda, a fisica ja
        // zerou a componente vertical no impacto. Refletir um vetor ja horizontal
        // devolve o mesmo vetor horizontal, e a bola sai reto para sempre.
        Vector2 velocidade = velocidadeAntesDaColisao;
        if (velocidade == Vector2.zero)
        {
            velocidade = rb.linearVelocity;
        }

        Vector2 refletida = Vector2.Reflect(velocidade, normal).normalized;
        refletida = GarantirAvanco(refletida);

        rb.linearVelocity = refletida * velocidadeAtual;
        velocidadeAntesDaColisao = rb.linearVelocity;
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
        ).normalized;

        velocidadeAtual = Mathf.Min(velocidadeAtual + aumentoPorRebatida, velocidadeMaxima);

        rb.linearVelocity = novaDirecao * velocidadeAtual;
        velocidadeAntesDaColisao = rb.linearVelocity;
    }

    // Se a direcao ficar quase vertical, a bola quica entre as paredes sem nunca
    // chegar numa raquete. Isso forca um minimo de avanco horizontal.
    private Vector2 GarantirAvanco(Vector2 direcao)
    {
        if (Mathf.Abs(direcao.x) >= minimoHorizontal)
        {
            return direcao;
        }

        float sinal = (direcao.x >= 0f) ? 1f : -1f;
        float y = Mathf.Sign(direcao.y) * Mathf.Sqrt(1f - (minimoHorizontal * minimoHorizontal));

        return new Vector2(sinal * minimoHorizontal, y).normalized;
    }

    private void TocarSom()
    {
        if (fonteDeAudio != null && somRebatida != null)
        {
            fonteDeAudio.PlayOneShot(somRebatida);
        }
    }
}