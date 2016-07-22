using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Documents;
using System.Windows.Media;
using RpgDevUI.Annotations;
using RpgSim;
using RpgSim.EventMessaging;
using RpgSim.Events;

namespace RpgDevUI
{
    public partial class GameProxy : INotifyPropertyChanged
    {
        private string _name;
        private string _id;
        private string _tvText;

        public string Id
        {
            get { return _id; }
            set
            {
                if (value == _id) return;
                _id = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if (value == _name) return;
                _name = value;
                OnPropertyChanged();
            }
        }

        public string TvText
        {
            get { return _tvText; }
            set
            {
                if (value == _tvText) return;
                _tvText = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public partial class GameProxy
    {
        private readonly GameWindow _myGameWindow;
        private readonly IEventMessagingSystem _messenger;
        public GameProxy(GameWindow myGameWindow)
        {
            _myGameWindow = myGameWindow;
            _messenger = EventMessagingSystem.Messenger;
            RegisterEvents();
        }

        private void RegisterEvents()
        {
            _messenger.Register(MessageEnum.LocationChanged, OnEvent);
        }

        public void OnEvent(IEventMessage message)
        {
            var targetType = message.GetType();
            var methods = GetType().GetMethodsBySig(typeof (void), targetType).ToArray();
            foreach (var method in methods)
                method.Invoke(this, new object[] {message});
        }

        public void NewLocation(LocationChangedEventMessage message)
        {
            var l = message.Location;
            var lines = _myGameWindow.ServerText.Inlines;
            lines.Clear();

            lines.Add(new Run(l.Name.Trim()) { Foreground = Brushes.Cyan });
            lines.Add(new LineBreak());
            lines.Add(new Run(l.Description.Trim()) { Foreground = Brushes.White });
            lines.Add(new LineBreak());
            foreach (var container in l.Containers)
            {
                lines.Add(new Run(container.Description.Trim()) { Foreground = Brushes.White });
                lines.Add(new LineBreak());
            }

            foreach (var item in l.Items)
            {
                lines.Add(new Run(item.Description.Trim()) { Foreground = Brushes.Yellow});
                lines.Add(new LineBreak());
            }

            lines.Add(new Run("Exits are:") { Foreground = Brushes.Red });
            lines.Add(new LineBreak());

            foreach (var exit in l.Exits)
            {
                lines.Add(new Run("    " + (exit.Description ?? exit.Name).Trim()) { Foreground = Brushes.Red });
                lines.Add(new LineBreak());
            }

            _myGameWindow.Player1Panel.Children.Clear();
            foreach (var exit in l.Exits)
            {
                var button = new Button
                {
                    Content = "Exit to: " + exit.Name,
                    Tag = exit.DestinationId
                };

                button.Click += (sender, args) =>
                {
                    _messenger.Post(new PlayerMoveMessage(((Button)sender).Tag.ToString()));
                };

                _myGameWindow.Player1Panel.Children.Add(button);
            }
            
        }

        public void GameWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            Debug.WriteLine("Key pressed: " + e.Key);

            switch (e.Key)
            {
                case Key.F5:
                    _messenger.Post(MessageEnum.ReloadCurrentLocation);
                    break;
            }
        }
    }
}
