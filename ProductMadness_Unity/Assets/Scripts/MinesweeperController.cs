using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Minesweeper
{
    /// <summary>
    /// A class for controlling Minesweeper behaviour
    /// </summary>
    public class MinesweeperController : MonoBehaviour
    {
        public int bombs = 10;
        public Transform gameGrid;
        public GameObject tilePrefab;

        private readonly Color[] colours = { Color.blue, Color.green, Color.red, Color.magenta, Color.magenta, Color.cyan, Color.black, Color.grey };

        public delegate void GameStartCallback();
        public GameStartCallback OnGameStart;

        public delegate void GameEndCallback(bool isVictory);
        public GameEndCallback OnGameEnd;

        public delegate void FlagPlacedCallback(int remaining);
        public FlagPlacedCallback OnFlagPlaced;

        private bool gameRunning;
        private int flagCount;

        private MinesweeperBoard board;

        void Start()
        {
            SetupGame(bombs);
        }

        /// <summary>
        /// Set up a new game
        /// </summary>
        /// <param name="numBombs"></param>
        public void SetupGame(int numBombs)
        {
            board = new MinesweeperBoard();
            this.bombs = numBombs;
            //Clear tile grid
            if (gameGrid.childCount > 0)
            {
                foreach (Transform child in gameGrid.transform)
                {
                    Destroy(child.gameObject);
                }
            }

            //Setup board data
            board.SetupBoard(bombs, tilePrefab, gameGrid, TileClicked, AddFlag);

            gameRunning = true;
            OnFlagPlaced?.Invoke(bombs - flagCount);
        }

        /// <summary>
        /// Add a flag to a tile
        /// </summary>
        /// <param name="tile">The Tile</param>
        private void AddFlag(MineTile tile)
        {
            if (gameRunning)
            {
                tile.isFlagged = !tile.isFlagged;
                tile.flag.gameObject.SetActive(tile.isFlagged);

                flagCount += tile.isFlagged ? 1 : -1;
                OnFlagPlaced?.Invoke(bombs - flagCount);
                if (bombs - flagCount == 0)
                {
                    if (board.CheckVictory())
                    {
                        OnGameEnd(true);
                        gameRunning = false;
                    }
                }
            }
        }

        /// <summary>
        /// Handle clicking of a Tile
        /// </summary>
        /// <param name="tile">The Tile</param>
        private void TileClicked(MineTile tile)
        {
            if (gameRunning)
            {
                OnGameStart?.Invoke();
                if (tile.isBomb)
                {
                    tile.isRevealed = true;
                    tile.button.gameObject.SetActive(false);
                    OnGameEnd?.Invoke(false);
                    gameRunning = false;
                }
                else
                {
                    if (tile.isFlagged)
                    {
                        flagCount--;
                        OnFlagPlaced?.Invoke(bombs - flagCount);
                    }

                    TestTile(tile);
                }
            }
        }

        /// <summary>
        /// Check nearby tiles after clicking an empty tile
        /// </summary>
        /// <param name="tile">The tile whose naighbours should be checked</param>
        private void RevealNearby(MineTile tile)
        {
            TestTile(board.GetTile(tile.x - 1, tile.y - 1));
            TestTile(board.GetTile(tile.x, tile.y - 1));
            TestTile(board.GetTile(tile.x + 1, tile.y - 1));
            TestTile(board.GetTile(tile.x - 1, tile.y));
            TestTile(board.GetTile(tile.x + 1, tile.y));
            TestTile(board.GetTile(tile.x - 1, tile.y + 1));
            TestTile(board.GetTile(tile.x, tile.y + 1));
            TestTile(board.GetTile(tile.x + 1, tile.y + 1));
        }

        /// <summary>
        /// Test if a Tile should be revealed
        /// </summary>
        /// <param name="tile"></param>
        private void TestTile(MineTile tile)
        {
            if (tile != null && !tile.isRevealed && !tile.isBomb)
            {
                StartCoroutine(RevealTile(tile));
                int count = board.GetBombCount(tile);
                if (count == 0)
                {
                    tile.text.text = null;
                    RunDelayed(() => RevealNearby(tile), .1f);
                }
                else
                {
                    tile.text.text = count.ToString();
                    tile.text.color = colours[count - 1];
                }
            }
        }

        /// <summary>
        /// Animate the reveal of the Tile
        /// </summary>
        /// <param name="tile">The Tile to reveal</param>
        /// <returns></returns>
        private IEnumerator RevealTile(MineTile tile)
        {
            tile.isRevealed = true;
            Image image = tile.button.GetComponent<Image>();
            float fillAmount = 1f;
            while(fillAmount > 0)
            {
                fillAmount -= Time.deltaTime * 3;
                image.fillAmount = fillAmount;
                yield return new WaitForEndOfFrame();
            }
            tile.button.gameObject.SetActive(false);
        }

        /// <summary>
        /// Run and Action after a set delay
        /// </summary>
        /// <param name="action">The Action to run</param>
        /// <param name="delay">The delay to wait</param>
        private void RunDelayed(System.Action action, float delay)
        {
            StartCoroutine(RunDelayedCR(action, delay));
        }

        /// <summary>
        /// Run and Action after a set delay
        /// </summary>
        /// <param name="action">The Action to run</param>
        /// <param name="delay">The delay to wait</param>
        /// <returns></returns>
        private IEnumerator RunDelayedCR(System.Action action, float delay)
        {
            yield return new WaitForSeconds(delay);
            action.Invoke();
        }
    }
}