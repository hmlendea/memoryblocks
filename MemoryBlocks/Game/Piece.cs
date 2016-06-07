﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace MemoryBlocks.Game
{
    public class Piece
    {
        int value;
        int size, posX, posY;
        bool isFlipped, isCleared;

        public int Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        public int Size
        {
            get { return size; }
            set { size = value; }
        }

        public int PositionX
        {
            get { return posX; }
            set { posX = value; }
        }

        public int PositionY
        {
            get { return posY; }
            set { posY = value; }
        }

        public bool IsFlipped
        {
            get { return isFlipped; }
            set { isFlipped = value; }
        }

        public bool IsCleared
        {
            get { return isCleared; }
            set { isCleared = value; }
        }

        public Piece()
        {
            size = 48;
            posX = 0;
            posY = 0;

            isFlipped = false;
            isCleared = false;

            value = -1;
        }
    }
}