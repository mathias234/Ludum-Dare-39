using UnityEngine;

public class PowerStation : MonoBehaviour {
    public GameObject powerCellPrefab;
    public GameObject badPowerCellPrefab;

    private GameWorld gameWorld;

    private void Awake() {
        gameWorld = FindObjectOfType<GameWorld>();
    }

    public void Update() {
        var pos = gameWorld.WorldToTileCoords(transform.position);
        Tile tile = gameWorld.GetTileMapAt((int)pos.x, (int)pos.y);

        tile.timeSinceLastUpdate += Time.deltaTime;

        // update this stuff ever 5 secounds
        if (tile.timeSinceLastUpdate >= 5) {


            // create power! and then send it to the generator
            var path = gameWorld.GetPathFrom(new Vector2(tile.x, tile.y));
            if (path.Count > 0) {
                int badCell = Random.Range(0, 100); // one in every 100 cell will be bad 

                GameObject powerCell = null;

                if (badCell == 0) {
                    powerCell = Instantiate(badPowerCellPrefab, gameWorld.TileMapToWorldCoord(path[1].x, path[1].y) + new Vector3(0, 0, -5), Quaternion.identity);
                    powerCell.AddComponent<PowerCellAI>();
                    powerCell.GetComponent<PowerCellAI>().SetBad(true);
                }
                else {
                    powerCell = Instantiate(powerCellPrefab, gameWorld.TileMapToWorldCoord(path[1].x, path[1].y) + new Vector3(0, 0, -5), Quaternion.identity);
                    powerCell.AddComponent<PowerCellAI>();
                    powerCell.GetComponent<PowerCellAI>().SetBad(false);
                }

                powerCell.GetComponent<PowerCellAI>().SetPath(path);
            }

            tile.timeSinceLastUpdate = 0;
        }
    }
}
