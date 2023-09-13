using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

public class GameBoard : MonoBehaviour
{
    [SerializeField] private Tilemap currentState;
    [SerializeField] private Tilemap nextState;

    [SerializeField] private Tile aliveTile;
    [SerializeField] private Tile deadTile;
    [SerializeField] private Pattern pattern;

    [SerializeField] private float updateInterval = 0.05f;

    [SerializeField] TextMeshProUGUI pauseButtonText;

    private HashSet<Vector3Int> aliveCells;
    private HashSet<Vector3Int> cellsToCheck;

    public bool isPaused = true;
    public bool isClickingPauseButton = false;

    void Awake()
    {
        aliveCells = new HashSet<Vector3Int>();
        cellsToCheck = new HashSet<Vector3Int>();

    }

    private void Start()
    {
        SetPattern(pattern);
    }

    private void SetPattern(Pattern pattern)
    {
        Clear();

        Vector2Int center = pattern.GetCenter();

        for (int i = 0; i < pattern.cells.Length; i++)
        {
            Vector3Int cell = (Vector3Int)(pattern.cells[i] - center);
            currentState.SetTile(cell, aliveTile);
            aliveCells.Add(cell);
        }
    }

    public void Clear()
    {
        isPaused = true;
        pauseButtonText.text = "Unpause";
        currentState.ClearAllTiles();
        nextState.ClearAllTiles();
        aliveCells.Clear();
        cellsToCheck.Clear();
    }

    void OnEnable()
    {
        StartCoroutine(Simulate());
    }

    public void StartSimulation()
    {
        StartCoroutine(Simulate());
    }

    private IEnumerator Simulate()
    {
        var interval = new WaitForSeconds(updateInterval);
        yield return interval;

        while (!isPaused)
        {
            UpdateState();
            yield return interval; // new WaitForSeconds(updateInterval); 
        }
    }

    public void StartOneStepForward()
    {
        StartCoroutine(OneStepForward());
    }

    private IEnumerator OneStepForward()
    {
        UpdateState();
        yield return new WaitForSeconds(updateInterval);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Application.Quit();
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            Clear();
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            isPaused = !isPaused;

            if (aliveCells.Count == 0)
            {
                isPaused = true;
            }


            if (!isPaused)
            {
                pauseButtonText.text = "Pause";
                StartCoroutine(Simulate());
            }
            else
            {
                pauseButtonText.text = "Unpause";
            }
        }
        else if (isPaused && Input.GetKeyDown(KeyCode.RightArrow))
        {
            StartCoroutine(OneStepForward());
        }

        if (Input.GetMouseButton(0))
        {
            if (isPaused)
            {
                Vector2 mousePoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                int x = (int)Math.Floor(mousePoint.x); //Mathf.RoundToInt(mousePoint.x);
                int y = (int)Math.Floor(mousePoint.y); //Mathf.RoundToInt(mousePoint.y);

                // check to see pause/unpause button

                currentState.SetTile(new Vector3Int(x, y, 0), aliveTile);
                aliveCells.Add(new Vector3Int(x, y, 0));

            }
        }
        else if (Input.GetMouseButton(1))
        {
            if (isPaused)
            {
                Vector2 mousePoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                int x = (int)Math.Floor(mousePoint.x); //Mathf.RoundToInt(mousePoint.x);
                int y = (int)Math.Floor(mousePoint.y); //Mathf.RoundToInt(mousePoint.y);

                if (IsAlive(new Vector3Int(x, y, 0)))
                {
                    currentState.SetTile(new Vector3Int(x, y, 0), deadTile);
                    aliveCells.Remove(new Vector3Int(x, y, 0));
                }
            }
        }
    }

    private void UpdateState()
    {
        cellsToCheck.Clear();
        if (aliveCells.Count == 0)
        {
            Clear();
        }
        else
        {
            foreach (Vector3Int cell in aliveCells)
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (!cellsToCheck.Contains(cell + new Vector3Int(x, y, 0)))
                        {
                            cellsToCheck.Add(cell + new Vector3Int(x, y, 0));
                        }
                    }
                }
            }

            foreach (Vector3Int cell in cellsToCheck)
            {
                int neighbors = CountNeighbors(cell);
                bool alive = IsAlive(cell);

                if (!alive && neighbors == 3)
                {
                    // becomes alive
                    nextState.SetTile(cell, aliveTile);
                    aliveCells.Add(cell);
                }
                else if (alive && (neighbors < 2 || neighbors > 3))
                {
                    // dies
                    nextState.SetTile(cell, deadTile);
                    aliveCells.Remove(cell);
                }
                else
                {
                    // Stays the same
                    nextState.SetTile(cell, currentState.GetTile(cell));
                }
            }

            Tilemap temp = currentState;
            currentState = nextState;
            nextState = temp;
            nextState.ClearAllTiles();
        }
    }

    private int CountNeighbors(Vector3Int cell)
    {
        int count = 0;

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector3Int neighbor = cell + new Vector3Int(x, y, 0);

                if (x == 0 && y == 0)
                {
                    continue;
                }
                else if (IsAlive(neighbor))
                {
                    count++;
                }
            }
        }

        return count;
    }

    private bool IsAlive(Vector3Int cell)
    {
        return currentState.GetTile(cell) == aliveTile;
    }

    public bool HaveNoAliveCells()
    {
        if (aliveCells.Count == 0)
        {
            return true;
        }

        return false;
    }

    public bool HaveOneAliveCell()
    {
        if (aliveCells.Count == 1)
        {
            return true;
        }

        return false;
    }
}



