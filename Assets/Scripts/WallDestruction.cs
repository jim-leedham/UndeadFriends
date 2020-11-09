using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallDestruction : MonoBehaviour
{
    public Sprite open;
    public Sprite undeadFriends;

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;

    // Start is called before the first frame update
    void Awake()
    {

        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.GetComponent<Player>())
        {
            spriteRenderer.sprite = undeadFriends;
            spriteRenderer.sortingLayerName = "Title";
        }
    }

    public void OnDestroy()
    {
        spriteRenderer.sprite = open;
        boxCollider.isTrigger = true;
    }
}
