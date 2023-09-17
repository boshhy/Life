using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

// Used to keep track of the gamebaord
public class GameBoard : MonoBehaviour
{
    // Used to keeep current and the next state of the board
    [SerializeField] private Tilemap currentState;
    [SerializeField] private Tilemap nextState;

    // Reference to alive and dead Tile
    [SerializeField] private Tile aliveTile;
    [SerializeField] private Tile deadTile;

    // Used to set a starting pattern
    // [SerializeField] private Pattern pattern;

    // Used for the intervals of state changes
    [SerializeField] private float updateInterval = 0.05f;

    // Reference to pause button
    [SerializeField] TextMeshProUGUI pauseButtonText;

    // A set of the currently alive cells and cells that 
    // need to be checked for changces on next state
    private HashSet<Vector3Int> aliveCells;
    private HashSet<Vector3Int> cellsToCheck;

    // Used to check is user has paused the board
    public bool isPaused = true;

    // Create a new aliveCells and cellsTocheck hash tables at the beginning
    void Awake()
    {
        aliveCells = new HashSet<Vector3Int>();
        cellsToCheck = new HashSet<Vector3Int>();
    }

    // private void Start()
    // {
    // SetPattern(pattern);
    // }

    // private void SetPattern(Pattern pattern)
    // {
    //     Clear();

    //     Vector2Int center = pattern.GetCenter();

    //     for (int i = 0; i < pattern.cells.Length; i++)
    //     {
    //         Vector3Int cell = (Vector3Int)(pattern.cells[i] - center);
    //         currentState.SetTile(cell, aliveTile);
    //         aliveCells.Add(cell);
    //     }
    // }

    // Used to clear the board
    public void Clear()
    {
        isPaused = true;
        pauseButtonText.text = "Unpause";
        currentState.ClearAllTiles();
        nextState.ClearAllTiles();
        aliveCells.Clear();
        cellsToCheck.Clear();
    }

    // Starts the simulation
    void OnEnable()
    {
        StartCoroutine(Simulate());
    }

    // Used to start the simulation
    public void StartSimulation()
    {
        StartCoroutine(Simulate());
    }

    // Used to simulate the game of life
    private IEnumerator Simulate()
    {
        // Set the time interval
        var interval = new WaitForSeconds(updateInterval);
        yield return interval;

        // kepp updating the state as long as game is not paused
        while (!isPaused)
        {
            UpdateState();
            yield return interval;
        }
    }

    // Used as a public function so buttons can call oneStepForward coroutine
    public void StartOneStepForward()
    {
        StartCoroutine(OneStepForward());
    }

    // Used to move one step forward in the game state
    private IEnumerator OneStepForward()
    {
        UpdateState();
        yield return new WaitForSeconds(updateInterval);
    }

    // Used to check for keyboard and mouse input
    void Update()
    {
        // If player presses 'Q' then quit
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Application.Quit();
        }
        // If player presses 'C' clear the board
        else if (Input.GetKeyDown(KeyCode.C))
        {
            Clear();
        }
        // If player presses 'space' pause/unpause game
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            isPaused = !isPaused;

            // If no alive cells on board, keep game paused
            if (aliveCells.Count == 0)
            {
                isPaused = true;
            }

            // If game is not paused then start the simulation, set pauseButtonText
            if (!isPaused)
            {
                pauseButtonText.text = "Pause";
                StartCoroutine(Simulate());
            }
            // If game is paused then don't start simulation, set pauseButtonText
            else
            {
                pauseButtonText.text = "Unpause";
            }
        }
        // If right arrow is pressed while paused, then step one step in the simulation
        else if (isPaused && Input.GetKeyDown(KeyCode.RightArrow))
        {
            StartCoroutine(OneStepForward());
        }

        // If left mouse clicked then add alive cell
        if (Input.GetMouseButton(0))
        {
            // Only add if game is not paused
            if (isPaused)
            {
                // Get mouse position
                Vector2 mousePoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                // Set x and y of mouse position
                int x = (int)Math.Floor(mousePoint.x);
                int y = (int)Math.Floor(mousePoint.y);

                // Add an alive cell at the location  clicked
                currentState.SetTile(new Vector3Int(x, y, 0), aliveTile);
                aliveCells.Add(new Vector3Int(x, y, 0));

            }
        }
        // If right mouse clicked then remove alive cell
        else if (Input.GetMouseButton(1))
        {
            // Only remove if game is not paused
            if (isPaused)
            {// Get mouse position
                Vector2 mousePoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                // Set x and y of mouse position
                int x = (int)Math.Floor(mousePoint.x);
                int y = (int)Math.Floor(mousePoint.y);

                // If the cell clicked is alive, then change to a dead cell
                if (IsAlive(new Vector3Int(x, y, 0)))
                {
                    currentState.SetTile(new Vector3Int(x, y, 0), deadTile);
                    aliveCells.Remove(new Vector3Int(x, y, 0));
                }
            }
        }
    }

    // Used to update the state of the board
    private void UpdateState()
    {
        // Clear the set of cellsToCheck
        cellsToCheck.Clear();

        // If no cells on board then clear the board to start over
        if (aliveCells.Count == 0)
        {
            Clear();
        }
        // Else if there is alive cells on board, then update the state
        else
        {
            // For each alive cell in, add it and its neighbors to the cells to check set
            foreach (Vector3Int cell in aliveCells)
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        // Only add the cell if it not already added
                        if (!cellsToCheck.Contains(cell + new Vector3Int(x, y, 0)))
                        {
                            cellsToCheck.Add(cell + new Vector3Int(x, y, 0));
                        }
                    }
                }
            }

            // For each cell in cells to check set, add its surrounding neighbors
            foreach (Vector3Int cell in cellsToCheck)
            {
                // Get the number of neighbors for the cell
                int neighbors = CountNeighbors(cell);

                // Check to see if the cell is alive
                bool alive = IsAlive(cell);

                // If it is not alive and has 3 neighbors then set it to be alive
                if (!alive && neighbors == 3)
                {
                    // Change dead cell to becomes alive
                    nextState.SetTile(cell, aliveTile);
                    aliveCells.Add(cell);
                }
                // If its alive and has less then 2 or more then 3 neighbors then kill the cell
                else if (alive && (neighbors < 2 || neighbors > 3))
                {
                    // Change alive cell to become dead
                    nextState.SetTile(cell, deadTile);
                    aliveCells.Remove(cell);
                }
                else
                {
                    // Stays the same
                    nextState.SetTile(cell, currentState.GetTile(cell));
                }
            }

            // Update the currentState and clear the nextState
            Tilemap temp = currentState;
            currentState = nextState;
            nextState = temp;
            nextState.ClearAllTiles();
        }
    }

    // Used to return number of neighbors for a cell
    private int CountNeighbors(Vector3Int cell)
    {
        // Start cell to 0
        int count = 0;

        // Check all 9 surrounding cells
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                // Get the neighbor at this location
                Vector3Int neighbor = cell + new Vector3Int(x, y, 0);

                // Skip cell that was passed in
                if (x == 0 && y == 0)
                {
                    continue;
                }
                // If neighbor is alive, update the count
                else if (IsAlive(neighbor))
                {
                    count++;
                }
            }
        }

        // Return the number of neighbors
        return count;
    }

    // Returns true is cell is alive, else its dead and return false
    private bool IsAlive(Vector3Int cell)
    {
        return currentState.GetTile(cell) == aliveTile;
    }

    // Returns true if no cells are alive, else false if we have cells  alive
    public bool HaveNoAliveCells()
    {
        if (aliveCells.Count == 0)
        {
            return true;
        }

        return false;
    }

    // Returns true if one cell is alive, else false
    public bool HaveOneAliveCell()
    {
        if (aliveCells.Count == 1)
        {
            return true;
        }

        return false;
    }
}
