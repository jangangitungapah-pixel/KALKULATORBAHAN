using ReactiveUI;
using System;
using System.ComponentModel;
using System.Reactive;

namespace KALKULATORBAHAN.ViewModels
{
    public class ProductRecipeComponentViewModel : ViewModelBase
    {
        private readonly Action<ProductRecipeComponentViewModel> _removeAction;
        private decimal _multiplier = 1m;

        public ProductRecipeComponentViewModel(
            RecipeViewModel recipe,
            Action<ProductRecipeComponentViewModel> removeAction)
        {
            Recipe = recipe;
            _removeAction = removeAction;
            RemoveCommand = ReactiveCommand.Create(() => _removeAction(this));

            Recipe.PropertyChanged += OnRecipePropertyChanged;
        }

        public RecipeViewModel Recipe { get; }

        public ReactiveCommand<Unit, Unit> RemoveCommand { get; }

        public string RecipeName => Recipe.Name;

        public string YieldSummary => $"{Recipe.YieldQuantity:N0} {Recipe.YieldUnit}";

        public decimal BaseRecipeCost => Recipe.TotalCost;

        public decimal Multiplier
        {
            get => _multiplier;
            set
            {
                var sanitized = Math.Max(0m, value);
                this.RaiseAndSetIfChanged(ref _multiplier, sanitized);
                this.RaisePropertyChanged(nameof(ComponentCost));
            }
        }

        public decimal ComponentCost => Math.Round(BaseRecipeCost * Multiplier, 2);

        public void Detach()
        {
            Recipe.PropertyChanged -= OnRecipePropertyChanged;
        }

        private void OnRecipePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            this.RaisePropertyChanged(nameof(RecipeName));
            this.RaisePropertyChanged(nameof(YieldSummary));
            this.RaisePropertyChanged(nameof(BaseRecipeCost));
            this.RaisePropertyChanged(nameof(ComponentCost));
        }
    }
}
