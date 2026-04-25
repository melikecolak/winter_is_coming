using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AN_PlugScript : MonoBehaviour
{
    [Tooltip("Feature for one using only")]
    public bool OneTime = false;
    [Tooltip("Plug follow this local EmptyObject")]
    public Transform HeroHandsPosition;
    [Tooltip("SocketObject with collider(shpere, box etc.) (is trigger = true)")]
    public Collider Socket; // need Trigger
    public AN_DoorScript DoorObject;

    // NearView()
    float distance;
    float angleView;
    Vector3 direction;
    Transform player;

    bool follow = false, isConnected = false, followFlag = false, youCan = true;
    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        player = GameObject.FindWithTag("Player").transform;
    }

    void Update()
    {
        if (youCan) Interaction();

        // frozen if it is connected to PowerOut
        if (isConnected)
        {
            gameObject.transform.position = Socket.transform.position;
            gameObject.transform.rotation = Socket.transform.rotation;
            DoorObject.isOpened = true;
        }
        else
        {
            DoorObject.isOpened = false;
        }
    }

    void Interaction()
    {
        if (NearView() && Input.GetKeyDown(KeyCode.E) && !follow)
        {
            isConnected = false; // unfrozen
            follow = true;
            followFlag = false;
        }

        if (follow)
        {
            rb.linearDamping = 10f;
            rb.angularDamping = 10f;
            if (followFlag)
            {
                distance = Vector3.Distance(transform.position, player.position);
                if (distance > 3f || Input.GetKeyDown(KeyCode.E))
                {
                    follow = false;
                }
            }

            followFlag = true;
            rb.AddExplosionForce(-1000f, HeroHandsPosition.position, 10f);
        }
        else
        {
            rb.linearDamping = 0f;
            rb.angularDamping = .5f;
        }
    }

    bool NearView()
    {
        distance = Vector3.Distance(transform.position, player.position);
        if (distance < 3f) return true;
        else return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == Socket)
        {
            isConnected = true;
            follow = false;
            DoorObject.rbDoor.AddRelativeTorque(new Vector3(0, 0, 20f));
        }
        if (OneTime) youCan = false;
    }
}