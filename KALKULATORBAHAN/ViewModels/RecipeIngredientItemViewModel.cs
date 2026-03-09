using ReactiveUI;
using System;
using System.ComponentModel;
using System.Reactive;

namespace KALKULATORBAHAN.ViewModels
{
    public class RecipeIngredientItemViewModel : ViewModelBase
    {
        private readonly Action<RecipeIngredientItemViewModel> _removeAction;
        private decimal _usageQuantity;

        public RecipeIngredientItemViewModel(
            IngredientCostItemViewModel ingredient,
            Action<RecipeIngredientItemViewModel> removeAction)
        {
            Ingredient = ingredient;
            _removeAction = removeAction;
            RemoveCommand = ReactiveCommand.Create(() => _removeAction(this));

            Ingredient.PropertyChanged += OnIngredientPropertyChanged;
        }

        public IngredientCostItemViewModel Ingredient { get; }

        public ReactiveCommand<Unit, Unit> RemoveCommand { get; }

        public string IngredientName => Ingredient.Name;

        public string Category => Ingredient.Category;

        public string UnitName => Ingredient.UnitName;

        public decimal PackPrice => Ingredient.PackPrice;

        public decimal PackQuantity => Ingredient.PackQuantity;

        public decimal CostPerUnit => Ingredient.CostPerUnit;

        public decimal UsageQuantity
        {
            get => _usageQuantity;
            set
            {
                var sanitized = Math.Max(0m, value);
                this.RaiseAndSetIfChanged(ref _usageQuantity, sanitized);
                this.RaisePropertyChanged(nameof(UsageCost));
                this.RaisePropertyChanged(nameof(UsageSummary));
                this.RaisePropertyChanged(nameof(IsValidForCosting));
            }
        }

        public decimal UsageCost => Math.Round(CostPerUnit * UsageQuantity, 2);

        public string UsageSummary => $"{UsageQuantity:N2} {UnitName}";

        public bool IsValidForCosting => Ingredient.IsValidForCosting && UsageQuantity > 0m;

        public void Detach()
        {
            Ingredient.PropertyChanged -= OnIngredientPropertyChanged;
        }

        private void OnIngredientPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            this.RaisePropertyChanged(nameof(IngredientName));
            this.RaisePropertyChanged(nameof(Category));
            this.RaisePropertyChanged(nameof(UnitName));
            this.RaisePropertyChanged(nameof(PackPrice));
            this.RaisePropertyChanged(nameof(PackQuantity));
            this.RaisePropertyChanged(nameof(CostPerUnit));
            this.RaisePropertyChanged(nameof(UsageCost));
            this.RaisePropertyChanged(nameof(UsageSummary));
            this.RaisePropertyChanged(nameof(IsValidForCosting));
        }
    }
}
