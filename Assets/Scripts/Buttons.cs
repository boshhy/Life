using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Buttons : MonoBehaviour
{
    [SerializeField] GameBoard gameBoard;
    [SerializeField] TextMeshProUGUI pauseButtonText;

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ClearTheBoard()
    {
        gameBoard.Clear();
    }

    public void TakeStep()
    {
        if (gameBoard.isPaused)
        {
            gameBoard.StartOneStepForward();
        }
    }

    public void PauseGame()
    {
        gameBoard.isPaused = !gameBoard.isPaused;

        if (gameBoard.HaveNoAliveCells() || gameBoard.HaveOneAliveCell())
        {
            gameBoard.Clear();
        }

        if (!gameBoard.isPaused)
        {
            pauseButtonText.text = "Pause";
            gameBoard.StartSimulation();
        }
        else
        {
            pauseButtonText.text = "Unpause";
        }
    }
}
