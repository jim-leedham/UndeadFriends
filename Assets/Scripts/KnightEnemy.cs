using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnightEnemy : MonoBehaviour
{
    public float moveSpeed = 1.0f;
    public float attackingDistance = 3.0f;
    public float aggroRadius = 5.0f;

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Rigidbody2D rigidbody2D;
    private MonoBehaviour target;
    public CircleCollider2D aggroCollider;

    public float attackCooldown = 2.0f;
    private float cooldownRemaining = -1.0f;

    public float damage = 1.0f;
    public float health = 3.0f;
    private float maxHealth;

    public float healthBarMaxScale = 5.5f;
    private SpriteRenderer healthBar;

    public GameObject bones;

    private AudioSource audioSource;
    public List<AudioClip> blips;

    public bool red = false;

    void Awake()
    {
         animator = GetComponent<Animator>();
         spriteRenderer = GetComponent<SpriteRenderer>();
         rigidbody2D = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();


        maxHealth = health;

        Transform healthBarTransform = transform.Find("HealthBar");
        healthBar = healthBarTransform.GetComponent<SpriteRenderer>();
    }

    // when the GameObjects collider arrange for this GameObject to travel to the left of the screen
    void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.GetComponent<KnightFriendly>() && !col.GetComponent<Player>())
            return;

        if(target == null)
            target = col.GetComponent<MonoBehaviour>();
    }

    public void FixedUpdate()
    {
        spriteRenderer.sortingOrder = Mathf.RoundToInt(transform.position.y * 100f) * -1;
        healthBar.sortingOrder = spriteRenderer.sortingOrder;
        if (cooldownRemaining > 0.0f)
        {
            cooldownRemaining -= Time.deltaTime;
        }

        if (target != null)
        {
            // path find to the player!
            Vector2 targetPos = target.transform.position;
            Vector2 toPlayer = targetPos - (Vector2)transform.position;

            if(toPlayer.magnitude > aggroCollider.radius * 2.0f)
            {
                // target escaped our radius!
                target = null;
                animator.SetBool("Moving", false);

                // check in case any other potential targets close by
                Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, aggroCollider.radius);
                int i = 0;
                while (i < hitColliders.Length)
                {
                    if (hitColliders[i].GetComponent<KnightFriendly>() || hitColliders[i].GetComponent<Player>())
                    {
                        target = hitColliders[i].GetComponent<MonoBehaviour>();
                        break;
                    }
                    i++;
                }
                return;
            }

            if (toPlayer.magnitude < attackingDistance)
            {
                // Already very close! Do nothing
                rigidbody2D.velocity = new Vector2(0.0f, 0.0f);
                animator.SetBool("Moving", false);
                Attack();
                return;
            }
            // else, path to player
            // super simply line-of-sight based

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
                if (toPlayer.normalized.y > 0.0f)
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

    private void Attack()
    {
        if (cooldownRemaining < 0.0f)
        {
            animator.SetTrigger("OnAttack");
            KnightFriendly knight = target.GetComponent<KnightFriendly>();
            bool dead = false;
            audioSource.clip = blips[Random.Range(0, blips.Count - 1)];
            audioSource.Play();
            if (knight != null)
            {
                dead = knight.TakeDamage(damage, this);

            }
            else
            {
                Player player = target.GetComponent<Player>();
                dead = player.TakeDamage(damage);
            }
            if (dead)
            {
                target = null;
                // check in case any other potential targets close by
                Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, aggroCollider.radius);
                int i = 0;
                while (i < hitColliders.Length)
                {
                    if (hitColliders[i].GetComponent<KnightFriendly>() || hitColliders[i].GetComponent<Player>())
                    {
                        target = hitColliders[i].GetComponent<MonoBehaviour>();
                        break;
                    }
                    i++;
                }
            }
            cooldownRemaining = attackCooldown;
        }
    }

    public bool TakeDamage(float dmg, MonoBehaviour aggressor)
    {
        health -= dmg;
        target = aggressor;
        if(health <= 0.0f)
        {
            if (bones)
            {
                Instantiate(bones, (Vector2)transform.position, Quaternion.identity);
                bones = null;
            }
            if(red)
            {
                GameManager.instance.KillAllFriendlies();
            }
            gameObject.SetActive(false);
        }
        else
        {
            Vector3 localScale = healthBar.transform.localScale;
            localScale.x = healthBarMaxScale * ((float)health / (float)maxHealth);
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



}
