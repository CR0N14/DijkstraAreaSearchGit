using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DijkstraAreaSearchProblem : MonoBehaviour
{
    MouseManager mm;

    void Start()
    {

        GameObject cmObject;
        cmObject = GameObject.FindWithTag("MouseManager");
        mm = cmObject.GetComponent(typeof(MouseManager)) as MouseManager;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            WorldTile _tileClicked = mm.mouseTile();
            if (_tileClicked != null)
            {
                GetDijkstraArea(_tileClicked, 3);
            }
        }
    }


    public Dictionary<WorldTile, PathAndCost> GetDijkstraArea(WorldTile start, double mobility)
    {
        Queue<WorldTile> path = new Queue<WorldTile>(); //a path to reach a tile is stored as a queue of tiles

        Dictionary<WorldTile, Queue<WorldTile>> pathToEachTile = new Dictionary<WorldTile, Queue<WorldTile>>(); //stores path to reach each tile

        Dictionary<WorldTile, double> costToReachTile = new Dictionary<WorldTile, double>();    //stores mobility cost to reach each tile

        Dictionary<WorldTile, PathAndCost> dijkstraArea = new Dictionary<WorldTile, PathAndCost>();  //Dictionary to be returned

        Dictionary<WorldTile, WorldTile> NextTileToStart = new Dictionary<WorldTile, WorldTile>();  //Key=Tile, Value=Direction to Start

        PriorityQueue<WorldTile> frontier = new PriorityQueue<WorldTile>();
        frontier.Enqueue(start, 0);
        costToReachTile[start] = 0;


        while (frontier.Count > 0) //main loop that handles pathfinding logic, deciding what tiles are reachable and and storing the costToReachTile and NextTileToStart for each tile
        {
            WorldTile curTile = frontier.Dequeue();

            foreach (WorldTile neighbour in curTile.NeighbourArray)
            {
                if (neighbour != null)
                {

                    double newCost = -999;

                    if (curTile.NeighbourArrayOctagonal.Contains(neighbour))     //only check octagonal neighbouring tiles
                    {
                        newCost = costToReachTile[curTile] + neighbour.MoveCost;
                    }
                    if (curTile.NeighbourArrayDiagonal.Contains(neighbour))     //don't check diagonal neighbouring tiles
                    {
                        continue;
                    }

                    if (newCost > mobility)     //neighbour won't be checked if reaching it exceeds mobility
                    {
                        continue;
                    }

                    if (costToReachTile.ContainsKey(neighbour) == false || newCost < costToReachTile[neighbour])
                    {
                        if (neighbour.IsWalkableSurface)  
                        {
                            costToReachTile[neighbour] = newCost;
                            double priority = newCost;
                            frontier.Enqueue(neighbour, priority);
                            NextTileToStart[neighbour] = curTile;
                            StorePathAndCost(neighbour);
                            continue;
                        }
                    }
                }
            }
        }
        
        List<WorldTile> movementArea = new List<WorldTile>(dijkstraArea.Keys);  // DEBUGGING
        foreach (WorldTile _tile in movementArea)                               //
        {                                                                       //
            dijkstraArea.TryGetValue(_tile, out PathAndCost _path);             //
            Debug.Log(_path.Path.Count);                                        // Prints the queue counts of ALL paths as 0??
            Debug.Log(_path.Cost);                                              // Prints costs of 1, 2 and 3 as expected
        }                                                                       //
        
        return dijkstraArea;

        void StorePathAndCost(WorldTile nei)   //secondary loop that takes reachable tiles from main loop
        {
            path = new Queue<WorldTile>();

            WorldTile nextTile = nei;

            while (true)    //creates a path of tiles to start (includes nei tile and excludes start tile)
            {
                path.Enqueue(nextTile);
                nextTile = NextTileToStart[nextTile];
                if (nextTile == start) break;
            }

            nextTile = nei;

            while (true)    //for each tile in the path, stores the tile as a dictionary key and the path and cost to reach the tile as the value
            {
                if (pathToEachTile.ContainsKey(nextTile))
                {
                    if (path.Count == 0) break;
                    nextTile = path.Dequeue();
                    continue;
                }

                pathToEachTile.Add(nextTile, path);

                PathAndCost pathAndCost = new PathAndCost
                {
                    Path = pathToEachTile[nextTile],
                    Cost = costToReachTile[nextTile],
                };
                dijkstraArea.Add(nextTile, pathAndCost);

                List<WorldTile> movementArea = new List<WorldTile>(dijkstraArea.Keys);  // DEBUGGING
                foreach (WorldTile _tile in movementArea)                               //
                {                                                                       //
                    dijkstraArea.TryGetValue(_tile, out PathAndCost _path);             //
                    Debug.Log(_path.Path.Count);                                        // Prints queue counts of 0, 1, 2, and 3 as expected
                    Debug.Log(_path.Cost);                                              // Prints costs of 1, 2 and 3 as expected
                }                                                                       //    
                
                if (nextTile == nei) path.Dequeue();
                if (path.Count == 0) break;

                nextTile = path.Dequeue();
            }
        }
    }
}