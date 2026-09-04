using UnityEngine;
using UnityEngine.InputSystem;

public class SpellSpawner : MonoBehaviour
{
    [Header("Area de spawn")]
    public float limiteX = 5f;
    public float limiteY = 3.5f;

    [Header("Sorteio")]
    [Range(0f, 1f)]
    public float chanceDeSpellBom = 0.7f;

    [Range(0f, 1f)]
    public float chanceDeRespingo = 0.3f;

    private SpellType[] spellsBons = { SpellType.SlowBall, SpellType.BigBall, SpellType.BigPaddle };
    private SpellType[] spellsRuins = { SpellType.RapidBall, SpellType.SmallBall, SpellType.SmallPaddle };

    private Vector2 SortearPosicao()
    {
        float x = Random.Range(-limiteX, limiteX);
        float y = Random.Range(-limiteY, limiteY);
        return new Vector2(x, y);
    }

    private SpellType SortearSpellType()
    {
        bool bom = Random.value < chanceDeSpellBom;
        SpellType[] grupo = bom ? spellsBons : spellsRuins;

        int indice = Random.Range(0, grupo.Length);
        return grupo[indice];
    }

    private bool SortearRespingo()
    {
        return Random.value < chanceDeRespingo;
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Vector2 posicao = SortearPosicao();
            SpellType tipo = SortearSpellType();
            bool respinga = SortearRespingo();

            Debug.Log("Spawn " + tipo + " em " + posicao + " | respinga: " + respinga);
        }
    }
}