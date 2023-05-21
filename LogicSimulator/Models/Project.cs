using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LogicSimulator.Models {
    public class Project: IComparable {
        public string Name { get; private set; }
        public long Created;
        public long Modified;

        public ObservableCollection<Scheme> schemes = new();
        public string? FileDir { get; private set; }
        public string? FileName { get; set; }

        private readonly FileHandler parent;

        public Project(FileHandler parent) { // Новый проект
            this.parent = parent;
            Name = "Новый проект";
            Created = Modified = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            FileDir = null;
            FileName = null; // FileHandler.GetProjectFileName();
            CreateScheme();
        }

        public Project(FileHandler parent, string dir, string fileName, object data) { // Импорт
            this.parent = parent;
            FileDir = dir;
            FileName = fileName;

            if (data is not Dictionary<string, object> dict) throw new Exception("Ожидался словарь в корне проекта");
            
            if (!dict.TryGetValue("name", out var value)) throw new Exception("В проекте нет имени");
            if (value is not string name) throw new Exception("Тип имени проекта - не строка");
            Name = name;

            if (!dict.TryGetValue("created", out var value2)) throw new Exception("В проекте нет времени создания");
            if (value2 is not int create_t) throw new Exception("Время создания проекта - не строка");
            Created = create_t;

            if (!dict.TryGetValue("modified", out var value3)) throw new Exception("В проекте нет времени изменения");
            if (value3 is not int mod_t) throw new Exception("Время изменения проекта - не строка");
            Modified = mod_t;

            if (!dict.TryGetValue("schemes", out var value4)) throw new Exception("В проекте нет списка схем");
            if (value4 is not List<object> arr) throw new Exception("Списко схем проекта - не массив строк");
            foreach (var s_data in arr) {
                if (s_data == null) throw new Exception("Одно из файловых имёт списка схем проекта - null");
                var scheme = new Scheme(this, s_data);
                schemes.Add(scheme);
            }
        }



        public Scheme CreateScheme() {
            var scheme = new Scheme(this);
            schemes.Add(scheme);
            Save();
            return scheme;
        }
        public Scheme AddScheme(Scheme? prev) {
            var scheme = new Scheme(this);
            int pos = prev == null ? 0 : schemes.IndexOf(prev) + 1;
            schemes.Insert(pos, scheme);
            Save();
            return scheme;
        }
        public void RemoveScheme(Scheme me) {
            schemes.Remove(me);
            Save();
        }
        public void UpdateList() {
            foreach (var scheme in schemes) scheme.UpdateProps();
        }

        public Scheme GetFirstScheme() => schemes[0];



        public object Export() {
            return new Dictionary<string, object> {
                ["name"] = Name,
                ["created"] = Created,
                ["modified"] = Modified,
                ["schemes"] = schemes.Select(x => x.Export()).ToArray(),
            };
        }

        public void Save() => FileHandler.SaveProject(this);

        public int CompareTo(object? obj) {
            if (obj is not Project proj) throw new ArgumentNullException(nameof(obj));
            return (int)(proj.Modified - Modified);
        }

        public override string ToString() {
            return Name + "\nИзменён: " + Modified.UnixTimeStampToString() + "\nСоздан: " + Created.UnixTimeStampToString();
        }

        internal void ChangeName(string name) {
            Name = name;
            Modified = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            Save();
        }

        public bool CanSave() => FileDir != null;
        public void SaveAs(Window mw) {
            FileDir = FileHandler.RequestProjectPath(mw);
            Save();
            parent.AppendProject(this);
        }

        /*
         * Для тестирования
         */

        public void SetDir(string path) => FileDir = path;
    }
}
