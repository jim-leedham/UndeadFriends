using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;

    public GameObject player;
    public List<KnightFriendly> knightFriendlies;

    public float musicFadeUpSpeed = 0.2f;
    private AudioSource music;

    public WallDestruction wallDestruction;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);

        music = GetComponent<AudioSource>();
        music.volume = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if(music.volume < 0.6f)
        {
            music.volume += musicFadeUpSpeed * Time.deltaTime;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 target = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // is it an enemy?
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(target, 2.0f);
            int i = 0;
            while (i < hitColliders.Length)
            {
                KnightEnemy knight = hitColliders[i].GetComponent<KnightEnemy>();
                if (knight != null)
                {
                    // send the horde!
                    for (int j = 0; j < knightFriendlies.Count; j++)
                    {
                        knightFriendlies[j].GoToTarget(knight);
                    }
                    return;
                }
                i++;
            }

            // send the horde!
            for (int j = 0; j < knightFriendlies.Count; j++)
            {
                knightFriendlies[j].GoToPosition(target);
            }
        }
    }

    internal void OnSummonPerformed(Vector3 position, float radius, float summonHP)
    {
        // find all tombstones within radius!
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(position, radius);
        int i = 0;
        while (i < hitColliders.Length)
        {
            Tombstone tomb = hitColliders[i].GetComponent<Tombstone>();
            if(tomb != null)
            {
                tomb.OnRessurect(summonHP);
            }
            else
            {
                Bones bones = hitColliders[i].GetComponent<Bones>();
                if(bones)
                {
                    bones.OnResurrect(summonHP);
                }
            }


            i++;
        }
    }

    public Vector2 GetPlayerPosition()
    {
        return player.transform.position;
    }

    public int GetNumFriendlyKnights()
    {
        return knightFriendlies.Count;
    }

    public void KillAllFriendlies()
    {
        for (int j = 0; j < knightFriendlies.Count; j++)
        {
            knightFriendlies[j].gameObject.SetActive(false);
        }
    }

    public void Boomer()
    {
        for (int j = 0; j < knightFriendlies.Count; j++)
        {
            knightFriendlies[j].Boom();
        }
        wallDestruction.OnDestroy();
    }

    //public Vector2 GetClosestFriendlyKnight(Vector2 pos)
    //{
    //    if (knightFriendlies.Count == 0)
    //        return new List<Vector2>();
    //
    //    Vector2 closest = new Vector2(0.0f, 0.0f);
    //    for(int i = 0; i < knightFriendlies.Count; i++)
    //    {
    //
    //    }
    //}
}
