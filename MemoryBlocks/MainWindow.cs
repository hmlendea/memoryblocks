using System;
using Gtk;

using MemoryBlocks.Game;

namespace MemoryBlocks
{
    public partial class MainWindow: Gtk.Window
    {
        GameEngine game;

        public MainWindow() : base(Gtk.WindowType.Toplevel)
        {
            this.Build();
            daTable.DoubleBuffered = false;

            game = new GameEngine(6);
            game.GdkWindowTable = daTable.GdkWindow;
            game.GdkWindowInfoBar = daInfoBar.GdkWindow;
            game.NewGame(6);

            daTable.ExposeEvent += delegate { game.DrawTable(); };
            daInfoBar.ExposeEvent += delegate { game.DrawInfoBar(); };
            daTable.AddEvents ((int) Gdk.EventMask.ButtonPressMask);
        }

        protected void OnDaTableButtonPressEvent(object o, ButtonPressEventArgs args)
        {
            if (game.IsRunning)
            {
                int tileSize = daTable.GdkWindow.GetWidth() / game.TableSize;
                int x = (int)args.Event.X / tileSize;
                int y = (int)args.Event.Y / tileSize;

                if (args.Event.Button == 1)
                { // Left mouse click
                    game.FlipPiece(x, y);
                }
                else if (args.Event.Button == 3)
                { // Right mouse click
                    
                }

                game.DrawTable();
                game.DrawInfoBar();

                if (game.Completed)
                    ShowDialog("Level complete", "You have successfully cleared this level!", MessageType.Info, ButtonsType.Ok);
            }
        }

        public void ShowDialog(string title, string message, MessageType msgType, ButtonsType btnType)
        {
            MessageDialog md = new MessageDialog(null, DialogFlags.Modal, msgType, btnType, message);
            md.Title = title;
            md.Run ();
            md.Destroy();
        }

        protected void OnDeleteEvent(object sender, DeleteEventArgs a)
        {
            Application.Quit();
            a.RetVal = true;
        }

        protected void OnActEasyActivated(object sender, EventArgs e)
        {
            game.NewGame(4);
        }
        protected void OnActModerateActivated(object sender, EventArgs e)
        {
            game.NewGame(6);
        }
        protected void OnActHardActivated(object sender, EventArgs e)
        {
            game.NewGame(8);
        }
        protected void OnActRetryActivated(object sender, EventArgs e)
        {
            game.NewGame(game.TableSize);
        }

        protected void OnDiAboutActivated(object sender, EventArgs e)
        {
            ShowDialog("About", "Made by Mlendea Horațiu", MessageType.Info, ButtonsType.Ok);
        }
    }
}
