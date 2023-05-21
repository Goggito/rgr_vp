using Avalonia.Controls.Presenters;
using Avalonia.Controls;
using ReactiveUI;
using System.Reactive;
using LogicSimulator.Views;
using LogicSimulator.Models;

namespace LogicSimulator.ViewModels {
    public class LauncherWindowViewModel: ViewModelBase {
        Window? me;
        private static readonly MainWindow mw = new();

        public LauncherWindowViewModel() {
            Create = ReactiveCommand.Create<Unit, Unit>(_ => { FuncCreate(); return new Unit(); });
            Open = ReactiveCommand.Create<Unit, Unit>(_ => { FuncOpen(); return new Unit(); });
            Exit = ReactiveCommand.Create<Unit, Unit>(_ => { FuncExit(); return new Unit(); });
        }
        public void AddWindow(Window lw) => me = lw;

        void FuncCreate() {
            var newy = map.filer.CreateProject();
            CurrentProj = newy;
            mw.Show();
            mw.Update();
            me?.Close();
        }
        void FuncOpen() {
            if (me == null) return;

            var selected = map.filer.SelectProjectFile(me);
            if (selected == null) return;

            CurrentProj = selected;
            mw.Show();
            mw.Update();
            me?.Close();
        }
        void FuncExit() {
            me?.Close();
            mw.Close();
        }

        public ReactiveCommand<Unit, Unit> Create { get; }
        public ReactiveCommand<Unit, Unit> Open { get; }
        public ReactiveCommand<Unit, Unit> Exit { get; }


        public static Project[] ProjectList { get => map.filer.GetSortedProjects(); }

        public void DTapped(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
            var src = (Control?) e.Source;

            if (src is ContentPresenter cp && cp.Child is Border bord) src = bord;
            if (src is Border bord2 && bord2.Child is TextBlock tb2) src = tb2;

            if (src is not TextBlock tb || tb.Tag is not Project proj) return;

            CurrentProj = proj;

            mw.Show();
            mw.Update();
            me?.Close();
        }

        /*
         * Для тестирования
         */

        public static MainWindow GetMW => mw;
    }
}