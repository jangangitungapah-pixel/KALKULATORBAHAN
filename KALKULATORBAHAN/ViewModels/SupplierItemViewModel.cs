using ReactiveUI;
using System;
using System.Reactive;

namespace KALKULATORBAHAN.ViewModels
{
    public class SupplierItemViewModel : ViewModelBase
    {
        private readonly Action<SupplierItemViewModel> _removeAction;
        private string _averagePriceBand = "Medium";
        private int _ingredientCount;
        private int _leadTimeDays = 2;
        private string _name = "Supplier Baru";
        private string _notes = string.Empty;
        private string _phone = string.Empty;
        private string _ratingStars = "????";

        public SupplierItemViewModel(Action<SupplierItemViewModel> removeAction)
        {
            _removeAction = removeAction;
            RemoveCommand = ReactiveCommand.Create(() => _removeAction(this));
        }

        public ReactiveCommand<Unit, Unit> RemoveCommand { get; }

        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public string Phone
        {
            get => _phone;
            set => this.RaiseAndSetIfChanged(ref _phone, value);
        }

        public int LeadTimeDays
        {
            get => _leadTimeDays;
            set => this.RaiseAndSetIfChanged(ref _leadTimeDays, Math.Max(0, value));
        }

        public string Notes
        {
            get => _notes;
            set => this.RaiseAndSetIfChanged(ref _notes, value);
        }

        public int IngredientCount
        {
            get => _ingredientCount;
            set => this.RaiseAndSetIfChanged(ref _ingredientCount, Math.Max(0, value));
        }

        public string AveragePriceBand
        {
            get => _averagePriceBand;
            set => this.RaiseAndSetIfChanged(ref _averagePriceBand, value);
        }

        public string RatingStars
        {
            get => _ratingStars;
            set => this.RaiseAndSetIfChanged(ref _ratingStars, value);
        }
    }
}
