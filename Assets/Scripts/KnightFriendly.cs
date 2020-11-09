
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnightFriendly : MonoBehaviour
{

    public float moveSpeed = 1.0f;
    public float haltDistance = 3.0f;

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Rigidbody2D rigidbody2D;

    private bool goToMouseClickPos = false;
    private Vector2 mouseClickPos;

    public CircleCollider2D aggroCollider;

    private MonoBehaviour target;
    private bool pathToPlayer = true;

    public float attackCooldown = 2.0f;
    private float cooldownRemaining = -1.0f;

    public float damage = 1.0f;
    public float health = 10.0f;
    private float maxHealth;

    public float healthBarMaxScale = 5.5f;
    private SpriteRenderer healthBar;


    private AudioSource audioSource;
    public List<AudioClip> blips;

    public bool red = false;
    public GameObject bones;

    void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rigidbody2D = GetComponent<Rigidbody2D>();

        audioSource = GetComponent<AudioSource>();

        mouseClickPos = new Vector2(0.0f, 0.0f);


        maxHealth = health;

        Transform healthBarTransform = transform.Find("HealthBar");
        healthBar = healthBarTransform.GetComponent<SpriteRenderer>();

        GameManager.instance.knightFriendlies.Add(this);
    }

    // when the GameObjects collider arrange for this GameObject to travel to the left of the screen
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag != "Character")
            return;

        if (target == null && !col.GetComponent<KnightFriendly>())
            target = col.GetComponent<MonoBehaviour>();

        if(col.GetComponent<Player>())
        {
            pathToPlayer = true;
        }
    }


    public void FixedUpdate()
    {

        spriteRenderer.sortingOrder = Mathf.RoundToInt(transform.position.y * 100f) * -1;
        healthBar.sortingOrder = spriteRenderer.sortingOrder;

        if (cooldownRemaining > 0.0f)
        {
            cooldownRemaining -= Time.deltaTime;
        }

        // path find to the player!
        Vector2 playerPos = GameManager.instance.GetPlayerPosition();
        Vector2 toPlayer = playerPos - (Vector2)transform.position;
        float toPlayerDist = Vector2.Distance(playerPos, (Vector2)transform.position);

        bool travel = false;

        if (goToMouseClickPos)
        {
            toPlayer = mouseClickPos - (Vector2)transform.position;
            toPlayerDist = Vector2.Distance(mouseClickPos, (Vector2)transform.position);
            travel = true;
        }
        else if(target != null)
        {
            toPlayer = (Vector2)target.transform.position - (Vector2)transform.position; 
            toPlayerDist = Vector2.Distance((Vector2)target.transform.position, (Vector2)transform.position);

            travel = true;
        }
        else if(pathToPlayer)
        {
            playerPos = GameManager.instance.GetPlayerPosition();
            toPlayer = playerPos - (Vector2)transform.position;
            toPlayerDist = Vector2.Distance(playerPos, (Vector2)transform.position);
            travel = true;
        }

        if (toPlayerDist < haltDistance)
        {
            // Already very close! Do nothing
            rigidbody2D.velocity = new Vector2(0.0f, 0.0f);
            animator.SetBool("Moving", false);

            if (goToMouseClickPos)
            {
                goToMouseClickPos = false;
                pathToPlayer = false;
                // check for enemies near
                // check in case any other potential targets close by
                Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, aggroCollider.radius);
                int i = 0;
                while (i < hitColliders.Length)
                {
                    if (hitColliders[i].GetComponent<KnightEnemy>())
                    {
                        target = hitColliders[i].GetComponent<MonoBehaviour>();
                        break;
                    }
                    i++;
                }
            }

            if(target != null && target.GetComponent<KnightEnemy>())
            {
                Attack();
            }
        }
        else if(travel)
        {
            Vector2 newPosition = (Vector2)transform.position + (toPlayer.normalized * moveSpeed * Time.deltaTime);
            rigidbody2D.MovePosition(newPosition);

            animator.SetBool("Moving", true);
            if (Mathf.Abs(toPlayer.normalized.x) > Mathf.Abs(toPlayer.normalized.y))
            {
                animator.SetTrigger("MoveHorizontal");
                spriteRenderer.flipX = (toPlayer.normalized.x < 0.0f ? true : false);
            }
            else
            {
                if(toPlayer.normalized.y > 0.0f)
                {
                    animator.SetTrigger("MoveUp");
                }
                else
                {
                    animator.SetTrigger("MoveDown");
                }
            }
        }
    }

    internal void GoToTarget(KnightEnemy knight)
    {
        target = knight;
    }

    public bool TakeDamage(float dmg, MonoBehaviour aggressor)
    {
        health -= dmg;
        target = aggressor;
        if (health <= 0.0f)
        {
            gameObject.SetActive(false);
            GameManager.instance.knightFriendlies.Remove(this);
        }
        else
        {
            Vector3 localScale = healthBar.transform.localScale;
            localScale.x = healthBarMaxScale * (health / maxHealth);
            healthBar.transform.localScale = localScale;

            if (health < (maxHealth * 0.33))
            {
                healthBar.color = new Color(255, 0, 0);
            }
            else if (health < (maxHealth * 0.66f))
            {
                healthBar.color = new Color(255, 100, 0);
            }
        }
        return (health <= 0.0f);
    }

    public void GoToPosition(Vector2 position)
    {
        pathToPlayer = false;
        mouseClickPos = position;
        target = null;
        goToMouseClickPos = true;
    }

    private void Attack()
    {
        if (cooldownRemaining < 0.0f)
        {
            animator.SetTrigger("OnAttack");
            KnightEnemy knight = target.GetComponent<KnightEnemy>();

            audioSource.clip = blips[Random.Range(0, blips.Count - 1)];
            audioSource.Play();

            if (knight != null)
            {
                bool dead = knight.TakeDamage(damage, this);
                if(dead)
                {
                    target = null;
                    // check in case any other potential targets close by
                    Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, aggroCollider.radius);
                    int i = 0;
                    while (i < hitColliders.Length)
                    {
                        if(hitColliders[i].GetComponent<KnightEnemy>())
                        {
                            target = hitColliders[i].GetComponent<MonoBehaviour>();
                            break;
                        }
                        i++;
                    }
                }
            }
            cooldownRemaining = attackCooldown;
        }
    }

    public void SetHP(float summonHP)
    {
        if (summonHP > 1.0f)
            summonHP = 1.0f;

        health = health * summonHP;

        Vector3 localScale = healthBar.transform.localScale;
        localScale.x = healthBarMaxScale * (health / maxHealth);
        healthBar.transform.localScale = localScale;

        if (health < (maxHealth * 0.33))
        {
            healthBar.color = new Color(255, 0, 0);
        }
        else if (health < (maxHealth * 0.66f))
        {
            healthBar.color = new Color(255, 100, 0);
        }
    }

    public void Boom()
    {
        if (!red)
            return;

        if (bones)
        {
            Instantiate(bones, (Vector2)transform.position, Quaternion.identity);
            bones = null;
        }
        gameObject.SetActive(false);
    }
}
