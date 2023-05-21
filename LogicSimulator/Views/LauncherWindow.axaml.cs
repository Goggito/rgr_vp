using Avalonia.Controls;
using LogicSimulator.ViewModels;

namespace LogicSimulator.Views {
    public partial class LauncherWindow: Window {
        readonly LauncherWindowViewModel lwvm;

        public LauncherWindow() {
            InitializeComponent();
            lwvm = new LauncherWindowViewModel();
            DataContext = lwvm;
            lwvm.AddWindow(this);
        }

        public void DTapped(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
            lwvm.DTapped(sender, e);
        }
    }
}