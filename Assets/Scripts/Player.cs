using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float movementSpeed = 1.0f;
    public float summonRadius = 3.0f;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rigidBody2D;

    private float horizontalMovement = 0.0f;
    private float verticalMovement = 0.0f;
    private bool chargingSummon = false;

    public float health = 10.0f;
    private float maxHealth;

    public float healthBarMaxScale = 5.5f;
    private SpriteRenderer healthBar;
    public bool godmode = true;

    public float mana = 10.0f;
    private float maxMana;
    public float manaRegen = 0.2f;
    public float manaDrainWhileHeld = 0.5f;
    private SpriteRenderer manaBar;

    private float resurrectionHeldTime = 0.0f;
    public float resurrectionFadeUpTime = 3.0f;
    private SpriteRenderer resurrectionSpriteRenderer;

    private bool resurrectionSpriteOn = false;


    void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rigidBody2D = GetComponent<Rigidbody2D>();

        maxHealth = health;

        Transform healthBarTransform = transform.Find("HealthBar");
        healthBar = healthBarTransform.GetComponent<SpriteRenderer>();


        maxMana = mana;
        Transform manaBarTransform = transform.Find("ManaBar");
        manaBar = manaBarTransform.GetComponent<SpriteRenderer>();

        Transform resurrectionTransform = transform.Find("Resurrection");
        resurrectionSpriteRenderer = resurrectionTransform.GetComponent<SpriteRenderer>();

    }

    // Update is called once per frame
    void Update()
    {
        spriteRenderer.sortingOrder = Mathf.RoundToInt(transform.position.y * 100f) * -1;
        healthBar.sortingOrder = spriteRenderer.sortingOrder;
        manaBar.sortingOrder = spriteRenderer.sortingOrder;


        float fire2 = Input.GetAxisRaw("Fire2");
        if(fire2 > 0.0f)
        {
            GameManager.instance.Boomer();
            return;
        }

        float chargingSummonInput = Input.GetAxisRaw("Fire1");
        if(chargingSummonInput > 0.0f && !chargingSummon)
        {
            chargingSummon = true;
            animator.SetBool("Moving", false);
            animator.SetTrigger("ChargingSummon");
            return;
        }

        if(chargingSummon)
        {
            if (chargingSummonInput < float.Epsilon)
            {
                // button released!
                animator.ResetTrigger("ChargingSummon");
                animator.SetTrigger("SummonRelease");
                chargingSummon = false;

                float summonHP = (resurrectionHeldTime / resurrectionFadeUpTime);
                GameManager.instance.OnSummonPerformed(transform.position, summonRadius, summonHP);

                resurrectionHeldTime = 0.0f;
                Color color = resurrectionSpriteRenderer.color;
                color.a = 0.0f;
                resurrectionSpriteRenderer.color = color;
                resurrectionSpriteRenderer.enabled = true;
            }
            else
            {
                // charge held
                resurrectionHeldTime += Time.deltaTime;

                // drain mana
                mana -= (manaDrainWhileHeld * Time.deltaTime);
                Vector3 localScale = manaBar.transform.localScale;
                localScale.x = healthBarMaxScale * (mana / maxMana);
                manaBar.transform.localScale = localScale;

                // illuminate floor
                Color color = resurrectionSpriteRenderer.color;
                color.a = (resurrectionHeldTime / (resurrectionFadeUpTime * 2.0f));
                resurrectionSpriteRenderer.color = color;


            }
            return;
        }
        else
        {
            if(mana < maxMana)
            {
                mana += manaRegen * Time.deltaTime;
                Vector3 localScale = manaBar.transform.localScale;
                localScale.x = healthBarMaxScale * (mana / maxMana);
                manaBar.transform.localScale = localScale;
            }
        }


        horizontalMovement = Input.GetAxisRaw("Horizontal");
        verticalMovement = Input.GetAxisRaw("Vertical");

        int horizontal = (int)horizontalMovement;
        int vertical = (int)verticalMovement;

        if (vertical < 0)
        {
            Debug.Log("vert" + vertical);
            animator.SetBool("Moving", true);
            animator.SetTrigger("MoveDown");
        }
        else if (vertical > 0)
        {
            Debug.Log("vert" + vertical);
            animator.SetBool("Moving", true);
            animator.SetTrigger("MoveUp");
        }
        else if (horizontal != 0)
        {
            Debug.Log("hor" + vertical);
            animator.SetBool("Moving", true);
            animator.SetTrigger("MoveHorizontal");
            spriteRenderer.flipX = (horizontal < 0 ? true : false);
        }
        //else if(vertical < 0)
        //{
        //    animator.SetBool("Moving", true);
        //    animator.SetTrigger("MoveDown");
        //}
        //else if (vertical > 0)
        //{
        //    animator.SetBool("Moving", true);
        //    animator.SetTrigger("MoveUp");
        //}
        else
        {
            animator.SetBool("Moving", false);
        }
    }

    void FixedUpdate()
    {
        if(!chargingSummon)
        {
            float overage = Mathf.Abs(horizontalMovement) + Mathf.Abs(verticalMovement);
            if (overage > 1.5f)
            {
                float equaliser = ((overage - 1.5f) / 2.0f);
                horizontalMovement += (horizontalMovement < 0.0f ? equaliser : -equaliser); //((overage-1.0f) / 2.0f);
                verticalMovement += (verticalMovement < 0.0f ? equaliser : -equaliser); //
                                                                                        //verticalMovement -= ((overage-1.0f) / 2.0f);
            }

            Vector2 newPosition = (Vector2)transform.position + new Vector2(horizontalMovement * movementSpeed * Time.deltaTime, verticalMovement * movementSpeed * Time.deltaTime);

            rigidBody2D.MovePosition(newPosition);
        }
        else
        {
            if (resurrectionHeldTime > resurrectionFadeUpTime)
            {
                resurrectionSpriteOn = !resurrectionSpriteOn;
                resurrectionSpriteRenderer.enabled = resurrectionSpriteOn;

            }
        }


    }

    public bool TakeDamage(float dmg)
    {
        if (godmode)
            return false;

        health -= dmg;
        if (health <= 0.0f)
        {
            gameObject.SetActive(false);
        }

        Vector3 localScale = healthBar.transform.localScale;
        localScale.x = healthBarMaxScale * (health / maxHealth);
        healthBar.transform.localScale = localScale;

        if(health < (maxHealth * 0.33))
        {
            healthBar.color = new Color(255, 0, 0);
        }
        else if(health < (maxHealth * 0.66f))
        {
            healthBar.color = new Color(255, 100, 0);
        }

        return (health <= 0.0f);
    }
}
