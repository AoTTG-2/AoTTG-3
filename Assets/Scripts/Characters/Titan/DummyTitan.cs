﻿using Assets.Scripts.Characters.Humans;
using Assets.Scripts.Services;
using Assets.Scripts.Services.Interface;
using Assets.Scripts.UI.InGame.HUD;
using UnityEngine;
using System.Collections;

/// <summary>
/// The dummy titan. Hasn't been touched since May 2020, so should be updated to match the new standards
/// </summary>
public class DummyTitan : Photon.MonoBehaviour
{
    protected readonly IEntityService EntityService = Service.Entity;

    public int health = 300;
    public GameObject myHero;
    public Transform pivot;
    public bool dead = false;
    public bool canRotate = true;
    public TextMesh healthLabel;
    public TextMesh healthLabel2;
    public MinimapIcon minimapIcon;

    public AudioSource dummyWiggleNoise;
    private float wiggleSoundTimer = 3f;
    private bool timerCheck = true;

    private bool kneeHit = false;
    private float rotationTimer = 5f;

    public float speed = 3.0f;

    Quaternion lookAtRotation;

    private void Start()
    {
        pivot = transform.Find("BodyPivot");
        healthLabel = pivot.Find("Body/HealthLabel").gameObject.GetComponent<TextMesh>();
        healthLabel2 = pivot.Find("Body/HealthLabel2").gameObject.GetComponent<TextMesh>();
    }

    void Update()
    {
        if (canRotate)
        {
            if (myHero && kneeHit == false)
            {
                Rotation();
                if(timerCheck = true && wiggleSoundTimer <= 0)
                {
                    dummyWiggleNoise.Play();
                    wiggleSoundTimer = 3f;
                    timerCheck = false;
                }
            }
            else if(myHero && kneeHit == true)
            {
                Debug.Log("Timer is running");
                StartCoroutine(KneeHitTimer(rotationTimer));  
            }
            else
            {
                myHero = GetNearestHero();
            }
        }
        healthLabel.text = healthLabel2.text = health.ToString();
    }

     private IEnumerator KneeHitTimer(float rotationTimer)
     {
        yield return new WaitForSeconds(rotationTimer);
        kneeHit = false;
     }

    private GameObject GetNearestHero()
    {
        GameObject obj2 = null;
        float positiveInfinity = float.PositiveInfinity;
        foreach (Hero hero in EntityService.GetAll<Hero>())
        {
            GameObject gameObject = hero.gameObject;
            float num2 = Vector3.Distance(gameObject.transform.position, transform.position);
            if (num2 < positiveInfinity)
            {
                obj2 = gameObject;
                positiveInfinity = num2;
            }
        }
        return obj2;
    }

    [PunRPC]
    public void GetHit(int dmg)
    {
        if(health != 0)
        {
            if(dmg >= health)
            {
                Die();
            }
        }
    }

    public void Die()
    {
        if (!dead)
        {
            Destroy(minimapIcon.gameObject);

            Transform body = pivot.transform.Find("Body");
            body.transform.parent = null;
            Rigidbody rb = body.gameObject.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.mass = 15;
            body.GetComponent<MeshCollider>().convex = true;
            Destroy(body.gameObject, 3);
            Destroy(gameObject, 3);
            dead = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "blades")
        {
            Debug.Log("Knee is hit");
            KneeHit();
        }
    }

    public void KneeHit()
    {
        kneeHit = true;
    }

    private void Rotation()
    {
        if(dead == false)
        {
            lookAtRotation = Quaternion.LookRotation(myHero.transform.position - pivot.position);
            Vector3 desiredRotation = Quaternion.RotateTowards(pivot.rotation, lookAtRotation, speed * Time.deltaTime).eulerAngles;
            pivot.rotation = Quaternion.Euler(0, desiredRotation.y, 0);



            //timerCheck = true;
            //wiggleSoundTimer =- Time.deltaTime; 
            //dummyWiggleNoise.loop = true;
        }
        
    }
}