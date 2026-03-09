using ReactiveUI;
using System;
using System.Reactive;

namespace KALKULATORBAHAN.ViewModels
{
    public class IngredientCostItemViewModel : ViewModelBase
    {
        private readonly Action<IngredientCostItemViewModel> _removeAction;
        private string _category = "Bahan Utama";
        private string _name = string.Empty;
        private decimal _packPrice;
        private decimal _packQuantity = 1000m;
        private string _supplierName = "Supplier utama";
        private string _unitName = "gram";
        private int _updateAgeDays = 1;
        private int _usedInCount;

        public IngredientCostItemViewModel(Action<IngredientCostItemViewModel> removeAction)
        {
            _removeAction = removeAction;
            RemoveCommand = ReactiveCommand.Create(() => _removeAction(this));
        }

        public ReactiveCommand<Unit, Unit> RemoveCommand { get; }

        public string Name
        {
            get => _name;
            set
            {
                this.RaiseAndSetIfChanged(ref _name, value);
                Touch();
                RaiseDerivedPropertiesChanged();
            }
        }

        public string Category
        {
            get => _category;
            set
            {
                this.RaiseAndSetIfChanged(ref _category, value);
                Touch();
            }
        }

        public string UnitName
        {
            get => _unitName;
            set
            {
                this.RaiseAndSetIfChanged(ref _unitName, value);
                Touch();
                RaiseDerivedPropertiesChanged();
            }
        }

        public string SupplierName
        {
            get => _supplierName;
            set
            {
                this.RaiseAndSetIfChanged(ref _supplierName, value);
                Touch();
            }
        }

        public decimal PackPrice
        {
            get => _packPrice;
            set
            {
                var sanitized = Math.Max(0m, value);
                this.RaiseAndSetIfChanged(ref _packPrice, sanitized);
                Touch();
                RaiseDerivedPropertiesChanged();
            }
        }

        public decimal PackQuantity
        {
            get => _packQuantity;
            set
            {
                var sanitized = Math.Max(0m, value);
                this.RaiseAndSetIfChanged(ref _packQuantity, sanitized);
                Touch();
                RaiseDerivedPropertiesChanged();
            }
        }

        public int UsedInCount
        {
            get => _usedInCount;
            set
            {
                var sanitized = Math.Max(0, value);
                if (_usedInCount == sanitized)
                {
                    return;
                }

                this.RaiseAndSetIfChanged(ref _usedInCount, sanitized);
                this.RaisePropertyChanged(nameof(UsedInSummary));
            }
        }

        public int UpdateAgeDays
        {
            get => _updateAgeDays;
            set
            {
                var sanitized = Math.Max(0, value);
                if (_updateAgeDays == sanitized)
                {
                    return;
                }

                this.RaiseAndSetIfChanged(ref _updateAgeDays, sanitized);
                this.RaisePropertyChanged(nameof(UpdatedLabel));
            }
        }

        public decimal CostPerUnit =>
            PackQuantity <= 0m ? 0m : Math.Round(PackPrice / PackQuantity, 4);

        public string PackSummary => $"{PackQuantity:N2} {UnitName}";

        public string UsedInSummary => UsedInCount == 1 ? "1 resep" : $"{UsedInCount} resep";

        public string UpdatedLabel => UpdateAgeDays <= 1 ? "1 hari" : $"{UpdateAgeDays} hari";

        public bool IsValidForCosting => !string.IsNullOrWhiteSpace(Name) && PackQuantity > 0m && PackPrice > 0m;

        public void SetSeedMetadata(int usedInCount, int updateAgeDays)
        {
            _usedInCount = Math.Max(0, usedInCount);
            _updateAgeDays = Math.Max(0, updateAgeDays);
            this.RaisePropertyChanged(nameof(UsedInCount));
            this.RaisePropertyChanged(nameof(UsedInSummary));
            this.RaisePropertyChanged(nameof(UpdateAgeDays));
            this.RaisePropertyChanged(nameof(UpdatedLabel));
        }

        private void Touch()
        {
            if (_updateAgeDays != 0)
            {
                _updateAgeDays = 0;
                this.RaisePropertyChanged(nameof(UpdateAgeDays));
                this.RaisePropertyChanged(nameof(UpdatedLabel));
            }
        }

        private void RaiseDerivedPropertiesChanged()
        {
            this.RaisePropertyChanged(nameof(CostPerUnit));
            this.RaisePropertyChanged(nameof(PackSummary));
            this.RaisePropertyChanged(nameof(IsValidForCosting));
        }
    }
}
