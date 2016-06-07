using System;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Timers;

using Gtk;

namespace MemoryBlocks.Game
{
    public class GameEngine
    {
        GameTable gameTable;
        Color primaryColor, secondaryColor, backgroundColor;
        int tableSize, tileSpacing;
        int gameTime, flips, piecesLeft;
        bool isRunning;

        Gdk.Window gdkWindowTable, gdkWindowInfoBar;

        public Gdk.Window GdkWindowTable
        {
            get { return gdkWindowTable; }
            set { gdkWindowTable = value; }
        }

        public Gdk.Window GdkWindowInfoBar
        {
            get { return gdkWindowInfoBar; }
            set { gdkWindowInfoBar = value; }
        }

        public bool Completed { get { return piecesLeft == 0; } }

        public int GameTime { get { return gameTime; } }

        public bool IsRunning { get { return isRunning; } }

        public int TableSize { get { return tableSize; } }

        public int TileSpacing
        {
            get { return tileSpacing; }
            set { tileSpacing = value; }
        }

        public Color PrimaryColor
        {
            get { return primaryColor; }
            set { primaryColor = value; }
        }

        public Color SecondaryColor
        {
            get { return secondaryColor; }
            set { secondaryColor = value; }
        }

        public Color BackgroundColor
        {
            get { return backgroundColor; }
            set { backgroundColor = value; }
        }

        public GameEngine (int tableSize)
        {
            backgroundColor = Color.GhostWhite;
            primaryColor = Color.CornflowerBlue;
            secondaryColor = Color.LightBlue;

            GLib.Timeout.Add (1000, new GLib.TimeoutHandler(TimerTick));
        }

        private bool TimerTick()
        {
            if (isRunning)
            {
                if (piecesLeft == 0)
                {
                    isRunning = false;
                    return true;
                }

                gameTime += 1;
                DrawInfoBar();
            }

            return true;
        }

        public void NewGame(int tableSize)
        {
            gameTable = new GameTable(tableSize);

            this.tableSize = tableSize;

            tileSpacing = 2;
            gameTime = 0;
            flips = 0;
            piecesLeft = (tableSize * tableSize) / 2;
            isRunning = true;

            DrawTable();
            DrawInfoBar();
        }

        public void DrawTable()
        {
            if (isRunning == false)
                return;

            int x, y;
            int width, height;
            int tileSize;
            Graphics gfx = Gtk.DotNet.Graphics.FromDrawable(gdkWindowTable);
            Rectangle recTable;
            Brush brBakcground = new SolidBrush(backgroundColor);
            Brush brUnflippedPiece = new SolidBrush(primaryColor);
            Brush brFlippedPiece = new SolidBrush(secondaryColor);

            gdkWindowTable.GetSize(out width, out height);
            tileSize = (width - tableSize * tileSpacing ) / tableSize;
            recTable = new Rectangle(0, 0, width, height);

            gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;

            gfx.FillRectangle(brBakcground, recTable);

            for (y = 0; y < tableSize; y++)
                for (x = 0; x < tableSize; x++)
                {
                    recTable = new Rectangle(
                        tileSpacing / 2 + x * tileSize + x * tileSpacing,
                        tileSpacing / 2 + y * tileSize + y * tileSpacing,
                        tileSize, tileSize);

                    if (gameTable.Pieces[x, y].IsFlipped)
                    {
                        gfx.FillRectangle(brFlippedPiece, recTable);

                        Bitmap bmpPicture = Gdk.Pixbuf.LoadFromResource(
                            "MemoryBlocks.Resources.Pieces.piece" +
                            gameTable.Pieces[x, y].Value + ".png").ToBitmap();
                        
                        gfx.DrawImage(bmpPicture, recTable);
                    }
                    else if (gameTable.Pieces[x, y].IsCleared == false)
                        gfx.FillRectangle(brUnflippedPiece, recTable);
                }

            gfx.Dispose();
        }

        public void DrawInfoBar()
        {
            if (isRunning == false)
                return;
            
            Graphics gfx = Gtk.DotNet.Graphics.FromDrawable(gdkWindowInfoBar);
            Brush brBackground = new SolidBrush(backgroundColor);
            Brush brForeground = new SolidBrush(primaryColor);

            int width, height, widthThird;
            gdkWindowInfoBar.GetSize(out width, out height);
            widthThird = width / 3;

            Rectangle recWhole = new Rectangle(0, 0, width, height);
            Rectangle recLeft = new Rectangle(0, 0, widthThird, height);
            Rectangle recMiddle = new Rectangle(widthThird, 0, widthThird, height);
            Rectangle recRight = new Rectangle(widthThird * 2, 0, widthThird, height);

            Font f = new Font("Sans", (int)(Math.Min(width, height) * 0.5), FontStyle.Regular);
            StringFormat strFormat = new StringFormat();
            strFormat.Alignment = StringAlignment.Center;
            strFormat.LineAlignment = StringAlignment.Center;

            string face = ":)";

            gfx.FillRectangle(brBackground, recWhole);

            gfx.DrawString(piecesLeft.ToString(), f, brForeground, recLeft, strFormat);
            gfx.DrawString(face, f, brForeground, recMiddle, strFormat);
            gfx.DrawString(string.Format("{0:00}:{1:00}", (gameTime / 60) % 60, gameTime % 60), f, brForeground, recRight, strFormat);

            gfx.Dispose();
        }

        public void FlipPiece(int x, int y)
        {
            if (gameTable.Pieces[x, y].IsCleared)
                return;

            if (gameTable.Pieces[x, y].IsFlipped)
            { // Unflip it
                gameTable.Pieces[x, y].IsFlipped = false;
                flips -= 1;
            }
            else
            { // Flip it
                if (flips == 2)
                    UnflipAllPieces();
                
                gameTable.Pieces[x, y].IsFlipped = true;
                flips += 1;
            }

            if (flips == 2)
                CheckFlipped();
        }

        private void CheckFlipped()
        {
            int flip1i = -1;
            int flip1j = -1;

            for (int i = 0; i < tableSize; i++)
                for (int j = 0; j < tableSize; j++)
                    if (gameTable.Pieces[i, j].IsFlipped)
                    {
                        if (flip1i == -1 && flip1j == -1)
                        {
                            flip1i = i;
                            flip1j = j;
                        }
                        else
                        {
                            if (gameTable.Pieces[flip1i, flip1j].Value == gameTable.Pieces[i, j].Value)
                            {
                                gameTable.Pieces[flip1i, flip1j].IsCleared = true;
                                gameTable.Pieces[flip1i, flip1j].IsFlipped = false;
                                gameTable.Pieces[i, j].IsCleared = true;
                                gameTable.Pieces[i, j].IsFlipped = false;

                                flips = 0; 
                                piecesLeft -= 1;
                            }
                            return;
                        }
                    }
        }

        private void UnflipAllPieces()
        {
            for (int i = 0; i < tableSize; i++)
                for (int j = 0; j < tableSize; j++)
                    gameTable.Pieces[i, j].IsFlipped = false;
            flips = 0;
        }
    }
}