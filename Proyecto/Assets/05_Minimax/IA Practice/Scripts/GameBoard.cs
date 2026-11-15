using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AICookbook.Minimax
{

    // @copyright © 2023+ IA Minimax
    // @author Author: Martin Pérez Villabrille
    // martinx902@gmail.com
    // @date 16/11/2023
    // @brief Creates and controls the board of the game

    public class GameBoard : MonoBehaviour
    {
        private enum Piece
        {
            Empty = 0,
            Blue = 1,
            Red = 2
        }

        public int columns = 7;
        public int rows = 6;

        public int[,] board;

        // Board for evaluation function
        public int[,] boardSpaces;

        [SerializeField]
        private bool debug = false;

        [SerializeField]
        private float dropTime = 5f;

        [SerializeField]
        private int numPiecesToWin = 4;

        // Game Objects
        public GameObject pieceRed;

        public GameObject pieceBlue;
        public GameObject pieceField;

        private bool isLoading = true;
        private bool gameOver = false;

        private GameObject gameObjectField;

        private bool isDropping = false;

        public GameManager gameManager;

        /// <summary> True while a piece is falling: the next move must wait for it </summary>
        public bool IsDropping => isDropping;

        public GameBoard()
        { }

        public GameBoard(GameBoard other)
        {
            this.board = other.board;
        }

        private void Awake()
        {
            board = new int[columns, rows];
            boardSpaces = new int[columns, rows];

            gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

            int max = Mathf.Max(rows, columns);

            if (numPiecesToWin > max)
                numPiecesToWin = max;

            CreateField();
        }

        private void CreateField()
        {
            isLoading = true;

            //Delete the last field if existing

            gameObjectField = GameObject.Find("Field");
            if (gameObjectField != null)
            {
                DestroyImmediate(gameObjectField);
            }

            //Create a new one
            gameObjectField = new GameObject("Field");
            gameObjectField.tag = "GameBoard";

            // create an empty board and instantiate the cells
            board = new int[columns, rows];
            for (int x = 0; x < columns; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    board[x, y] = (int)Piece.Empty;
                    GameObject g = Instantiate(pieceField, new Vector3(x, y * -1, -1), Quaternion.identity) as GameObject;
                    g.tag = "GameBoard";
                    g.transform.parent = gameObjectField.transform;
                }
            }

            isLoading = false;
            gameOver = false;

            // center camera
            Camera.main.transform.position = new Vector3(
                (columns - 1) / 2.0f, -((rows - 1) / 2.0f), Camera.main.transform.position.z);
        }

        // Gets all the possibles moves.
        public List<int> GetPossibleMoves()
        {
            return ConnectFourRules.ValidMoves(board);
        }

        public void MakeMove(int column, int playerID, bool tempMove = false)
        {
            //Debug.Log("Player with ID: " + playerID + " has placed a piece in column: " + column);

            if (!isDropping)
                StartCoroutine(dropPiece(column, playerID));
        }

    #region TemporalBoard

        public GameBoard CreateTempBoardWithMove(int column, int playerID)
        {
            GameBoard tempBoard = new GameBoard();

            tempBoard.board = new int[columns, rows];

            tempBoard.gameManager = gameManager;

            for (int i = 0; i < columns; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    tempBoard.board[i, j] = board[i, j];
                }
            }

            MakeTemporalMove(tempBoard, column, playerID);

            return tempBoard;
        }

        private void MakeTemporalMove(GameBoard tempBoard, int column, int playerID)
        {
            for (int i = rows - 1; i >= 0; i--)
            {
                if (tempBoard.board[column, i] == 0)
                {
                    tempBoard.board[column, i] = playerID == 1 ? (int)Piece.Blue : (int)Piece.Red;
                    break;
                }
            }
        }

    #endregion TemporalBoard

        private IEnumerator dropPiece(int dropColumn, int playerID)
        {
            isDropping = true;

            //Find the place for the new piece in the board

            Vector3 startPosition = new Vector3(dropColumn, gameObjectField.transform.position.y + 1, 0);
            Vector3 endPosition = new Vector3();

            bool foundFreeCell = false;
            for (int i = rows - 1; i >= 0; i--)
            {
                if (board[dropColumn, i] == 0)
                {
                    foundFreeCell = true;
                    board[dropColumn, i] = playerID == 1 ? (int)Piece.Blue : (int)Piece.Red;
                    endPosition = new Vector3(dropColumn, i * -1, startPosition.z);

                    break;
                }
            }

            //Drop Piece Animation

            if (foundFreeCell)
            {
                GameObject newPiece = Instantiate(playerID == 1 ? pieceBlue : pieceRed);

                float distance = Vector3.Distance(startPosition, endPosition);

                float t = 0;
                while (t < 1)
                {
                    t += Time.deltaTime * dropTime * ((rows - distance) + 1);

                    newPiece.transform.position = Vector3.Lerp(startPosition, endPosition, t);
                    yield return null;
                }

                newPiece.transform.parent = gameObjectField.transform;
            }

            isDropping = false;

            yield return 0;
        }

        //public int Evaluate(int playerID)
        //{
        //    int maxHeight = Math.Min(rows, columns);
        //    int total = 0;

        //    for (int row = 0; row < rows; row++)
        //    {
        //        for (int col = 0; col < columns; col++)
        //        {
        //            if (board[col, row] == playerID)
        //            {
        //                int minHeight = Math.Min(maxHeight, row + 1);

        //                for (int height = minHeight - 1; height >= 0; height--)
        //                {
        //                    if (board[col, row - height] == playerID)
        //                    {
        //                        total += 1 << height;
        //                    }
        //                    else
        //                    {
        //                        break;
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    return total;
        //}

    #region CheckWin

        public GameState CheckWin(int playerID)
        {
            if (ConnectFourRules.IsWin(board, playerID))
                return GameState.Win;
            if (ConnectFourRules.IsFull(board))
                return GameState.Draw;
            return GameState.Playing;
        }




    #endregion CheckWin

    #region Board Comprobations

        public bool IsValidMove(int column)
        {
            return ConnectFourRules.IsValidMove(board, column);
        }

        public bool FieldContainsEmptyCell()
        {
            return !ConnectFourRules.IsFull(board);
        }

        public int GetColumnFromHitPoint(Vector3 hitPoint)
        {
            float cellWidth = GetCellWidth();

            float xOffset = hitPoint.x - gameObjectField.transform.position.x + cellWidth * 0.5f;

            int column = (int)(xOffset / cellWidth);

            column = Mathf.Clamp(column, 0, columns);

            return column;
        }

        private float GetCellWidth()
        {
            return pieceField.transform.localScale.x;
        }

    #endregion Board Comprobations

    #region Board Evaluation


        public int EvaluateBoard(int ID)
        {
            return BoardEvaluator.Evaluate(board, ID);
        }

    #endregion Board Evaluation
    }
}
