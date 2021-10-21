using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Minesweeper
{
    /// <summary>
    /// A class for controlling Minesweeper UI elements
    /// </summary>
    public class MinesweeperUIController : MonoBehaviour
    {
        public MinesweeperController minesweeperController;
        public InputField bombsInput;
        public InputField gridInput;
        public Button startButton;

        public Text timerText;
        public Text remainingText;

        private bool gameRunning;
        private int timeTaken;

        void Awake()
        {
            startButton.onClick.AddListener(SetupNewGame);
            minesweeperController.OnGameStart += () =>
            {
                if (!gameRunning)
                {
                    gameRunning = true;
                    StartCoroutine(StartTimer());
                }
            };

            minesweeperController.OnGameEnd += (isVictory) =>
            {
                gameRunning = false;
                remainingText.text = isVictory ? "Victory" : "Defeat";
            };

            minesweeperController.OnFlagPlaced += (remaining) =>
            {
                remainingText.text = remaining.ToString();
            };
        }

        /// <summary>
        /// Set up a new game
        /// </summary>
        private void SetupNewGame()
        {
            gameRunning = false;
            timerText.text = null;
            minesweeperController.SetupGame(int.Parse(bombsInput.text));
        }

        /// <summary>
        /// Begin the timer and update UI every second
        /// </summary>
        /// <returns></returns>
        private IEnumerator StartTimer()
        {
            timeTaken = 0;
            while (gameRunning)
            {
                yield return new WaitForSeconds(1);
                timeTaken++;
                timerText.text = timeTaken.ToString() + "s";
            }
        }
    }
}