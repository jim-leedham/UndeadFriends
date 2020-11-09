using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bones : MonoBehaviour
{
    public GameObject knight;
    private SpriteRenderer spriteRenderer;

    public void Awake()
    {

        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void OnResurrect(float summonHP)
    {
        GameObject knightGO = Instantiate(knight, (Vector2)transform.position + new Vector2(0.0f, -0.5f), Quaternion.identity);
        KnightFriendly realKnight = knightGO.GetComponent<KnightFriendly>();
        realKnight.SetHP(summonHP);

        gameObject.SetActive(false);
    }

    public void Update()
    {
        spriteRenderer.sortingOrder = Mathf.RoundToInt(transform.position.y * 100f) * -1;

    }

}
