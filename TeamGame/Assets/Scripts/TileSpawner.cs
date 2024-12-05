using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // For UI elements
using TMPro;

namespace TempleRun
{
    public class TileSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject startingTile;
        [SerializeField] private List<GameObject> turnTiles;
        [SerializeField] private List<GameObject> obstacles;
        [SerializeField] private GameObject tileAssociatedPrefab; // Prefab for the rotunda
        [SerializeField] private GameObject secondaryPrefab; // Prefab for the dorm
        //[SerializeField] private GameObject arenaPrefab;
        [SerializeField] private TextMeshProUGUI turnAnnouncementText; // Reference to the UI Text for announcements
        [SerializeField] private float announcementDuration = 2f; // Duration to show the announcement
        [SerializeField] private int tileStartCount = 10;
        [SerializeField] private int minimumStraightTiles = 3;
        [SerializeField] private int maximumStraightTiles = 15;

        [SerializeField] private float minimumObstacleDistance = 15f;
        [SerializeField] private float startingObstacleProbability = 0.3f;
        [SerializeField] private float maxObstacleProbability = 0.6f;
        [SerializeField] private int tilesToMaxProbability = 50;

        private Vector3 lastObstacleLocation = Vector3.negativeInfinity;
        private Vector3 currentTileLocation = Vector3.zero;
        private Vector3 currentTileDirection = Vector3.forward;
        private GameObject prevTile;

        private List<GameObject> currentTiles;
        private List<GameObject> currentObstacles;
        private List<GameObject> currentTileObjects; // List to track associated game objects

        private int tilesSpawned = 0;

        private void Start()
        {
            currentTiles = new List<GameObject>();
            currentObstacles = new List<GameObject>();
            currentTileObjects = new List<GameObject>();

            Random.InitState(System.DateTime.Now.Millisecond);

            for (int i = 0; i < tileStartCount; i++)
            {
                SpawnTile(startingTile.GetComponent<Tile>());
            }

            SpawnTile(SelectRandomGameObjectFromList(turnTiles).GetComponent<Tile>());
        }

        private void SpawnTile(Tile tile, bool spawnObstacle = false)
        {
            Quaternion newTileRotation = tile.gameObject.transform.rotation * Quaternion.LookRotation(currentTileDirection, Vector3.up);

            prevTile = Instantiate(tile.gameObject, currentTileLocation, newTileRotation);
            currentTiles.Add(prevTile);

            // Spawn associated game objects (rotunda and dorm) without overlapping
            if (tilesSpawned >= 10 && (tilesSpawned - 10) % 20 == 0)
            {
                Vector3 spawnOffsetRotunda = -16 * Vector3.Cross(currentTileDirection, Vector3.up).normalized; // Offset to the left
                Vector3 spawnOffsetDorm = 17 * Vector3.Cross(currentTileDirection, Vector3.up).normalized; // Offset to the right
                
                if (tileAssociatedPrefab != null)
                {
                    GameObject rotunda = Instantiate(tileAssociatedPrefab, currentTileLocation + spawnOffsetRotunda, newTileRotation);
                    tileAssociatedPrefab.SetActive(true);
                    rotunda.transform.parent = prevTile.transform; // Optional: Parent it to the tile for organization
                    currentTileObjects.Add(rotunda);
                }

                if (secondaryPrefab != null)
                {
                    Quaternion dormRotation = Quaternion.LookRotation(-currentTileDirection, Vector3.up) * Quaternion.Euler(0, 70, 0); // Rotate by 45 degrees
                    GameObject dorm = Instantiate(secondaryPrefab, currentTileLocation + spawnOffsetDorm, dormRotation);
                    secondaryPrefab.SetActive(true);
                    dorm.transform.parent = prevTile.transform; // Optional: Parent it to the tile for organization
                    currentTileObjects.Add(dorm);
                    
                }

            }
/*
            if (tilesSpawned >= 15 && (tilesSpawned - 15) % 30 == 0) {
                Vector3 spawnOffsetArena = 17 * Vector3.Cross(currentTileDirection, Vector3.up).normalized; // Offset to the right
                if (arenaPrefab != null)
                {
                    Quaternion arenaRotation = Quaternion.LookRotation(-currentTileDirection, Vector3.up) * Quaternion.Euler(0, 70, 0); // Rotate by 45 degrees
                    GameObject arena = Instantiate(arenaPrefab, currentTileLocation + spawnOffsetArena, arenaRotation);
                    arenaPrefab.SetActive(true);
                    arena.transform.parent = prevTile.transform; // Optional: Parent it to the tile for organization
                    currentTileObjects.Add(arena);
                }
                else
{
    Debug.LogWarning("Arena prefab is null.");
} 
            }*/

            if (tile.type == TileType.LEFT || tile.type == TileType.RIGHT)
            {
                float dynamicDelay = Mathf.Clamp(tilesSpawned * 0.4f, 0f, 4f);
                string turnDirection = tile.type == TileType.LEFT ? "Turn Left" : "Turn Right";
                StartCoroutine(ShowTurnAnnouncement(turnDirection, dynamicDelay));
            }

            if (spawnObstacle)
            {
                float obstacleProbability = Mathf.Lerp(startingObstacleProbability, maxObstacleProbability, (float)tilesSpawned / tilesToMaxProbability);
                if (Random.value <= obstacleProbability)
                {
                    SpawnObstacle();
                }
            }

            if (tile.type == TileType.STRAIGHT)
            {
                currentTileLocation += Vector3.Scale(prevTile.GetComponent<Renderer>().bounds.size, currentTileDirection);
            }

            tilesSpawned++;
        }

        private IEnumerator ShowTurnAnnouncement(string message, float dynamicDelay)
        {
            if (dynamicDelay > 0f)
            {
                yield return new WaitForSeconds(dynamicDelay); // Wait for the specified delay
            }

            Debug.Log($"Showing announcement: {message}");
            turnAnnouncementText.text = message;        // Set the message text
            turnAnnouncementText.gameObject.SetActive(true); // Show the message
            yield return new WaitForSeconds(announcementDuration); // Wait for the duration
            turnAnnouncementText.gameObject.SetActive(false); // Hide the message
        }

        public void AddNewDirection(Vector3 direction)
        {
            currentTileDirection = direction;
            DeletePreviousTiles();

            currentTileLocation += Vector3.Scale(prevTile.GetComponent<Renderer>().bounds.size, currentTileDirection);

            int currentPathLength = Random.Range(minimumStraightTiles, maximumStraightTiles);
            for (int i = 0; i < currentPathLength; i++)
            {
                SpawnTile(startingTile.GetComponent<Tile>(), i > 0);
            }

            SpawnTile(SelectRandomGameObjectFromList(turnTiles).GetComponent<Tile>(), false);
        }

        private void DeletePreviousTiles()
        {
            while (currentTiles.Count > 1)
            {
                GameObject tile = currentTiles[0];
                currentTiles.RemoveAt(0);
                Destroy(tile);
            }

            while (currentTileObjects.Count > 0)
            {
                GameObject tileObject = currentTileObjects[0];
                currentTileObjects.RemoveAt(0);
                Destroy(tileObject);
            }

            while (currentObstacles.Count > 0)
            {
                GameObject obstacle = currentObstacles[0];
                currentObstacles.RemoveAt(0);
                Destroy(obstacle);
            }
        }

        private void SpawnObstacle()
        {
            if (Vector3.Distance(currentTileLocation, lastObstacleLocation) < minimumObstacleDistance) return;

            GameObject obstaclePrefab = SelectRandomGameObjectFromList(obstacles);
            Quaternion newObjectRotation = obstaclePrefab.gameObject.transform.rotation * Quaternion.LookRotation(currentTileDirection, Vector3.up);

            GameObject obstacle = Instantiate(obstaclePrefab, currentTileLocation, newObjectRotation);
            currentObstacles.Add(obstacle);

            lastObstacleLocation = currentTileLocation;
        }

        private GameObject SelectRandomGameObjectFromList(List<GameObject> list)
        {
            if (list.Count == 0) return null;
            return list[Random.Range(0, list.Count)];
        }
    }
}