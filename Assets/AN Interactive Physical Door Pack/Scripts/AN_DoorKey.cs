using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AN_DoorKey : MonoBehaviour
{
    [Tooltip("True - red key object, false - blue key")]
    public bool isRedKey = true;
    AN_HeroInteractive hero;

    // NearView()
    float distance;
    float angleView;
    Vector3 direction;
    Transform player;

    private void Start()
    {
        hero = FindObjectOfType<AN_HeroInteractive>();
        player = GameObject.FindWithTag("Player").transform;
    }

    void Update()
    {
        if ( NearView() && Input.GetKeyDown(KeyCode.E) )
        {
            if (isRedKey) hero.RedKey = true;
            else hero.BlueKey = true;
            Destroy(gameObject);
        }
    }

    bool NearView()
    {
        distance = Vector3.Distance(transform.position, player.position);
        if (distance < 3f) return true;
        else return false;
    }
}