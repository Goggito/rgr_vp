using LogicSimulator.ViewModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive;

namespace LogicSimulator.Models {
    public class Scheme : ReactiveObject {
        public string Name { get; set; }
        public long Created;
        public long Modified;

        public object[] items;
        public object[] joins;
        public string states;

        private readonly Project parent;

        public Scheme(Project p) { // Новая схема
            Created = Modified = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            Name = "Newy";
            items = joins = Array.Empty<object>();
            states = "0";
            parent = p;

            Open = ReactiveCommand.Create<Unit, Unit>(_ => { FuncOpen(); return new Unit(); });
            NewItem = ReactiveCommand.Create<Unit, Unit>(_ => { FuncNewItem(); return new Unit(); });
            Delete = ReactiveCommand.Create<Unit, Unit>(_ => { FuncDelete(); return new Unit(); });
        }

        public Scheme(Project p, object data) { // Импорт
            parent = p;

            if (data is not Dictionary<string, object> dict) throw new Exception("Ожидался словарь в корне схемы");

            if (!dict.TryGetValue("name", out var value)) throw new Exception("В схеме нет имени");
            if (value is not string name) throw new Exception("Тип имени схемы - не строка");
            Name = name;

            if (!dict.TryGetValue("created", out var value2)) throw new Exception("В схеме нет времени создания");
            if (value2 is not int create_t) throw new Exception("Время создания схемы - не строка");
            Created = create_t;

            if (!dict.TryGetValue("modified", out var value3)) throw new Exception("В схеме нет времени изменения");
            if (value3 is not int mod_t) throw new Exception("Время изменения схемы - не строка");
            Modified = mod_t;

            if (!dict.TryGetValue("items", out var value4)) throw new Exception("В схеме нет списка элементов");
            if (value4 is not List<object> arr) throw new Exception("Список элементов схемы - не массив объектов");
            items = arr.ToArray();

            if (!dict.TryGetValue("joins", out var value5)) throw new Exception("В схеме нет списка соединений");
            if (value5 is not List<object> arr2) throw new Exception("Список соединений схемы - не массив объектов");
            joins = arr2.ToArray();

            if (!dict.TryGetValue("states", out var value6)) throw new Exception("В схеме нет списка состояний");
            if (value6 is not string arr3) throw new Exception("Список состояний схемы - не строка");
            states = arr3;

            Open = ReactiveCommand.Create<Unit, Unit>(_ => { FuncOpen(); return new Unit(); });
            NewItem = ReactiveCommand.Create<Unit, Unit>(_ => { FuncNewItem(); return new Unit(); });
            Delete = ReactiveCommand.Create<Unit, Unit>(_ => { FuncDelete(); return new Unit(); });
        }

        public void Update(object[] items, object[] joins, string states) {
            this.items = items;
            this.joins = joins;
            this.states = states;
            Modified = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            Update();
        }



        public object Export() {
            return new Dictionary<string, object> {
                ["name"] = Name,
                ["created"] = Created,
                ["modified"] = Modified,
                ["items"] = items,
                ["joins"] = joins,
                ["states"] = states,
            };
        }
        public void Update() {
            Modified = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            parent.Modified = Modified;
            parent.Save();
        }

        public override string ToString() => Name;

        internal void ChangeName(string name) {
            Name = name;
            Update();
        }

        /*
         * Кнопоки
         */

        void FuncOpen() {
            ViewModelBase.map.current_scheme = this;
            ViewModelBase.map.ImportScheme();
            parent.UpdateList();
        }
        void FuncNewItem() {
            parent.AddScheme(this);
            parent.UpdateList();
        }
        void FuncDelete() {
            parent.RemoveScheme(this);
            parent.UpdateList();
        }

        public ReactiveCommand<Unit, Unit> Open { get; }
        public ReactiveCommand<Unit, Unit> NewItem { get; }
        public ReactiveCommand<Unit, Unit> Delete { get; }

        public bool CanUseSchemeDeleter { get => parent.schemes.Count > 1; }
        public bool CanOpenMe { get => ViewModelBase.map.current_scheme != this; }

        public void UpdateProps() {
            this.RaisePropertyChanged(nameof(CanUseSchemeDeleter));
            this.RaisePropertyChanged(nameof(CanOpenMe));
        }
    }
}
