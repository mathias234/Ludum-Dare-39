using System.Collections.Generic;
using UnityEngine;

public class PowerCellAI : MonoBehaviour {
    private List<Tile> path;
    private int lastPathPos = 1;
    private GameWorld gameWorld;
    private float timeSinceLastUpdate = 0;
    private Vector3 currentGoal;

    private bool isBad;

    private void Awake() {
        gameWorld = FindObjectOfType<GameWorld>();
    }

    public void SetBad(bool value) {
        isBad = value;
    }

    public void SetPath(List<Tile> path) {
        currentGoal = gameWorld.TileMapToWorldCoord(path[1].x, path[1].y);

        this.path = path;

        if (path[lastPathPos - 1].y > path[lastPathPos].y || path[lastPathPos - 1].y < path[lastPathPos].y) {
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
        }
        if (path[lastPathPos - 1].x > path[lastPathPos].x || path[lastPathPos - 1].x < path[lastPathPos].x) {
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, -90));
        }
    }

    public void RecalculatePath() {
        if (path == null) {
            path = gameWorld.GetPathFrom(gameWorld.WorldToTileCoords(transform.position));
            lastPathPos = 0;

            if (path.Count <= 0)
                path = null;

            return;
        }

        var newPath = gameWorld.GetPathFrom(new Vector2(path[lastPathPos].x, path[lastPathPos].y));
        if (newPath.Count <= 0) {
            newPath = null;
            path = null;
            return;
        }

        if (newPath != path) {
            path = newPath;
            lastPathPos = 0;
        }
    }


    private void Update() {
        transform.position = Vector3.Lerp(transform.position, currentGoal, Time.deltaTime * 3f);
        transform.position = new Vector3(transform.position.x, transform.position.y, -5);



        timeSinceLastUpdate += Time.deltaTime;

        if (timeSinceLastUpdate >= 1) {
            timeSinceLastUpdate = 0;

            if (path == null)
                return;

            var currentPathPos = lastPathPos + 1;

            if (currentPathPos == path.Count) {
                if (isBad == true)
                    FindObjectOfType<PowerManager>().DecreasePower(1);
                else
                    FindObjectOfType<PowerManager>().IncreasePower(3);
                Destroy(gameObject);
                return;
            }

            currentGoal = gameWorld.TileMapToWorldCoord(path[currentPathPos].x, path[currentPathPos].y);

            lastPathPos = currentPathPos;

            if (path[lastPathPos - 1].y > path[lastPathPos].y || path[lastPathPos - 1].y < path[lastPathPos].y) {
                transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
            }
            if (path[lastPathPos - 1].x > path[lastPathPos].x || path[lastPathPos - 1].x < path[lastPathPos].x) {
                transform.rotation = Quaternion.Euler(new Vector3(0, 0, -90));
            }
        }
    }
}
