using UnityEngine;
using UnityEngine.InputSystem;

public class SpellSpawner : MonoBehaviour
{

    [Header("Area de spawn")]
    public float limitex = 5f;
    public float limitey = 3.5f;

    [Header("Sorteio")]
    [Range(0f, 1f)]
    public float chanceDeSpellBom = 0.7f;
    public float chanceDeSpawn = 0.3f;
    private Vector2 SortearPosicao()
    {
        float x = Random.Range(-limitex, limitex);
        float y = Random.Range(-limitey, limitey);
        return new Vector2(x, y);
    }

    private SpellType SortearTipo()
    {
        int totalTipos = System.Enum.GetValues(typeof(SpellType)).Length;
        int indiceSorteado = Random.Range(0, totalTipos);
        return (SpellType)indiceSorteado;
    }

    private bool SpawnRate()
    {
        return Random.value < chanceDeSpawn;
    }

     private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Vector2 posicao = SortearPosicao();
            SpellType tipo = SortearTipo();
            bool respinga = SpawnRate();

            Debug.Log("Spawn " + tipo + " em " + posicao + " | respinga: " + respinga);
        }
    }
    
}
