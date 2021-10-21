using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Minesweeper
{
    /// <summary>
    /// A class for holding the Minesweeper board data
    /// </summary>
    public class MinesweeperBoard
    {
        public int rows = 10;
        public int cols = 10;
        public int bombs = 10;
        private MineTile[][] tileArray;

        public delegate void TileClickCallback(MineTile tile);
        public delegate void TileLongPressCallback(MineTile tile);

        /// <summary>
        /// Set up the board state for a new game
        /// </summary>
        /// <param name="numBombs">The number of bombs to set</param>
        /// <param name="tilePrefab">The prefab for a Tile</param>
        /// <param name="tileParent">The parent Transform to add Tiles to</param>
        /// <param name="tileClicked">Callback for when a tile is clicked</param>
        /// <param name="onLongPress">Callback for when a tile is long pressed</param>
        public void SetupBoard(int numBombs, GameObject tilePrefab, Transform tileParent, TileClickCallback tileClicked, TileLongPressCallback onLongPress)
        {
            //Validate number of bombs
            if (numBombs > rows * cols / 2)
                numBombs = rows * cols / 2;
            if (numBombs < 1)
                numBombs = 1;
            this.bombs = numBombs;

            //Create array
            tileArray = new MineTile[rows][];
            for (int i = 0; i < rows; i++)
            {
                tileArray[i] = new MineTile[cols];
            }

            //Instantiate tiles
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    GameObject tile = GameObject.Instantiate(tilePrefab);
                    tile.transform.parent = tileParent;
                    tile.transform.localScale = Vector3.one;
                    MineTile mineTile = tile.GetComponent<MineTile>();
                    mineTile.x = col;
                    mineTile.y = row;
                    tileArray[col][row] = mineTile;
                    mineTile.button.onClick.AddListener(delegate { tileClicked(mineTile); });
                    mineTile.button.onLongPress.AddListener(delegate { onLongPress(mineTile); });
                }
            }

            // Set the bombs
            int bombCount = numBombs;
            while (bombCount > 0)
            {
                int randCol = Random.Range(0, cols);
                int randRow = Random.Range(0, rows);

                if (!tileArray[randCol][randRow].isBomb)
                {
                    tileArray[randCol][randRow].isBomb = true;
                    tileArray[randCol][randRow].image.gameObject.SetActive(true);
                    bombCount--;
                }
            }
        }

        /// <summary>
        /// Get the MineTile at the given row and column
        /// </summary>
        /// <param name="col">The Column</param>
        /// <param name="row">The Row</param>
        /// <returns></returns>
        public MineTile GetTile(int col, int row)
        {
            if (col > -1 && col < cols && row > -1 && row < rows)
                return tileArray[col][row];
            return null;
        }

        /// <summary>
        /// Find how many bombs are in neighbouring tiles
        /// </summary>
        /// <param name="tile">The tile to check</param>
        /// <returns>The number of neighbouring bombs</returns>
        public int GetBombCount(MineTile tile)
        {
            int nearbyBombs = 0;
            nearbyBombs += IsTileBomb(tile.x - 1, tile.y - 1);
            nearbyBombs += IsTileBomb(tile.x, tile.y - 1);
            nearbyBombs += IsTileBomb(tile.x + 1, tile.y - 1);
            nearbyBombs += IsTileBomb(tile.x - 1, tile.y);
            nearbyBombs += IsTileBomb(tile.x + 1, tile.y);
            nearbyBombs += IsTileBomb(tile.x - 1, tile.y + 1);
            nearbyBombs += IsTileBomb(tile.x, tile.y + 1);
            nearbyBombs += IsTileBomb(tile.x + 1, tile.y + 1);
            return nearbyBombs;
        }

        /// <summary>
        /// Check if every bomb has a flag on it, or if the number of unrevealed tiles matches the number of bombs
        /// </summary>
        /// <returns>If the game has been won</returns>
        public bool CheckVictory()
        {
            int matchCount = 0;
            int remainingCount = 0;
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    if (tileArray[col][row].isBomb && tileArray[col][row].isFlagged)
                    {
                        matchCount++;
                    }
                    if(!tileArray[col][row].isRevealed)
                    {
                        remainingCount++;
                    }
                }
            }

            return (matchCount == bombs || remainingCount == bombs);
        }

        /// <summary>
        /// Test is a tile has a bomb, checking for array range
        /// </summary>
        /// <param name="col">Column number</param>
        /// <param name="row">Row number</param>
        /// <returns></returns>
        private int IsTileBomb(int col, int row)
        {
            if (col > -1 && col < cols && row > -1 && row < rows)
                return tileArray[col][row].isBomb ? 1 : 0;
            return 0;
        }
    }
}