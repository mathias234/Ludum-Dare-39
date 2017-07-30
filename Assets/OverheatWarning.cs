using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverheatWarning : MonoBehaviour {
    float timeToDestroy = 5;
    GameWorld gameWorld;
    public GameObject explosionAudio;

    private void Awake() {
        gameWorld = FindObjectOfType<GameWorld>();
    }

    private void Update() {
        timeToDestroy -= Time.deltaTime;
        if (timeToDestroy <= 0) {

            var tilePos = gameWorld.WorldToTileCoords(transform.position);

            gameWorld.SetTileMapAt((int)tilePos.x, (int)tilePos.y, 0);

            gameWorld.DrawTileMap();

            Instantiate(explosionAudio);

            Destroy(gameObject);
        }
    }
}
