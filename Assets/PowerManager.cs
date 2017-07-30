using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerManager : MonoBehaviour {
    public Scrollbar powerLeftBar;
    public Image powerColor;
    public float powerLeft = 100;
    public float powerUsage = 0;
    public float powerGeneration = 0;
    public int powerGenTimes = 0;
    public Text powerUsageText;
    public Text powerGenerationText;

    public GameObject GameOverScene;
    public GameObject GameManagerObject;

    public void IncreasePower(int value) {
        if (powerLeft <= 100)
            powerLeft += value;

        powerGeneration += value;
        powerGenTimes += 1;
    }

    public void DecreasePower(int value) {
        powerLeft -= value;
    }

    float timer = 1;

    // Update is called once per frame
    void Update() {

        powerLeft -= Time.deltaTime * powerUsage;

        powerUsage += Time.deltaTime / 20;


        if (powerLeft <= 0) {
            powerLeftBar.enabled = false;
            // GAME OVER!
            GameOverScene.SetActive(true);
            powerLeft = 0;
        }
        powerLeftBar.size = powerLeft / 100.0f;
        powerUsageText.text = "GW/h: " + powerUsage.ToString("N2");


        timer += Time.deltaTime;

        if (timer >= 1) {
            if (powerGenTimes == 0)
                powerGenerationText.text = "GW/h: 0.00";
            else
                powerGenerationText.text = "GW/h: " + (powerGeneration).ToString("N2");

            powerGeneration = 0;
            timer = 0;
        }
        if (powerLeft >= 50) {
            powerColor.color = new Color(0, 1, 0);
        }
        if (powerLeft <= 50) {
            powerColor.color = new Color(1, 195.0f / 255.0f, 0);
        }
        if (powerLeft <= 20) {
            powerColor.color = new Color(1, 0, 0);
        }
    }
}
