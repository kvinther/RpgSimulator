using System;
using System.Windows;
using System.Windows.Input;
using RpgSim;
using RpgSim.EventMessaging;

namespace RpgDevUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class GameWindow : Window
    {
        public GameProxy MyGameProxy { get; set; }
        private readonly Engine _engine;
        public GameWindow()
        {
            InitializeComponent();

            MyGameProxy = new GameProxy(MyGameWindow);
            var game = new Game("Demo");
            _engine = new Engine(game);
            _engine.ReloadCurrentLocation();
        }

        private void GameWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            MyGameProxy.GameWindow_OnKeyDown(sender, e);
        }
    }

    public static class MyCommands
    {
        public static readonly RoutedUICommand DoReloadLocation
            = new RoutedUICommand("Reload location", "DoReloadLocation", typeof(GameWindow));
    }
}
