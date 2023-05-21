using LogicSimulator.Models;
using ReactiveUI;

namespace LogicSimulator.ViewModels {
    public class ViewModelBase: ReactiveObject {
        public readonly static Mapper map = new();
        private static Project? current_proj;
        protected static Project? CurrentProj {
            get => current_proj;
            set {
                if (value == null) return;
                current_proj = value;
                map.current_scheme = value.GetFirstScheme();
            }
        }

        /*
         * Для тестирования
         */

        public static Project? TopSecretGetProj() => current_proj;
    }
}