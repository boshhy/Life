using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Used to hold a pattern
[CreateAssetMenu(menuName = "Game of Life/Pattern")]
public class Pattern : ScriptableObject
{
    // Holds where a cell should exist according to x,y location
    public Vector2Int[] cells;

    // Used to get the center of the pattern
    public Vector2Int GetCenter()
    {
        // If not cells are in the pattern return an empty pattern
        if (cells == null || cells.Length == 0)
        {
            return Vector2Int.zero;
        }

        // Set min and max to zero, will be used 
        // for left and right of pattern location
        Vector2Int min = Vector2Int.zero;
        Vector2Int max = Vector2Int.zero;

        // Go through all the cells and see which one is 
        // min (most left) and max (most right)
        for (int i = 0; i < cells.Length; i++)
        {
            Vector2Int cell = cells[i];

            // Get most left cell
            min.x = Mathf.Min(cell.x, min.x);
            min.y = Mathf.Min(cell.y, min.y);

            // Get most right cell
            max.x = Mathf.Max(cell.x, max.x);
            max.y = Mathf.Max(cell.y, max.y);
        }

        // Return the middle of the most left and most right cell (mididle)
        return (min + max) / 2;
    }
}
