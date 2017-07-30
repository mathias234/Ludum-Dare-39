using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Tilemap definitions
/// 0 = Empty
/// 1 = Generator
/// 2 = Power Factory
/// 3 = Power Rails
/// 4 = Red Thing
/// </summary>

public class GameWorld : MonoBehaviour {
    public GameObject generatorPrefab;
    public GameObject powerFactoryPrefab;
    public GameObject railPrefab;
    public GameObject railCornerPrefab;
    public GameObject intersection4Prefab;
    public GameObject intersection3Prefab;

    public GameObject tempPathThing;

    public Vector3 mapOffset;

    private Tile[,] tileMap;

    private List<GameObject> spawnedObjects;

    public int currentType;

    public void SetTileType(int value) {
        currentType = value;
    }

    public void Restart() {
        SceneManager.LoadScene("MainScene");
    }


    private void Start() {
        spawnedObjects = new List<GameObject>();
        tileMap = new Tile[9, 9];

        for (int x = 0; x < 9; x++) {
            for (int y = 0; y < 9; y++) {
                tileMap[x, y] = new Tile(x, y, 0);
            }
        }

        // randomize the spawn position of the core GEN
        int rX = UnityEngine.Random.Range(0, 9);
        int rY = UnityEngine.Random.Range(0, 9);
        tileMap[rX, rY] = new Tile(rX, rY, 1);

        DrawTileMap();
    }

    public Tile[,] GetTileMap() {
        return tileMap;
    }


    public Tile GetTileMapAt(int x, int y) {
        if (x < 0)
            return new Tile(x, y, 0);
        if (x > 8)
            return new Tile(x, y, 0);
        if (y < 0)
            return new Tile(x, y, 0);
        if (y > 8)
            return new Tile(x, y, 0);

        return tileMap[x, y];
    }

    public void SetTileMapAt(int x, int y, int value) {
        if (x < 0)
            return;
        if (x > 8)
            return;
        if (y < 0)
            return;
        if (y > 8)
            return;

        if (tileMap[x, y].type == value)
            return;

        if (tileMap[x, y].type == 1) // you can't remove the generator
            return;


        if (value == 2) {
            if (GetTileMapAt(x + 1, y).type == 2 ||
                GetTileMapAt(x - 1, y).type == 2 ||
                GetTileMapAt(x, y - 1).type == 2 ||
                GetTileMapAt(x, y + 1).type == 2) { // you cant place a power factory next to another
                return;                             // TEMPORARY should be explosions
            }
        }

        tileMap[x, y] = new Tile(x, y, value);
    }

    private Vector3 DivideVector(Vector3 left, Vector3 right) {
        return new Vector3(left.x / right.x, left.y / right.y, left.z / right.z);
    }

    Vector3 GetMousePosInTileMap() {
        var mousePos = Input.mousePosition;

        var mouseInWorld = Camera.main.ScreenToWorldPoint(mousePos);

        var tilePos = DivideVector(mouseInWorld, new Vector3(1.28f, 1.28f, 1.28f));
        var iTilePos = new Vector3(
            ((int)Math.Floor(tilePos.x + 0.64f) + 4),
            ((int)Math.Floor(tilePos.y + 0.64f) + 4),
            (int)Math.Floor(tilePos.z));

        //Debug.Log(iTilePos);

        return iTilePos;
    }

    private void Update() {
        if (Input.GetMouseButtonDown(0)) {
            var iTilePos = GetMousePosInTileMap();

            SetTileMapAt((int)iTilePos.x, (int)iTilePos.y, currentType);

            DrawTileMap();
        }

        if (Input.GetKeyDown(KeyCode.E)) {
            Vector2 start = GetMousePosInTileMap();


            var path = GetPathFrom(start);

            foreach (var tile in path) {
                SetTileMapAt(tile.x, tile.y, 4);
            }

            DrawTileMap();
        }


        if (Input.GetKeyDown(KeyCode.R)) {
            DrawTileMap();
        }
    }

    int MinimumDistance(int[] distance, bool[] shortestPathTreeSet, int count) {
        int min = int.MaxValue;
        int minIndex = 0;

        for (int v = 0; v < count; v++) {
            if (shortestPathTreeSet[v] == false && distance[v] <= min) {
                min = distance[v];
                minIndex = v;
            }
        }

        return minIndex;
    }

    public List<Tile> GetPathFrom(Vector2 start) {
        Dictionary<Tile, float> distance = new Dictionary<Tile, float>();
        Dictionary<Tile, Tile> previous = new Dictionary<Tile, Tile>();

        List<Tile> unvisited = new List<Tile>();

        Tile source = GetTileMapAt((int)start.x, (int)start.y);

        Vector2 end = new Vector2();

        foreach (var mapTile in tileMap) {
            if (mapTile.type == 1)
                end = new Vector2(mapTile.x, mapTile.y);
        }

        Tile target = GetTileMapAt((int)end.x, (int)end.y);


        distance[source] = 0;
        previous[source] = null;

        foreach (Tile v in tileMap) {
            if (v != source) {
                distance[v] = Mathf.Infinity;
                previous[v] = null;
            }

            unvisited.Add(v);
        }

        while (unvisited.Count > 0) {
            Tile u = null;

            foreach (Tile possibleU in unvisited) {
                if (u == null || distance[possibleU] < distance[u]) {
                    u = possibleU;
                }
            }

            if (u == target) {
                break; // we have arrived
            }

            unvisited.Remove(u);

            foreach (Tile v in GetNeighboursFor(u.x, u.y)) {
                float alt = distance[u] + CostToEnterTile(u.x, u.y, v.x, v.y);
                if (alt < distance[v]) {
                    distance[v] = alt;
                    previous[v] = u;
                }
            }
        }

        if (previous[target] == null)
            return new List<Tile>(); // no route between target and source

        List<Tile> currentPath = new List<Tile>();
        Tile current = target;

        while (current != null) {
            currentPath.Add(current);
            current = previous[current];
        }

        currentPath.Reverse();

        return currentPath;
    }

    public float CostToEnterTile(int sourceX, int sourceY, int targetX, int targetY) {
        int tileType = tileMap[targetX, targetY].type;

        if (tileType == 3 || tileType == 1 || tileType == 4) {
            return 0.5f;
        }

        return Mathf.Infinity;
    }

    List<Tile> GetNeighboursFor(int x, int y) {
        List<Tile> neighbours = new List<Tile>();

        if (GetTileMapAt(x + 1, y).type != 0)
            neighbours.Add(GetTileMapAt(x + 1, y));
        if (GetTileMapAt(x - 1, y).type != 0)
            neighbours.Add(GetTileMapAt(x - 1, y));
        if (GetTileMapAt(x, y + 1).type != 0)
            neighbours.Add(GetTileMapAt(x, y + 1));
        if (GetTileMapAt(x, y - 1).type != 0)
            neighbours.Add(GetTileMapAt(x, y - 1));

        return neighbours;
    }

    public Vector3 TileMapToWorldCoord(int x, int y) {
        return new Vector3(x * 1.28f, y * 1.28f, 0) + mapOffset;
    }

    public Vector2 WorldToTileCoords(Vector3 pos) {
        var offsetPos = pos - mapOffset;

        return new Vector2(offsetPos.x / 1.28f, offsetPos.y / 1.28f);
    }


    public void DrawTileMap() {
        foreach (var powerCell in FindObjectsOfType<PowerCellAI>()) {
            powerCell.RecalculatePath();
        }


        foreach (var spawnedObject in spawnedObjects) {
            Destroy(spawnedObject);
        }

        // Draw new tilemap
        for (int x = 0; x < 9; x++) {
            for (int y = 0; y < 9; y++) {
                switch (tileMap[x, y].type) {
                    case 0:
                        break; // empty map

                    case 1: // generator
                        var mapTile = Instantiate(generatorPrefab, new Vector3(x * 1.28f, y * 1.28f, -6) + mapOffset, Quaternion.identity);
                        mapTile.transform.SetParent(transform);
                        spawnedObjects.Add(mapTile);
                        break;

                    case 2: // power factory
                        mapTile = Instantiate(powerFactoryPrefab, new Vector3(x * 1.28f, y * 1.28f, 0) + mapOffset, Quaternion.identity);
                        mapTile.transform.SetParent(transform);
                        spawnedObjects.Add(mapTile);
                        break;

                    case 3: // rails
                        if (GetTileMapAt(x + 1, y).type != 0 && GetTileMapAt(x - 1, y).type != 0
                            && GetTileMapAt(x, y + 1).type != 0 && GetTileMapAt(x, y - 1).type != 0) {
                            mapTile = Instantiate(intersection4Prefab, new Vector3(x * 1.28f, y * 1.28f, 0) + mapOffset, Quaternion.identity);
                            mapTile.transform.SetParent(transform);
                            spawnedObjects.Add(mapTile);

                            continue;
                        }

                        if (GetTileMapAt(x, y + 1).type != 0 && GetTileMapAt(x, y - 1).type != 0
                            && GetTileMapAt(x - 1, y).type != 0) {
                            mapTile = Instantiate(intersection3Prefab, new Vector3(x * 1.28f, y * 1.28f, 0) + mapOffset, Quaternion.identity);
                            mapTile.transform.SetParent(transform);
                            mapTile.transform.rotation = Quaternion.Euler(new Vector3(0, 0, -90));
                            spawnedObjects.Add(mapTile);

                            continue;
                        }

                        if (GetTileMapAt(x, y + 1).type != 0 && GetTileMapAt(x, y - 1).type != 0
                            && GetTileMapAt(x + 1, y).type != 0) {
                            mapTile = Instantiate(intersection3Prefab, new Vector3(x * 1.28f, y * 1.28f, 0) + mapOffset, Quaternion.identity);
                            mapTile.transform.SetParent(transform);
                            mapTile.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 90));
                            spawnedObjects.Add(mapTile);

                            continue;
                        }

                        if (GetTileMapAt(x + 1, y).type != 0 && GetTileMapAt(x - 1, y).type != 0
                            && GetTileMapAt(x, y - 1).type != 0) {
                            mapTile = Instantiate(intersection3Prefab, new Vector3(x * 1.28f, y * 1.28f, 0) + mapOffset, Quaternion.identity);
                            mapTile.transform.SetParent(transform);
                            spawnedObjects.Add(mapTile);
                            continue;
                        }


                        if (GetTileMapAt(x + 1, y).type != 0 && GetTileMapAt(x - 1, y).type != 0
                            && GetTileMapAt(x, y + 1).type != 0) {
                            mapTile = Instantiate(intersection3Prefab, new Vector3(x * 1.28f, y * 1.28f, 0) + mapOffset, Quaternion.identity);
                            mapTile.transform.SetParent(transform);
                            mapTile.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 180));
                            spawnedObjects.Add(mapTile);
                            continue;
                        }

                        if (GetTileMapAt(x, y - 1).type != 0 && GetTileMapAt(x - 1, y).type != 0) {
                            mapTile = Instantiate(railCornerPrefab, new Vector3(x * 1.28f, y * 1.28f, 0) + mapOffset, Quaternion.identity);
                            mapTile.transform.SetParent(transform);
                            spawnedObjects.Add(mapTile);

                            continue;
                        }

                        if (GetTileMapAt(x, y + 1).type != 0 && GetTileMapAt(x - 1, y).type != 0) {
                            mapTile = Instantiate(railCornerPrefab, new Vector3(x * 1.28f, y * 1.28f, 0) + mapOffset, Quaternion.identity);
                            mapTile.transform.SetParent(transform);
                            mapTile.transform.rotation = Quaternion.Euler(new Vector3(0, 0, -90));
                            spawnedObjects.Add(mapTile);

                            continue;
                        }


                        if (GetTileMapAt(x, y - 1).type != 0 && GetTileMapAt(x + 1, y).type != 0) {
                            mapTile = Instantiate(railCornerPrefab, new Vector3(x * 1.28f, y * 1.28f, 0) + mapOffset, Quaternion.identity);
                            mapTile.transform.SetParent(transform);
                            mapTile.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 90));
                            spawnedObjects.Add(mapTile);

                            continue;
                        }


                        if (GetTileMapAt(x, y + 1).type != 0 && GetTileMapAt(x + 1, y).type != 0) {
                            mapTile = Instantiate(railCornerPrefab, new Vector3(x * 1.28f, y * 1.28f, 0) + mapOffset, Quaternion.identity);
                            mapTile.transform.SetParent(transform);
                            mapTile.transform.rotation = Quaternion.Euler(new Vector3(0, 0, -180));
                            spawnedObjects.Add(mapTile);
                            continue;
                        }


                        mapTile = Instantiate(railPrefab, new Vector3(x * 1.28f, y * 1.28f, 0) + mapOffset, Quaternion.identity);
                        mapTile.transform.SetParent(transform);

                        if (GetTileMapAt(x, y + 1).type != 0 || GetTileMapAt(x, y - 1).type != 0) {
                            mapTile.transform.rotation = Quaternion.Euler(new Vector3(0, 0, -90));
                        }

                        spawnedObjects.Add(mapTile);
                        break;

                    case 4:
                        mapTile = Instantiate(tempPathThing, new Vector3(x * 1.28f, y * 1.28f, 0) + mapOffset, Quaternion.identity);
                        mapTile.transform.SetParent(transform);
                        spawnedObjects.Add(mapTile);
                        break;
                    default:
                        break;
                }

            }
        }
    }
}
