using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reactive;

namespace KALKULATORBAHAN.ViewModels
{
    public class ProductViewModel : ViewModelBase
    {
        private readonly Action<ProductViewModel> _removeAction;
        private string _category = "Produk Utama";
        private decimal _defaultMarginPercent = 40m;
        private decimal _laborCost = 1000m;
        private string _name = "Produk Baru";
        private decimal _outputUnits = 20m;
        private decimal _overheadCost = 1000m;
        private decimal _packagingCost = 500m;

        public ProductViewModel(Action<ProductViewModel> removeAction)
        {
            _removeAction = removeAction;
            Components = new ObservableCollection<ProductRecipeComponentViewModel>();
            Components.CollectionChanged += OnComponentsCollectionChanged;
            RemoveCommand = ReactiveCommand.Create(() => _removeAction(this));
        }

        public ObservableCollection<ProductRecipeComponentViewModel> Components { get; }

        public ReactiveCommand<Unit, Unit> RemoveCommand { get; }

        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public string Category
        {
            get => _category;
            set => this.RaiseAndSetIfChanged(ref _category, value);
        }

        public decimal OutputUnits
        {
            get => _outputUnits;
            set
            {
                var sanitized = Math.Max(0m, value);
                this.RaiseAndSetIfChanged(ref _outputUnits, sanitized);
                RaiseAggregatePropertiesChanged();
            }
        }

        public decimal PackagingCost
        {
            get => _packagingCost;
            set
            {
                var sanitized = Math.Max(0m, value);
                this.RaiseAndSetIfChanged(ref _packagingCost, sanitized);
                RaiseAggregatePropertiesChanged();
            }
        }

        public decimal LaborCost
        {
            get => _laborCost;
            set
            {
                var sanitized = Math.Max(0m, value);
                this.RaiseAndSetIfChanged(ref _laborCost, sanitized);
                RaiseAggregatePropertiesChanged();
            }
        }

        public decimal OverheadCost
        {
            get => _overheadCost;
            set
            {
                var sanitized = Math.Max(0m, value);
                this.RaiseAndSetIfChanged(ref _overheadCost, sanitized);
                RaiseAggregatePropertiesChanged();
            }
        }

        public decimal DefaultMarginPercent
        {
            get => _defaultMarginPercent;
            set
            {
                var sanitized = Math.Clamp(value, 0m, 95m);
                this.RaiseAndSetIfChanged(ref _defaultMarginPercent, sanitized);
                RaiseAggregatePropertiesChanged();
            }
        }

        public string PrimaryRecipeName => Components.Count == 0 ? "-" : Components[0].RecipeName;

        public string MarginLabel => $"{DefaultMarginPercent:N0}%";

        public decimal IngredientCost
        {
            get
            {
                decimal total = 0m;

                foreach (var component in Components)
                {
                    total += component.ComponentCost;
                }

                return Math.Round(total, 2);
            }
        }

        public decimal TotalCost => Math.Round(IngredientCost + PackagingCost + LaborCost + OverheadCost, 2);

        public decimal HppPerUnit => OutputUnits <= 0m ? 0m : Math.Round(TotalCost / OutputUnits, 2);

        public decimal SuggestedSellingPrice =>
            DefaultMarginPercent >= 100m ? 0m : Math.Round(HppPerUnit / (1m - (DefaultMarginPercent / 100m)), 2);

        public decimal ProfitPerUnit => Math.Round(SuggestedSellingPrice - HppPerUnit, 2);

        public void AddComponent(RecipeViewModel recipe, decimal multiplier)
        {
            var component = new ProductRecipeComponentViewModel(recipe, RemoveComponent)
            {
                Multiplier = multiplier,
            };

            Components.Add(component);
        }

        private void RemoveComponent(ProductRecipeComponentViewModel component)
        {
            component.Detach();
            Components.Remove(component);
        }

        private void OnComponentsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems is not null)
            {
                foreach (ProductRecipeComponentViewModel component in e.NewItems)
                {
                    component.PropertyChanged += OnComponentPropertyChanged;
                }
            }

            if (e.OldItems is not null)
            {
                foreach (ProductRecipeComponentViewModel component in e.OldItems)
                {
                    component.PropertyChanged -= OnComponentPropertyChanged;
                }
            }

            RaiseAggregatePropertiesChanged();
        }

        private void OnComponentPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            RaiseAggregatePropertiesChanged();
        }

        private void RaiseAggregatePropertiesChanged()
        {
            this.RaisePropertyChanged(nameof(PrimaryRecipeName));
            this.RaisePropertyChanged(nameof(MarginLabel));
            this.RaisePropertyChanged(nameof(IngredientCost));
            this.RaisePropertyChanged(nameof(TotalCost));
            this.RaisePropertyChanged(nameof(HppPerUnit));
            this.RaisePropertyChanged(nameof(SuggestedSellingPrice));
            this.RaisePropertyChanged(nameof(ProfitPerUnit));
        }
    }
}
