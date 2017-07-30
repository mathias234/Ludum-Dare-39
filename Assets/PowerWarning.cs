using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerWarning : MonoBehaviour {
    float timeToDestroy = 5;
    PowerManager powerManager;
    public GameObject explosionAudio;

    private void Awake() {
        powerManager = FindObjectOfType<PowerManager>();
    }

    private void Update () {
        timeToDestroy -= Time.deltaTime;
        if (timeToDestroy <= 0) {
            powerManager.DecreasePower(10);
            Instantiate(explosionAudio);
            Destroy(gameObject);
        }
    }
}
