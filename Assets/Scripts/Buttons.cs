using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

// Used so player can use mouse input
public class Buttons : MonoBehaviour
{
    // Reference to the game board and the puse button text
    [SerializeField] GameBoard gameBoard;
    [SerializeField] TextMeshProUGUI pauseButtonText;

    // Used to quit the game
    public void QuitGame()
    {
        Application.Quit();
    }

    // Used to clear the game and start new game
    public void ClearTheBoard()
    {
        gameBoard.Clear();
    }

    // Used to move on step forward in simulation
    public void TakeStep()
    {
        // Only move one step if game is paused
        if (gameBoard.isPaused)
        {
            gameBoard.StartOneStepForward();
        }
    }

    // Used to pause and unpause the game
    public void PauseGame()
    {
        gameBoard.isPaused = !gameBoard.isPaused;

        // If no cells are alive and the one cell is alive when 
        // pause button is clicked then clear the board
        if (gameBoard.HaveNoAliveCells() || gameBoard.HaveOneAliveCell())
        {
            gameBoard.Clear();
        }

        // If game board is being unpaused then start the simulation again
        if (!gameBoard.isPaused)
        {
            pauseButtonText.text = "Pause";
            gameBoard.StartSimulation();
        }
        // Else game is paused, don't update anything, set pause button text
        else
        {
            pauseButtonText.text = "Unpause";
        }
    }
}
