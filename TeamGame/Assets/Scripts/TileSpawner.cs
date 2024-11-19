using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TempleRun {

public class TileSpawner : MonoBehaviour
{
    [SerializeField] private GameObject startingTile;
    [SerializeField] private List<GameObject> turnTiles;
    [SerializeField] private List<GameObject> obstacles;
    [SerializeField] private int tileStartCount = 10;
    [SerializeField] private int minimumStraightTiles = 3;
    [SerializeField] private int maximumStraightTiles = 15;

    private Vector3 currentTileLocation = Vector3.zero;
    private Vector3 currentTileDirection = Vector3.forward;
    private GameObject prevTile;

    private List<GameObject> currentTiles;
    private List<GameObject> currentObstacles;

    private void Start() {
        currentTiles = new List<GameObject>();
        currentObstacles = new List<GameObject>();

        // In order for tiles to be chosen at random, use this function in order to choose random starting number
        Random.InitState(System.DateTime.Now.Millisecond);

        // Spawns the first straight tiles of the game
        for (int i = 0; i < tileStartCount; i++) {
            SpawnTile(startingTile.GetComponent<Tile>());
        }

        // Spawns a randomly selected turn object
        SpawnTile(SelectRandomGameObjectFromList(turnTiles).GetComponent<Tile>());
    }

    // Spawns tile at location in direction currently facing towards
    private void SpawnTile(Tile tile, bool spawnObstacle = false) {
        // Rotate tile based on direction of current tile
        // Ensures that turn tiles are not overlapping with the straight tiles
        Quaternion newTileRotation = tile.gameObject.transform.rotation * Quaternion.LookRotation(currentTileDirection, Vector3.up);

        prevTile = GameObject.Instantiate(tile.gameObject, currentTileLocation, newTileRotation);
        currentTiles.Add(prevTile);

        if (spawnObstacle) SpawnObstacle();

        if (tile.type == TileType.STRAIGHT) {
            currentTileLocation += Vector3.Scale(prevTile.GetComponent<Renderer>().bounds.size, currentTileDirection);
        }

    }

    // Spawn new tiles after the player turns
    public void AddNewDirection(Vector3 direction) {
        currentTileDirection = direction;
        DeletePreviousTiles();

        // Determine what the previous turn tile was
        Vector3 tilePlacementScale;
        if (prevTile.GetComponent<Tile>().type == TileType.SIDEWAYS) {
            tilePlacementScale = Vector3.Scale(prevTile.GetComponent<Renderer>().bounds.size / 2 + (Vector3.one * startingTile.GetComponent<BoxCollider>().size.z / 2), 
                currentTileDirection);
        }
        else {
            tilePlacementScale = Vector3.Scale((prevTile.GetComponent<Renderer>().bounds.size - (Vector3.one * 2)) + (Vector3.one * startingTile.GetComponent<BoxCollider>().size.z / 2), 
                currentTileDirection);
        }

        currentTileLocation += tilePlacementScale;

        int currentPathLength = Random.Range(minimumStraightTiles, maximumStraightTiles);
        for (int i = 0; i < currentPathLength; i++) {
            SpawnTile(startingTile.GetComponent<Tile>(), (i==0) ? false : true);
        }

        SpawnTile(SelectRandomGameObjectFromList(turnTiles).GetComponent<Tile>(), false);

    }

    // delete tiles once the player turns in order to save space and ensure game doesn't loop back to previous tiles
    private void DeletePreviousTiles() {
        // Removes all but the turn tile
        while (currentTiles.Count != 1) {
            GameObject tile = currentTiles[0];
            currentTiles.RemoveAt(0);
            Destroy(tile);
        }

        // Removes all obstacles
        while (currentObstacles.Count != 0) {
            GameObject obstacle = currentObstacles[0];
            currentObstacles.RemoveAt(0);
            Destroy(obstacle);
        }
    }


    // If the spawn obstacle is true in SpawnTile function, spawns an obstacle 20% of the time
    private void SpawnObstacle() {
        if (Random.value > .4f) return;

        GameObject obstaclePrefab = SelectRandomGameObjectFromList(obstacles);
        Quaternion newObjectRotation = obstaclePrefab.gameObject.transform.rotation * Quaternion.LookRotation(currentTileDirection, Vector3.up);

        GameObject obstacle = Instantiate(obstaclePrefab, currentTileLocation, newObjectRotation);
        currentObstacles.Add(obstacle);
    }

    // Returns random tile; Returns null if there are no objects in list
    private GameObject SelectRandomGameObjectFromList(List<GameObject> list) {
        if (list.Count == 0) return null;

        return list[Random.Range(0, list.Count)];
    }
    
}
}