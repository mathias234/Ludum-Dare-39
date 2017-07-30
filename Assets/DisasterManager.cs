using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisasterManager : MonoBehaviour {
    public GameObject warningSurgePrefab;
    public GameObject warningOverheatPrefab;

    GameWorld gameWorld;
    PowerManager powerManager;


    private void Awake() {
        gameWorld = FindObjectOfType<GameWorld>();
        powerManager = FindObjectOfType<PowerManager>();
    }

    // Update is called once per frame
    void Update() {
        int randomDisaster = Random.Range(0, 1000);
        if (randomDisaster == 0) {
            int randomType = Random.Range(0, 2);

            switch (randomType) {
                case 0:
                    Debug.Log("Power Surge");

                    Tile gen = null;
                    foreach (Tile tile in gameWorld.GetTileMap()) {
                        if (tile.type == 1) {
                            gen = tile;
                        }
                    }

                    Instantiate(warningSurgePrefab, gameWorld.TileMapToWorldCoord(gen.x, gen.y) + new Vector3(0, 0, -7), Quaternion.identity);
                    break;
                case 1:
                    foreach (Tile t in gameWorld.GetTileMap()) {
                        if (t.type == 2 || t.type == 3) {
                            int shouldDestroy = Random.Range(0, 5);
                            if (shouldDestroy == 0) {
                                Instantiate(warningOverheatPrefab, gameWorld.TileMapToWorldCoord(t.x, t.y) + new Vector3(0, 0, -7), Quaternion.identity);
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
