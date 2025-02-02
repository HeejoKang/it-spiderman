﻿using UnityEngine;
using System.Collections;

public class EnemyTriggerDamage : MonoBehaviour {

    public int Damage = 1;

    public bool Heavy;

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Player")
        {
            if (Heavy)
            {
                col.gameObject.GetComponent<PlayerMachine>().HeavyDamage(Damage, transform.position);
            }
            else
            {
                col.gameObject.GetComponent<PlayerMachine>().GroundDamageLight(Damage, transform.position);
            }

            SendMessage("PlayerTookDamage", SendMessageOptions.DontRequireReceiver);
        }
    }
}
