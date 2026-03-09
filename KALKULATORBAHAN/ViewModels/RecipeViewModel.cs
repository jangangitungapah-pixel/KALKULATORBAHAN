using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reactive;

namespace KALKULATORBAHAN.ViewModels
{
    public class RecipeViewModel : ViewModelBase
    {
        private readonly Action<RecipeViewModel> _duplicateAction;
        private readonly Action<RecipeViewModel> _removeAction;
        private string _category = "Donat";
        private string _description = string.Empty;
        private decimal _laborCost = 0m;
        private string _name = "Resep Baru";
        private decimal _overheadCost = 0m;
        private decimal _packagingCost = 0m;
        private decimal _targetYieldQuantity = 20m;
        private int _updateAgeDays = 1;
        private decimal _yieldQuantity = 20m;
        private string _yieldUnit = "pcs";

        public RecipeViewModel(Action<RecipeViewModel> removeAction, Action<RecipeViewModel> duplicateAction)
        {
            _removeAction = removeAction;
            _duplicateAction = duplicateAction;

            Items = new ObservableCollection<RecipeIngredientItemViewModel>();
            Items.CollectionChanged += OnItemsCollectionChanged;

            RemoveCommand = ReactiveCommand.Create(() => _removeAction(this));
            DuplicateCommand = ReactiveCommand.Create(() => _duplicateAction(this));
        }

        public ObservableCollection<RecipeIngredientItemViewModel> Items { get; }

        public ReactiveCommand<Unit, Unit> RemoveCommand { get; }

        public ReactiveCommand<Unit, Unit> DuplicateCommand { get; }

        public string Name
        {
            get => _name;
            set
            {
                this.RaiseAndSetIfChanged(ref _name, value);
                Touch();
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

        public string Description
        {
            get => _description;
            set
            {
                this.RaiseAndSetIfChanged(ref _description, value);
                Touch();
            }
        }

        public decimal YieldQuantity
        {
            get => _yieldQuantity;
            set
            {
                var sanitized = Math.Max(0m, value);
                this.RaiseAndSetIfChanged(ref _yieldQuantity, sanitized);
                Touch();
                RaiseAggregatePropertiesChanged();
            }
        }

        public string YieldUnit
        {
            get => _yieldUnit;
            set
            {
                this.RaiseAndSetIfChanged(ref _yieldUnit, value);
                Touch();
            }
        }

        public decimal TargetYieldQuantity
        {
            get => _targetYieldQuantity;
            set
            {
                var sanitized = Math.Max(0m, value);
                this.RaiseAndSetIfChanged(ref _targetYieldQuantity, sanitized);
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
                Touch();
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
                Touch();
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
                Touch();
                RaiseAggregatePropertiesChanged();
            }
        }

        public int UpdateAgeDays
        {
            get => _updateAgeDays;
            set
            {
                var sanitized = Math.Max(0, value);
                this.RaiseAndSetIfChanged(ref _updateAgeDays, sanitized);
                this.RaisePropertyChanged(nameof(UpdatedLabel));
            }
        }

        public int ItemCount => Items.Count;

        public decimal BaseCost
        {
            get
            {
                decimal total = 0m;

                foreach (var item in Items)
                {
                    total += item.UsageCost;
                }

                return Math.Round(total, 2);
            }
        }

        public decimal AdditionalCost => Math.Round(PackagingCost + LaborCost + OverheadCost, 2);

        public decimal TotalCost => Math.Round(BaseCost + AdditionalCost, 2);

        public decimal CostPerUnit => YieldQuantity <= 0m ? 0m : Math.Round(TotalCost / YieldQuantity, 2);

        public decimal ScaleFactor => YieldQuantity <= 0m ? 0m : Math.Round(TargetYieldQuantity / YieldQuantity, 4);

        public decimal ScaledCost => Math.Round(TotalCost * ScaleFactor, 2);

        public string UpdatedLabel => UpdateAgeDays <= 1 ? "1 hari" : $"{UpdateAgeDays} hari";

        public bool IsValidForCosting => ItemCount > 0 && YieldQuantity > 0m;

        public void AddIngredient(IngredientCostItemViewModel ingredient, decimal usageQuantity)
        {
            var item = new RecipeIngredientItemViewModel(ingredient, RemoveIngredient)
            {
                UsageQuantity = usageQuantity,
            };

            Items.Add(item);
            Touch();
        }

        public void MoveIngredient(RecipeIngredientItemViewModel sourceItem, RecipeIngredientItemViewModel targetItem)
        {
            var sourceIndex = Items.IndexOf(sourceItem);
            var targetIndex = Items.IndexOf(targetItem);

            if (sourceIndex < 0 || targetIndex < 0 || sourceIndex == targetIndex)
            {
                return;
            }

            Items.Move(sourceIndex, targetIndex);
            Touch();
        }

        public RecipeViewModel Clone()
        {
            var clone = new RecipeViewModel(_removeAction, _duplicateAction)
            {
                Name = $"{Name} Copy",
                Category = Category,
                Description = Description,
                YieldQuantity = YieldQuantity,
                YieldUnit = YieldUnit,
                TargetYieldQuantity = TargetYieldQuantity,
                PackagingCost = PackagingCost,
                LaborCost = LaborCost,
                OverheadCost = OverheadCost,
                UpdateAgeDays = 0,
            };

            foreach (var item in Items)
            {
                clone.AddIngredient(item.Ingredient, item.UsageQuantity);
            }

            return clone;
        }

        public void SetSeedUpdateAge(int updateAgeDays)
        {
            UpdateAgeDays = updateAgeDays;
        }

        private void RemoveIngredient(RecipeIngredientItemViewModel item)
        {
            item.Detach();
            Items.Remove(item);
            Touch();
        }

        private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems is not null)
            {
                foreach (RecipeIngredientItemViewModel item in e.NewItems)
                {
                    item.PropertyChanged += OnRecipeItemPropertyChanged;
                }
            }

            if (e.OldItems is not null)
            {
                foreach (RecipeIngredientItemViewModel item in e.OldItems)
                {
                    item.PropertyChanged -= OnRecipeItemPropertyChanged;
                }
            }

            RaiseAggregatePropertiesChanged();
        }

        private void OnRecipeItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            Touch();
            RaiseAggregatePropertiesChanged();
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

        private void RaiseAggregatePropertiesChanged()
        {
            this.RaisePropertyChanged(nameof(ItemCount));
            this.RaisePropertyChanged(nameof(BaseCost));
            this.RaisePropertyChanged(nameof(AdditionalCost));
            this.RaisePropertyChanged(nameof(TotalCost));
            this.RaisePropertyChanged(nameof(CostPerUnit));
            this.RaisePropertyChanged(nameof(ScaleFactor));
            this.RaisePropertyChanged(nameof(ScaledCost));
            this.RaisePropertyChanged(nameof(IsValidForCosting));
        }
    }
}
