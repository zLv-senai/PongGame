using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class GameManager : MonoBehaviour
{
    // Referencia global: qualquer script alcanca o GameManager por GameManager.instancia
    public static GameManager instancia;

    [Header("Regras")]
    public int pontosParaVencer = 5;
    public float tempoEntrePontos = 1f;

    [Header("Visual")]
    public Font fonteJogo;
    public Color corDaUI = Color.white;

    [Header("Enquadramento")]
    public float margemLateral = 0.8f;

    [Header("Som")]
    public AudioClip somPonto;

    // Encontrados sozinhos no Awake
    private BallControls bola;
    private PlayerControls raqueteEsquerda;
    private PlayerControls raqueteDireita;

    // Criados por codigo em ConstruirInterface()
    private Text textoPlacarEsquerda;
    private Text textoPlacarDireita;
    private GameObject painelMenu;
    private GameObject painelVitoria;
    private Text textoVitoria;

    private int pontosJogador1;
    private int pontosJogador2;
    private AudioSource fonteDeAudio;

    // Outros scripts leem, so o GameManager altera
    public bool JogoRodando { get; private set; }

    private void Awake()
    {
        instancia = this;

        fonteDeAudio = GetComponent<AudioSource>();
        if (fonteDeAudio == null)
        {
            fonteDeAudio = gameObject.AddComponent<AudioSource>();
        }
        fonteDeAudio.playOnAwake = false;

        if (fonteJogo == null)
        {
            fonteJogo = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        // Se a cor vier zerada da serializacao, a UI ficaria invisivel
        if (corDaUI.a <= 0f)
        {
            corDaUI = Color.white;
        }

        ProcurarObjetosDaCena();
        ConstruirInterface();
        PosicionarRaquetes();

        if (Camera.main != null)
        {
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
            Camera.main.backgroundColor = Color.black;
        }
    }

    private void Start()
    {
        MostrarMenu();
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            MostrarMenu();
        }
    }

    // ---------- montagem automatica ----------

    private void ProcurarObjetosDaCena()
    {
        bola = FindFirstObjectByType<BallControls>();

        PlayerControls[] raquetes = FindObjectsByType<PlayerControls>(FindObjectsSortMode.None);
        foreach (PlayerControls raquete in raquetes)
        {
            if (raquete.jogador1) raqueteEsquerda = raquete;
            else raqueteDireita = raquete;
        }
    }

    // A largura visivel depende do formato da janela, entao a posicao
    // das raquetes vem da camera em vez de ser cravada na cena
    private void PosicionarRaquetes()
    {
        if (Camera.main == null) return;

        float metadeLargura = Camera.main.orthographicSize * Camera.main.aspect;
        float x = metadeLargura - margemLateral;

        if (raqueteEsquerda != null)
        {
            raqueteEsquerda.transform.position = new Vector3(-x, 0f, 0f);
        }

        if (raqueteDireita != null)
        {
            raqueteDireita.transform.position = new Vector3(x, 0f, 0f);
        }
    }

    private void ConstruirInterface()
    {
        GameObject canvasGO = new GameObject("CanvasJogo", typeof(RectTransform));
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler escala = canvasGO.AddComponent<CanvasScaler>();
        escala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        escala.referenceResolution = new Vector2(1920f, 1080f);

        canvasGO.AddComponent<GraphicRaycaster>();

        // Sem EventSystem os botoes nao respondem ao clique
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject eventos = new GameObject("EventSystem");
            eventos.AddComponent<EventSystem>();
            eventos.AddComponent<InputSystemUIInputModule>();
        }

        Transform raiz = canvasGO.transform;

        textoPlacarEsquerda = CriarTexto(raiz, "PlacarEsquerda", "0", 120,
            new Vector2(0.5f, 1f), new Vector2(-300f, -150f), new Vector2(300f, 200f));

        textoPlacarDireita = CriarTexto(raiz, "PlacarDireita", "0", 120,
            new Vector2(0.5f, 1f), new Vector2(300f, -150f), new Vector2(300f, 200f));

        painelMenu = CriarPainel(raiz, "PainelMenu");
        CriarTexto(painelMenu.transform, "Titulo", "PONG", 160,
            new Vector2(0.5f, 0.5f), new Vector2(0f, 230f), new Vector2(900f, 240f));
        CriarTexto(painelMenu.transform, "Instrucoes", "JOGADOR 1: W / S          JOGADOR 2: SETAS", 34,
            new Vector2(0.5f, 0.5f), new Vector2(0f, 80f), new Vector2(1200f, 60f));
        CriarBotao(painelMenu.transform, "BotaoJogar", "JOGAR", new Vector2(0f, -50f), Jogar);
        CriarBotao(painelMenu.transform, "BotaoSair", "SAIR", new Vector2(0f, -170f), Sair);

        painelVitoria = CriarPainel(raiz, "PainelVitoria");
        textoVitoria = CriarTexto(painelVitoria.transform, "TextoVitoria", "", 90,
            new Vector2(0.5f, 0.5f), new Vector2(0f, 120f), new Vector2(1600f, 220f));
        CriarBotao(painelVitoria.transform, "BotaoMenu", "MENU", new Vector2(0f, -90f), MostrarMenu);
        painelVitoria.SetActive(false);
    }

    private Text CriarTexto(Transform pai, string nome, string conteudo, int tamanho,
                            Vector2 ancora, Vector2 posicao, Vector2 dimensao)
    {
        GameObject go = new GameObject(nome, typeof(RectTransform));
        go.transform.SetParent(pai, false);

        Text texto = go.AddComponent<Text>();
        texto.font = fonteJogo;
        texto.text = conteudo;
        texto.fontSize = tamanho;
        texto.color = corDaUI;
        texto.alignment = TextAnchor.MiddleCenter;
        texto.horizontalOverflow = HorizontalWrapMode.Overflow;
        texto.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform rt = texto.rectTransform;
        rt.anchorMin = ancora;
        rt.anchorMax = ancora;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = posicao;
        rt.sizeDelta = dimensao;

        return texto;
    }

    private GameObject CriarPainel(Transform pai, string nome)
    {
        GameObject go = new GameObject(nome, typeof(RectTransform));
        go.transform.SetParent(pai, false);

        Image fundo = go.AddComponent<Image>();
        fundo.color = new Color(0f, 0f, 0f, 0.88f);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        return go;
    }

    private void CriarBotao(Transform pai, string nome, string rotulo, Vector2 posicao,
                            UnityEngine.Events.UnityAction acao)
    {
        GameObject go = new GameObject(nome, typeof(RectTransform));
        go.transform.SetParent(pai, false);

        Image fundo = go.AddComponent<Image>();
        fundo.color = corDaUI;

        Button botao = go.AddComponent<Button>();
        botao.targetGraphic = fundo;
        botao.onClick.AddListener(acao);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = posicao;
        rt.sizeDelta = new Vector2(380f, 90f);

        Text texto = CriarTexto(go.transform, "Texto", rotulo, 44,
            new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(380f, 90f));
        texto.color = Color.black;
    }

    // ---------- fluxo do jogo ----------

    public void MostrarMenu()
    {
        JogoRodando = false;
        CancelInvoke();

        pontosJogador1 = 0;
        pontosJogador2 = 0;
        AtualizarPlacar();

        if (painelMenu != null) painelMenu.SetActive(true);
        if (painelVitoria != null) painelVitoria.SetActive(false);
        if (bola != null) bola.Parar();
    }

    public void Jogar()
    {
        CancelInvoke();

        pontosJogador1 = 0;
        pontosJogador2 = 0;
        AtualizarPlacar();

        if (painelMenu != null) painelMenu.SetActive(false);
        if (painelVitoria != null) painelVitoria.SetActive(false);

        if (raqueteEsquerda != null) raqueteEsquerda.VoltarAoInicio();
        if (raqueteDireita != null) raqueteDireita.VoltarAoInicio();

        JogoRodando = true;
        Invoke(nameof(LancarBola), tempoEntrePontos);
    }

    private void LancarBola()
    {
        if (!JogoRodando || bola == null) return;

        int direcao = (Random.value < 0.5f) ? -1 : 1;
        bola.Lancar(direcao);
    }

    public void MarcarPonto(bool pontoParaJogador1)
    {
        if (!JogoRodando) return;

        if (pontoParaJogador1) pontosJogador1++;
        else pontosJogador2++;

        AtualizarPlacar();

        if (fonteDeAudio != null && somPonto != null)
        {
            fonteDeAudio.PlayOneShot(somPonto);
        }

        if (bola != null) bola.Parar();

        if (pontosJogador1 >= pontosParaVencer || pontosJogador2 >= pontosParaVencer)
        {
            Vencer(pontosJogador1 > pontosJogador2);
        }
        else
        {
            Invoke(nameof(LancarBola), tempoEntrePontos);
        }
    }

    private void Vencer(bool jogador1Venceu)
    {
        JogoRodando = false;
        CancelInvoke();

        if (bola != null) bola.Parar();
        if (painelVitoria != null) painelVitoria.SetActive(true);

        if (textoVitoria != null)
        {
            textoVitoria.text = jogador1Venceu ? "JOGADOR 1 VENCEU!" : "JOGADOR 2 VENCEU!";
        }
    }

    private void AtualizarPlacar()
    {
        if (textoPlacarEsquerda != null) textoPlacarEsquerda.text = pontosJogador1.ToString();
        if (textoPlacarDireita != null) textoPlacarDireita.text = pontosJogador2.ToString();
    }

    public void Sair()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}