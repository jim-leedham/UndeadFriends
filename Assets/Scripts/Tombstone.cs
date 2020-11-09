using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tombstone : MonoBehaviour
{
    public Sprite vacantSprite;
    public GameObject knight;
    private SpriteRenderer spriteRenderer;

    private bool vacant = false;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void OnRessurect(float summonHP)
    {
        if(!vacant)
        {
            GameObject knightGO = Instantiate(knight, (Vector2)transform.position + new Vector2(0.0f, -0.5f), Quaternion.identity);
            KnightFriendly realKnight = knightGO.GetComponent<KnightFriendly>();
            realKnight.SetHP(summonHP);

            spriteRenderer.sprite = vacantSprite;
            vacant = true;
        }

    }

    public void Update()
    {

        spriteRenderer.sortingOrder = Mathf.RoundToInt(transform.position.y * 100f) * -1;
    }
}
