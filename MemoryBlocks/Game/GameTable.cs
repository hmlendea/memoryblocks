using System;
using System.Collections.Generic;

namespace MemoryBlocks.Game
{
    public class GameTable
    {
        Piece[,] pieces;
        int tableSize;
        int piecesMaxValue = 102;

        public int TableSize { get { return tableSize; } }

        public Piece[,] Pieces
        {
            get { return pieces; }
            set { pieces = value; }
        }

        public GameTable(int tableSize)
        {
            this.tableSize = tableSize;

            InitializePieces();
            GenerateTable();
        }

        public void Regenerate()
        {
            ClearTable();
            InitializePieces();
            GenerateTable();
        }

        private void ClearTable()
        {
            for (int i = 0; i < tableSize; i++)
                for (int j = 0; j < tableSize; j++)
                    pieces[i, j] = null;
            pieces = null;
        }

        private void InitializePieces()
        {
            pieces = new Piece[tableSize, tableSize];

            for (int i = 0; i < tableSize; i++)
                for (int j = 0; j < tableSize; j++)
                {
                    pieces[i, j] = new Piece();
                    pieces[i, j].PositionX = i;
                    pieces[i, j].PositionY = j;
                } 
        }

        private void GenerateTable()
        {
            Random rnd = new Random();
            List<int> valuesUsed = new List<int>();
            int piecesCount = tableSize * tableSize / 2;
            int val = -1;

            for (int i = 0; i < piecesCount; i++)
            {
                int placed = 0;

                while(val == -1 || valuesUsed.Contains(val))
                    val = rnd.Next(0, piecesMaxValue);
                valuesUsed.Add(val);

                while (placed < 2)
                {
                    int x = rnd.Next(0, TableSize);
                    int y = rnd.Next(0, TableSize);

                    if (pieces[x, y].Value < 0)
                    {
                        pieces[x, y].Value = val;
                        placed += 1;
                    }
                }
            }
        }
    }
}

