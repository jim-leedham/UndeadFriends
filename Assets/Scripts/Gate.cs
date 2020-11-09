using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : MonoBehaviour
{
    public int number = 4;

    public Sprite gateOff;
    public Sprite gateNum4;
    public Sprite gateNum6;
    public Sprite gateNum10;

    private SpriteRenderer spriteRenderer;
    private SpriteRenderer numberSpriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();


        Transform numberTransform = transform.Find("Number");
        numberSpriteRenderer = numberTransform.GetComponent<SpriteRenderer>();

        if(number == 4)
        {
            numberSpriteRenderer.sprite = gateNum4;
        }
        else if(number == 6)
        {
            numberSpriteRenderer.sprite = gateNum6;
        }
        else if (number == 10)
        {
            numberSpriteRenderer.sprite = gateNum10;
        }
    }

    // when the GameObjects collider arrange for this GameObject to travel to the left of the screen
    void OnTriggerEnter2D(Collider2D col)
    {
        Player player = col.GetComponent<Player>();
        if (player && GameManager.instance.GetNumFriendlyKnights() >= number)
        {
            OpenGate();
        }
    }

    private void OpenGate()
    {

        Transform numberTransform = transform.Find("Number");
        numberTransform.GetComponent<BoxCollider2D>().enabled = false;

        spriteRenderer.sprite = gateOff;
        numberSpriteRenderer.sprite = null;
    }

    // Update is called once per frame
    void Update()
    {
        spriteRenderer.sortingOrder = Mathf.RoundToInt(transform.position.y * 100f) * -1;
    }
}
