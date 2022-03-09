using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Threading;

namespace ChunkLoadingTerrain
{
    public class TerrainGenerator : MonoBehaviour
    {
        [SerializeField] GameObject player;
        [SerializeField] GameObject quadPrefab;
        [SerializeField] GameObject chunkParent;
        [SerializeField] GameObject poolParent;
        [SerializeField] int chunkRadius = 5;
        [SerializeField] int chunkSize = 1;
        [SerializeField] float cleanupDist = 10f;

        List<GameObject> quadPool;
        Dictionary<Vector2, GameObject> chunkDict;
        List<Vector2> activeChunks;

        float accumFrameTime = 0f;
        float currentTime = 0f;
        float lastTime = 0f;

        bool isCreatingChunks = false;
        bool isCleaningChunks = false;

        void Start()
        {
            quadPool = new List<GameObject>();
            chunkDict = new Dictionary<Vector2, GameObject>();
            activeChunks = new List<Vector2>();

            createChunks(player.transform.position);

        }

        /*
        void Test(object state)
        {
            isCreatingChunks = true;
            Thread.Sleep(2000);
            Debug.Log(Thread.CurrentThread.ManagedThreadId);
            isCreatingChunks = false;
        }
        */

        void createChunks(Vector3 playerPos)
        {
            Vector3Int playerQPos = getPlayerQPos(playerPos);
            for (int xg = -chunkRadius; xg < chunkRadius + 1; xg++)
            {
                for (int yg = -chunkRadius; yg < chunkRadius + 1; yg++)
                {
                    int x = xg + playerQPos.x;
                    int y = yg + playerQPos.z;

                    if (!chunkDict.ContainsKey(new Vector2(x, y)))
                    {
                        addChunk(x, y, false);
                    }
                    else if (chunkDict[new Vector2(x, y)] == null)
                    {
                        addChunk(x, y, true);
                    }
                }
            }
        }

        void addChunk(int x, int y, bool keyExists)
        {
            if (quadPool.Count != 0)
            {
                GameObject quad = quadPool[0];
                quad.SetActive(true);
                quad.transform.position = new Vector3(x * chunkSize, 0, y * chunkSize);
                quad.transform.parent = chunkParent.transform;

                if (keyExists)
                {
                    chunkDict[new Vector2(x, y)] = quad;
                }
                else
                {
                    chunkDict.Add(new Vector2(x, y), quad);
                }
                quadPool.RemoveAt(0);
            }
            else
            {
                GameObject quad = PrefabUtility.InstantiatePrefab(quadPrefab) as GameObject;
                quad.transform.localScale = new Vector3(chunkSize, chunkSize, chunkSize);
                quad.transform.position = new Vector3(x * chunkSize, 0, y * chunkSize);
                quad.transform.parent = chunkParent.transform;

                if (keyExists)
                {
                    chunkDict[new Vector2(x, y)] = quad;
                }
                else
                {
                    chunkDict.Add(new Vector2(x, y), quad);
                }
            }

            activeChunks.Add(new Vector2(x, y));
        }

        void cleanupChunks(Vector3 playerPos)
        {
            Vector3Int playerQPos = getPlayerQPos(playerPos);
            List<int> activeChunksToRemove = new List<int>();

            for (int i = 0; i < activeChunks.Count; i++)
            {
                if (Vector2.Distance(activeChunks[i], new Vector2(playerQPos.x, playerQPos.z)) > cleanupDist)
                {
                    GameObject quad = chunkDict[activeChunks[i]];
                    quad.SetActive(false);
                    quad.transform.parent = poolParent.transform;
                    quadPool.Add(quad);
                    chunkDict[activeChunks[i]] = null;
                    activeChunksToRemove.Add(i);
                }
            }

            for (int i = activeChunksToRemove.Count - 1; i > -1; i--)
            {
                activeChunks.RemoveAt(activeChunksToRemove[i]);
            }
        }

        Vector3Int getPlayerQPos(Vector3 playerPos)
        {
            ;
            return new Vector3Int(getQuantizedPos(playerPos.x), 0, getQuantizedPos(playerPos.z));
        }

        int getQuantizedPos(float pos)
        {
            return Mathf.FloorToInt((pos / chunkSize) + 0.5f);
        }

        void Update()
        {
            currentTime = Time.realtimeSinceStartup;
            float frameTime = currentTime - lastTime;
            accumFrameTime += frameTime;
            if (accumFrameTime > 1f)
            {
                Debug.Log(1f / frameTime);
                accumFrameTime = 0f;
            }
            lastTime = currentTime;

            /*
            if (!isCreatingChunks)
            {
                ThreadPool.QueueUserWorkItem(Test);
            }
            */

            createChunks(player.transform.position);
            cleanupChunks(player.transform.position);
        }
    }

}
