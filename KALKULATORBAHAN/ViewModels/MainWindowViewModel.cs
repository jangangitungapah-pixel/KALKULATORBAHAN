using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace KALKULATORBAHAN.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private static readonly ObservableCollection<RecipeIngredientItemViewModel> EmptyRecipeItems = new();

        private string _businessName = "Donat Mabes";
        private string _businessType = "Bakery & Snack";
        private string _currency = "IDR";
        private string _currentSection = "dashboard";
        private string _defaultUnit = "gram";
        private string _globalSearchText = string.Empty;
        private string _ingredientSearchText = string.Empty;
        private bool _isNavigationPaneOpen = true;
        private bool _isOnboardingVisible = true;
        private bool _isQuickAddOpen;
        private decimal _marginPercentInput = 40m;
        private decimal _markupPercentInput = 50m;
        private int _onboardingStep;
        private decimal _plannerTargetProduction = 200m;
        private string _pricingMethod = "Margin";
        private string _recipeName = "Donat Original";
        private string _reportFormat = "PDF";
        private decimal _targetProfitPerUnitInput = 500m;
        private string _themeMode = "System";
        private decimal _wastePercentage = 5m;
        private IngredientCostItemViewModel? _selectedCatalogIngredient;
        private ProductViewModel? _selectedProduct;
        private RecipeViewModel? _selectedRecipe;
        private RecipeViewModel? _selectedRecipeForProduct;
        private SupplierItemViewModel? _selectedSupplier;

        public MainWindowViewModel()
        {
            Ingredients = new ObservableCollection<IngredientCostItemViewModel>();
            Recipes = new ObservableCollection<RecipeViewModel>();
            Products = new ObservableCollection<ProductViewModel>();
            Suppliers = new ObservableCollection<SupplierItemViewModel>();
            PricingMethods = new ObservableCollection<string>(new[] { "Margin", "Markup", "Target Profit" });
            ReportFormats = new ObservableCollection<string>(new[] { "PDF", "Excel", "CSV" });
            ThemeModes = new ObservableCollection<string>(new[] { "System", "Light", "Dark" });

            Ingredients.CollectionChanged += OnIngredientsCollectionChanged;
            Recipes.CollectionChanged += OnRecipesCollectionChanged;
            Products.CollectionChanged += OnProductsCollectionChanged;
            Suppliers.CollectionChanged += OnSuppliersCollectionChanged;

            AddIngredientCommand = ReactiveCommand.Create(AddIngredient);
            QuickCreateIngredientCommand = ReactiveCommand.Create(QuickCreateIngredient);
            AddRecipeCommand = ReactiveCommand.Create(AddRecipe);
            DuplicateSelectedRecipeCommand = ReactiveCommand.Create(DuplicateSelectedRecipe);
            AddSelectedIngredientToRecipeCommand = ReactiveCommand.Create(AddSelectedIngredientToRecipe);
            AddProductCommand = ReactiveCommand.Create(AddProduct);
            AddSelectedRecipeToProductCommand = ReactiveCommand.Create(AddSelectedRecipeToProduct);
            AddSupplierCommand = ReactiveCommand.Create(AddSupplier);
            NextOnboardingCommand = ReactiveCommand.Create(NextOnboardingStep);
            PreviousOnboardingCommand = ReactiveCommand.Create(PreviousOnboardingStep);
            CompleteOnboardingCommand = ReactiveCommand.Create(CompleteOnboarding);
            ToggleQuickAddCommand = ReactiveCommand.Create(() =>
            {
                IsQuickAddOpen = !IsQuickAddOpen;
            });

            SeedData();
        }

        public ObservableCollection<IngredientCostItemViewModel> Ingredients { get; }

        public ObservableCollection<RecipeViewModel> Recipes { get; }

        public ObservableCollection<ProductViewModel> Products { get; }

        public ObservableCollection<SupplierItemViewModel> Suppliers { get; }

        public ObservableCollection<string> PricingMethods { get; }

        public ObservableCollection<string> ReportFormats { get; }

        public ObservableCollection<string> ThemeModes { get; }

        public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> AddIngredientCommand { get; }

        public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> QuickCreateIngredientCommand { get; }

        public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> AddRecipeCommand { get; }

        public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> DuplicateSelectedRecipeCommand { get; }

        public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> AddSelectedIngredientToRecipeCommand { get; }

        public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> AddProductCommand { get; }

        public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> AddSelectedRecipeToProductCommand { get; }

        public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> AddSupplierCommand { get; }

        public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> NextOnboardingCommand { get; }

        public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> PreviousOnboardingCommand { get; }

        public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> CompleteOnboardingCommand { get; }

        public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> ToggleQuickAddCommand { get; }

        public bool IsNavigationPaneOpen
        {
            get => _isNavigationPaneOpen;
            set => this.RaiseAndSetIfChanged(ref _isNavigationPaneOpen, value);
        }

        public bool IsOnboardingVisible
        {
            get => _isOnboardingVisible;
            set => this.RaiseAndSetIfChanged(ref _isOnboardingVisible, value);
        }

        public bool IsQuickAddOpen
        {
            get => _isQuickAddOpen;
            set => this.RaiseAndSetIfChanged(ref _isQuickAddOpen, value);
        }

        public int OnboardingStep
        {
            get => _onboardingStep;
            set
            {
                this.RaiseAndSetIfChanged(ref _onboardingStep, Math.Clamp(value, 0, 2));
                RaiseOnboardingProperties();
            }
        }

        public bool IsWelcomeStep => OnboardingStep == 0;

        public bool IsBusinessSetupStep => OnboardingStep == 1;

        public bool IsIngredientSetupStep => OnboardingStep == 2;

        public string OnboardingTitle => OnboardingStep switch
        {
            1 => "Setup usaha",
            2 => "Tambah bahan pertama",
            _ => "Selamat datang di HPP Calculator",
        };

        public string OnboardingSubtitle => OnboardingStep switch
        {
            1 => "Lengkapi profil usaha agar currency dan satuan langsung sesuai.",
            2 => "Tambahkan bahan pertama, lalu bangun resep, produk, dan HPP.",
            _ => "Kelola biaya produksi makanan dengan flow yang cepat, ringkas, dan real-time.",
        };

        public string BusinessName
        {
            get => _businessName;
            set => this.RaiseAndSetIfChanged(ref _businessName, value);
        }

        public string BusinessType
        {
            get => _businessType;
            set => this.RaiseAndSetIfChanged(ref _businessType, value);
        }

        public string Currency
        {
            get => _currency;
            set => this.RaiseAndSetIfChanged(ref _currency, value);
        }

        public string DefaultUnit
        {
            get => _defaultUnit;
            set => this.RaiseAndSetIfChanged(ref _defaultUnit, value);
        }

        public string CurrentSection
        {
            get => _currentSection;
            private set
            {
                this.RaiseAndSetIfChanged(ref _currentSection, value);
                RaiseSectionProperties();
            }
        }

        public string CurrentSectionTitle => CurrentSection switch
        {
            "ingredients" => "Ingredients",
            "recipes" => "Recipes",
            "products" => "Products",
            "costing" => "HPP Calculator",
            "planner" => "Production Planner",
            "analytics" => "Analytics",
            "suppliers" => "Suppliers",
            "reports" => "Reports",
            "settings" => "Settings",
            _ => "Dashboard",
        };

        public string CurrentSectionDescription => CurrentSection switch
        {
            "ingredients" => "Database bahan dengan supplier, usage, dan update tracking.",
            "recipes" => "Builder resep real-time dengan tabel bahan dan cost summary.",
            "products" => "Hubungkan resep menjadi produk jual dan lihat margin default.",
            "costing" => "Simulasi HPP, pricing strategy, dan profit per unit.",
            "planner" => "Hitung kebutuhan bahan berdasarkan target produksi aktual.",
            "analytics" => "Lihat breakdown biaya, profitabilitas, dan tren cost.",
            "suppliers" => "Kelola vendor bahan, lead time, dan kualitas sourcing.",
            "reports" => "Siapkan export laporan HPP, bahan, dan profit produk.",
            "settings" => "Atur profil usaha, waste, tema, dan pengelolaan data.",
            _ => "Ringkasan kondisi bisnis dari bahan sampai profit.",
        };

        public bool IsDashboardSelected => CurrentSection == "dashboard";
        public bool IsIngredientsSelected => CurrentSection == "ingredients";
        public bool IsRecipesSelected => CurrentSection == "recipes";
        public bool IsProductsSelected => CurrentSection == "products";
        public bool IsCostingSelected => CurrentSection == "costing";
        public bool IsPlannerSelected => CurrentSection == "planner";
        public bool IsAnalyticsSelected => CurrentSection == "analytics";
        public bool IsSuppliersSelected => CurrentSection == "suppliers";
        public bool IsReportsSelected => CurrentSection == "reports";
        public bool IsSettingsSelected => CurrentSection == "settings";

        public string GlobalSearchText
        {
            get => _globalSearchText;
            set
            {
                this.RaiseAndSetIfChanged(ref _globalSearchText, value);
                RaiseSearchProperties();
            }
        }

        public string IngredientSearchText
        {
            get => _ingredientSearchText;
            set
            {
                this.RaiseAndSetIfChanged(ref _ingredientSearchText, value);
                this.RaisePropertyChanged(nameof(FilteredIngredients));
            }
        }

        public IEnumerable<IngredientCostItemViewModel> SearchIngredients => Ingredients.Where(ingredient => MatchesGlobalSearch(ingredient.Name, ingredient.Category, ingredient.SupplierName)).Take(4);
        public IEnumerable<IngredientCostItemViewModel> FilteredIngredients => Ingredients.Where(ingredient =>
            string.IsNullOrWhiteSpace(IngredientSearchText) ||
            ingredient.Name.Contains(IngredientSearchText, StringComparison.OrdinalIgnoreCase) ||
            ingredient.Category.Contains(IngredientSearchText, StringComparison.OrdinalIgnoreCase) ||
            ingredient.SupplierName.Contains(IngredientSearchText, StringComparison.OrdinalIgnoreCase));
        public IEnumerable<RecipeViewModel> SearchRecipes => Recipes.Where(recipe => MatchesGlobalSearch(recipe.Name, recipe.Category, recipe.Description)).Take(4);
        public IEnumerable<ProductViewModel> SearchProducts => Products.Where(product => MatchesGlobalSearch(product.Name, product.Category, product.PrimaryRecipeName)).Take(4);
        public bool HasSearchIngredientResults => SearchIngredients.Any();
        public bool HasSearchRecipeResults => SearchRecipes.Any();
        public bool HasSearchProductResults => SearchProducts.Any();
        public bool HasGlobalSearchResults => !string.IsNullOrWhiteSpace(GlobalSearchText) && (HasSearchIngredientResults || HasSearchRecipeResults || HasSearchProductResults);

        public IngredientCostItemViewModel? SelectedCatalogIngredient
        {
            get => _selectedCatalogIngredient;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedCatalogIngredient, value);
                RaiseSelectedIngredientProperties();
            }
        }

        public RecipeViewModel? SelectedRecipe
        {
            get => _selectedRecipe;
            set
            {
                if (ReferenceEquals(_selectedRecipe, value))
                {
                    return;
                }

                this.RaiseAndSetIfChanged(ref _selectedRecipe, value);
                SyncRecipeEditorState(value);
                this.RaisePropertyChanged(nameof(RecipeItems));
                RaiseRecipeSummaryProperties();
            }
        }

        public RecipeViewModel? SelectedRecipeForProduct
        {
            get => _selectedRecipeForProduct;
            set => this.RaiseAndSetIfChanged(ref _selectedRecipeForProduct, value);
        }
        public ProductViewModel? SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedProduct, value);
                RaisePricingProperties();
                RaiseDashboardProperties();
                RaisePlannerProperties();
            }
        }

        public SupplierItemViewModel? SelectedSupplier
        {
            get => _selectedSupplier;
            set => this.RaiseAndSetIfChanged(ref _selectedSupplier, value);
        }

        public string RecipeName
        {
            get => _recipeName;
            set
            {
                var sanitized = value ?? string.Empty;
                if (_recipeName == sanitized)
                {
                    return;
                }

                this.RaiseAndSetIfChanged(ref _recipeName, sanitized);
                if (SelectedRecipe != null && SelectedRecipe.Name != sanitized)
                {
                    SelectedRecipe.Name = sanitized;
                }

                RaiseRecipeSummaryProperties();
                RaiseSearchProperties();
            }
        }

        public decimal PortionCount
        {
            get => SelectedRecipe?.TargetYieldQuantity ?? 0m;
            set
            {
                if (SelectedRecipe == null)
                {
                    return;
                }

                var sanitized = Math.Max(0m, value);
                if (SelectedRecipe.TargetYieldQuantity == sanitized)
                {
                    return;
                }

                SelectedRecipe.TargetYieldQuantity = sanitized;
                this.RaisePropertyChanged(nameof(PortionCount));
                RaiseRecipeSummaryProperties();
            }
        }

        public string PricingMethod
        {
            get => _pricingMethod;
            set
            {
                this.RaiseAndSetIfChanged(ref _pricingMethod, value);
                RaisePricingProperties();
            }
        }

        public decimal MarginPercentInput
        {
            get => _marginPercentInput;
            set
            {
                this.RaiseAndSetIfChanged(ref _marginPercentInput, Math.Clamp(value, 0m, 95m));
                RaisePricingProperties();
            }
        }

        public decimal MarkupPercentInput
        {
            get => _markupPercentInput;
            set
            {
                this.RaiseAndSetIfChanged(ref _markupPercentInput, Math.Max(0m, value));
                RaisePricingProperties();
            }
        }

        public decimal TargetProfitPerUnitInput
        {
            get => _targetProfitPerUnitInput;
            set
            {
                this.RaiseAndSetIfChanged(ref _targetProfitPerUnitInput, Math.Max(0m, value));
                RaisePricingProperties();
            }
        }

        public bool IsMarginMode => PricingMethod == "Margin";
        public bool IsMarkupMode => PricingMethod == "Markup";
        public bool IsTargetProfitMode => PricingMethod == "Target Profit";

        public decimal PlannerTargetProduction
        {
            get => _plannerTargetProduction;
            set
            {
                this.RaiseAndSetIfChanged(ref _plannerTargetProduction, Math.Max(0m, value));
                RaisePlannerProperties();
            }
        }

        public decimal WastePercentage
        {
            get => _wastePercentage;
            set
            {
                this.RaiseAndSetIfChanged(ref _wastePercentage, Math.Max(0m, value));
                RaisePlannerProperties();
            }
        }

        public string ReportFormat
        {
            get => _reportFormat;
            set => this.RaiseAndSetIfChanged(ref _reportFormat, value);
        }

        public string ThemeMode
        {
            get => _themeMode;
            set => this.RaiseAndSetIfChanged(ref _themeMode, value);
        }

        public int TotalIngredients => Ingredients.Count;
        public int TotalRecipes => Recipes.Count;
        public int TotalProducts => Products.Count;
        public int TotalSuppliers => Suppliers.Count;
        public decimal AverageHpp => Products.Count == 0 ? 0m : Math.Round(Products.Average(product => product.HppPerUnit), 2);
        public decimal AverageMargin => Products.Count == 0 ? 0m : Math.Round(Products.Average(product => product.DefaultMarginPercent), 2);
        public ObservableCollection<RecipeIngredientItemViewModel> RecipeItems => SelectedRecipe?.Items ?? EmptyRecipeItems;
        public int RecipeIngredientCount => RecipeItems.Count;
        public decimal TotalRecipeIngredientCost => SelectedRecipe?.BaseCost ?? 0m;
        public decimal SelectedRecipePackagingCost => SelectedRecipe?.PackagingCost ?? 0m;
        public decimal SelectedRecipeLaborCost => SelectedRecipe?.LaborCost ?? 0m;
        public decimal SelectedRecipeOverheadCost => SelectedRecipe?.OverheadCost ?? 0m;
        public decimal TotalModal => SelectedRecipe?.TotalCost ?? 0m;
        public decimal HppPerPortion => SelectedRecipe?.CostPerUnit ?? 0m;
        public decimal SelectedRecipeScaledCost => SelectedRecipe?.ScaledCost ?? 0m;
        public string FormulaSummary => "Bahan disimpan sekali, resep memanggil bahan, produk memanggil resep, lalu HPP dan harga jual dihitung otomatis.";
        public decimal SelectedProductIngredientCost => SelectedProduct?.IngredientCost ?? 0m;
        public decimal SelectedProductPackagingCost => SelectedProduct?.PackagingCost ?? 0m;
        public decimal SelectedProductLaborCost => SelectedProduct?.LaborCost ?? 0m;
        public decimal SelectedProductOverheadCost => SelectedProduct?.OverheadCost ?? 0m;
        public decimal SelectedProductTotalCost => SelectedProduct?.TotalCost ?? 0m;
        public decimal SelectedProductHpp => SelectedProduct?.HppPerUnit ?? 0m;

        public decimal SuggestedSellingPrice => SelectedProduct == null ? 0m : PricingMethod switch
        {
            "Markup" => Math.Round(SelectedProduct.HppPerUnit * (1m + (MarkupPercentInput / 100m)), 2),
            "Target Profit" => Math.Round(SelectedProduct.HppPerUnit + TargetProfitPerUnitInput, 2),
            _ => MarginPercentInput >= 100m ? 0m : Math.Round(SelectedProduct.HppPerUnit / (1m - (MarginPercentInput / 100m)), 2),
        };

        public decimal ProfitPerUnit => SelectedProduct == null ? 0m : Math.Round(SuggestedSellingPrice - SelectedProduct.HppPerUnit, 2);
        public string PricingInsight => SelectedProduct == null ? "Pilih produk untuk mulai simulasi pricing." : $"{PricingMethod} menghasilkan harga jual {SuggestedSellingPrice:N0} dengan profit {ProfitPerUnit:N0} per unit.";
        public string IngredientAlert => Ingredients.Count == 0 ? "Belum ada data bahan." : $"Harga {Ingredients.OrderByDescending(x => x.PackPrice).First().Name} masih menjadi cost tertinggi.";
        public string StockAlert => Ingredients.Count == 0 ? "Belum ada stok terpantau." : $"Stok referensi {Ingredients.OrderBy(x => x.PackQuantity).First().Name} perlu dicek ulang.";
        public string MarginAlert => Products.Count == 0 ? "Belum ada produk." : $"Margin terendah saat ini ada di {Products.OrderBy(x => x.DefaultMarginPercent).First().Name}.";
        public IEnumerable<string> NotificationItems => new[] { IngredientAlert, StockAlert, MarginAlert };
        public IEnumerable<ProductViewModel> TopProfitableProducts => Products.OrderByDescending(product => product.ProfitPerUnit).Take(4);
        public IEnumerable<IngredientCostItemViewModel> MostExpensiveIngredients => Ingredients.OrderByDescending(ingredient => ingredient.PackPrice).Take(4);
        public string MostProfitableProduct => TopProfitableProducts.FirstOrDefault()?.Name ?? "-";
        public string HighestHppProduct => Products.OrderByDescending(product => product.HppPerUnit).FirstOrDefault()?.Name ?? "-";
        public string MostExpensiveIngredient => MostExpensiveIngredients.FirstOrDefault()?.Name ?? "-";
        public string HighlightNarrative => SelectedProduct == null ? FormulaSummary : $"{SelectedProduct.Name} memiliki HPP {SelectedProduct.HppPerUnit:N0} dan harga rekomendasi {SuggestedSellingPrice:N0}.";
        public IEnumerable<BarDataItemViewModel> DashboardCostBreakdown => BuildBreakdownBars(SelectedProductIngredientCost, SelectedProductPackagingCost, SelectedProductLaborCost, SelectedProductOverheadCost);
        public IEnumerable<BarDataItemViewModel> SelectedIngredientHistory => BuildIngredientHistoryBars();
        public IEnumerable<BarDataItemViewModel> IngredientUsageBars => BuildIngredientUsageBars();
        public IEnumerable<BarDataItemViewModel> ProductProfitBars => BuildProductProfitBars();
        public IEnumerable<BarDataItemViewModel> CostTrendBars => BuildBreakdownBars(SelectedProductIngredientCost * 0.84m, SelectedProductPackagingCost * 0.92m, SelectedProductLaborCost, SelectedProductOverheadCost * 1.08m, prefixA: "Jan", prefixB: "Feb", prefixC: "Mar", prefixD: "Now");

        public IEnumerable<string> SelectedIngredientRecipeNames => SelectedCatalogIngredient == null ? Array.Empty<string>() : Recipes.Where(recipe => recipe.Items.Any(item => ReferenceEquals(item.Ingredient, SelectedCatalogIngredient))).Select(recipe => recipe.Name).ToArray();
        public bool HasSelectedIngredientRecipes => SelectedIngredientRecipeNames.Any();
        public IEnumerable<PlannerRequirementItemViewModel> PlannerRequirements => BuildPlannerRequirements();
        public decimal PlannerScaleFactor => SelectedProduct == null || SelectedProduct.OutputUnits <= 0m ? 0m : Math.Round(PlannerTargetProduction / SelectedProduct.OutputUnits, 4);
        public string PlannerNarrative => SelectedProduct == null ? "Pilih produk untuk menghitung kebutuhan bahan." : $"Target {PlannerTargetProduction:N0} unit dengan waste {WastePercentage:N0}% akan menghitung kebutuhan semua bahan otomatis.";

        public void SelectSection(string? key)
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                CurrentSection = key;
            }
        }

        private void SeedData()
        {
            var supplierA = AddSupplier("Sumber Makmur", "0812-0000-1111", 2, "Fokus tepung dan gula", "????");
            var supplierB = AddSupplier("Makmur Jaya", "0812-2222-3333", 1, "Produk susu dan telur", "???");
            var supplierC = AddSupplier("Toko A", "0812-8888-5555", 3, "Minyak dan bahan penunjang", "????");

            var flour = AddIngredient("Tepung Terigu", "Bahan", DefaultUnit, 1000m, 12000m, supplierA.Name, 2, 2);
            var sugar = AddIngredient("Gula", "Bahan", DefaultUnit, 1000m, 14000m, supplierA.Name, 3, 1);
            var egg = AddIngredient("Telur", "Protein", "pcs", 30m, 15000m, supplierB.Name, 2, 1);
            var oil = AddIngredient("Minyak", "Goreng", "ml", 1000m, 18000m, supplierC.Name, 2, 3);
            var cheese = AddIngredient("Keju", "Topping", DefaultUnit, 1000m, 68000m, supplierB.Name, 1, 1);
            var chocolate = AddIngredient("Coklat", "Topping", DefaultUnit, 1000m, 72000m, supplierB.Name, 1, 2);

            var recipe1 = AddRecipe("Donat Original", "Donat", 20m, "pcs", "Base recipe untuk semua varian donat.", 2);
            recipe1.AddIngredient(flour, 500m);
            recipe1.AddIngredient(sugar, 120m);
            recipe1.AddIngredient(egg, 2m);
            recipe1.AddIngredient(oil, 250m);
            recipe1.PackagingCost = 500m;
            recipe1.LaborCost = 1000m;
            recipe1.OverheadCost = 1000m;
            recipe1.SetSeedUpdateAge(2);

            var recipe2 = AddRecipe("Donat Coklat", "Donat", 20m, "pcs", "Varian donat dengan topping coklat.", 1);
            recipe2.AddIngredient(flour, 500m);
            recipe2.AddIngredient(sugar, 120m);
            recipe2.AddIngredient(egg, 2m);
            recipe2.AddIngredient(oil, 250m);
            recipe2.AddIngredient(chocolate, 150m);
            recipe2.PackagingCost = 500m;
            recipe2.LaborCost = 1000m;
            recipe2.OverheadCost = 1000m;
            recipe2.SetSeedUpdateAge(1);

            var recipe3 = AddRecipe("Donat Keju", "Donat", 20m, "pcs", "Varian donat dengan topping keju.", 1);
            recipe3.AddIngredient(flour, 500m);
            recipe3.AddIngredient(sugar, 120m);
            recipe3.AddIngredient(egg, 2m);
            recipe3.AddIngredient(oil, 250m);
            recipe3.AddIngredient(cheese, 150m);
            recipe3.PackagingCost = 500m;
            recipe3.LaborCost = 1000m;
            recipe3.OverheadCost = 1000m;
            recipe3.SetSeedUpdateAge(1);

            var product1 = AddProduct("Donat Coklat", "Donat", 20m, 500m, 1000m, 1000m, 63m);
            product1.AddComponent(recipe2, 1m);
            var product2 = AddProduct("Donat Keju", "Donat", 20m, 500m, 1000m, 1000m, 62m);
            product2.AddComponent(recipe3, 1m);
            var product3 = AddProduct("Donat Original", "Donat", 20m, 500m, 1000m, 1000m, 60m);
            product3.AddComponent(recipe1, 1m);

            SyncRelationshipMetadata();

            SelectedCatalogIngredient = flour;
            SelectedRecipe = recipe1;
            SelectedRecipeForProduct = recipe2;
            SelectedProduct = product1;
            SelectedSupplier = supplierA;
            SyncRecipeEditorState(recipe1);
        }

        private void AddIngredient()
        {
            var supplierName = Suppliers.Count > 0 ? Suppliers[0].Name : "Supplier utama";
            SelectedCatalogIngredient = AddIngredient("Bahan baru", "Bahan", DefaultUnit, 1000m, 0m, supplierName, 0, 0);
            CurrentSection = "ingredients";

            if (IsOnboardingVisible && IsIngredientSetupStep)
            {
                CompleteOnboarding();
            }
        }

        private void QuickCreateIngredient()
        {
            AddIngredient();
            IsQuickAddOpen = false;
        }

        private IngredientCostItemViewModel AddIngredient(string name, string category, string unit, decimal quantity, decimal price, string supplier, int usedInCount, int updateAgeDays)
        {
            var ingredient = new IngredientCostItemViewModel(RemoveIngredient)
            {
                Name = name,
                Category = category,
                UnitName = unit,
                PackQuantity = quantity,
                PackPrice = price,
                SupplierName = supplier,
            };

            ingredient.SetSeedMetadata(usedInCount, updateAgeDays);
            Ingredients.Add(ingredient);
            return ingredient;
        }
        private void AddRecipe()
        {
            SelectedRecipe = AddRecipe("Recipe Baru", "Produk Baru", 20m, "pcs", string.Empty, 0);
            CurrentSection = "recipes";
            IsQuickAddOpen = false;
        }

        private RecipeViewModel AddRecipe(string name, string category, decimal yieldQuantity, string yieldUnit, string description, int updateAgeDays)
        {
            var recipe = new RecipeViewModel(RemoveRecipe, DuplicateRecipe)
            {
                Name = name,
                Category = category,
                YieldQuantity = yieldQuantity,
                YieldUnit = yieldUnit,
                TargetYieldQuantity = yieldQuantity,
                Description = description,
            };

            recipe.SetSeedUpdateAge(updateAgeDays);
            Recipes.Add(recipe);
            return recipe;
        }

        private void DuplicateSelectedRecipe()
        {
            if (SelectedRecipe != null)
            {
                DuplicateRecipe(SelectedRecipe);
            }
        }

        private void DuplicateRecipe(RecipeViewModel recipe)
        {
            var clone = recipe.Clone();
            Recipes.Add(clone);
            SelectedRecipe = clone;
        }

        private void AddSelectedIngredientToRecipe()
        {
            if (SelectedRecipe == null || SelectedCatalogIngredient == null)
            {
                return;
            }

            SelectedRecipe.AddIngredient(SelectedCatalogIngredient, 0m);
            RaiseRecipeSummaryProperties();
            SyncRelationshipMetadata();
        }

        private void AddProduct()
        {
            SelectedProduct = AddProduct("Produk Baru", "Produk", 20m, 500m, 1000m, 1000m, 40m);
            CurrentSection = "products";
            IsQuickAddOpen = false;
        }

        private ProductViewModel AddProduct(string name, string category, decimal outputUnits, decimal packaging, decimal labor, decimal overhead, decimal defaultMargin)
        {
            var product = new ProductViewModel(RemoveProduct)
            {
                Name = name,
                Category = category,
                OutputUnits = outputUnits,
                PackagingCost = packaging,
                LaborCost = labor,
                OverheadCost = overhead,
                DefaultMarginPercent = defaultMargin,
            };

            Products.Add(product);
            return product;
        }

        private void AddSelectedRecipeToProduct()
        {
            if (SelectedProduct == null || SelectedRecipeForProduct == null)
            {
                return;
            }

            SelectedProduct.AddComponent(SelectedRecipeForProduct, 1m);
            RaiseDashboardProperties();
            RaisePlannerProperties();
        }

        private void AddSupplier()
        {
            SelectedSupplier = AddSupplier("Supplier Baru", string.Empty, 2, string.Empty, "???");
            CurrentSection = "suppliers";
            IsQuickAddOpen = false;
        }

        private SupplierItemViewModel AddSupplier(string name, string phone, int leadTimeDays, string notes, string ratingStars)
        {
            var supplier = new SupplierItemViewModel(RemoveSupplier)
            {
                Name = name,
                Phone = phone,
                LeadTimeDays = leadTimeDays,
                Notes = notes,
                RatingStars = ratingStars,
            };

            Suppliers.Add(supplier);
            return supplier;
        }

        private void NextOnboardingStep()
        {
            if (OnboardingStep < 2)
            {
                OnboardingStep++;
            }
            else
            {
                CompleteOnboarding();
            }
        }

        private void PreviousOnboardingStep()
        {
            if (OnboardingStep > 0)
            {
                OnboardingStep--;
            }
        }

        private void CompleteOnboarding()
        {
            IsOnboardingVisible = false;
            CurrentSection = "dashboard";
        }

        private void RemoveIngredient(IngredientCostItemViewModel ingredient)
        {
            foreach (var recipe in Recipes)
            {
                for (var index = recipe.Items.Count - 1; index >= 0; index--)
                {
                    if (ReferenceEquals(recipe.Items[index].Ingredient, ingredient))
                    {
                        recipe.Items[index].Detach();
                        recipe.Items.RemoveAt(index);
                    }
                }
            }

            Ingredients.Remove(ingredient);
            if (ReferenceEquals(SelectedCatalogIngredient, ingredient))
            {
                SelectedCatalogIngredient = Ingredients.FirstOrDefault();
            }

            SyncRelationshipMetadata();
        }

        private void RemoveRecipe(RecipeViewModel recipe)
        {
            foreach (var product in Products)
            {
                for (var index = product.Components.Count - 1; index >= 0; index--)
                {
                    if (ReferenceEquals(product.Components[index].Recipe, recipe))
                    {
                        product.Components[index].Detach();
                        product.Components.RemoveAt(index);
                    }
                }
            }

            Recipes.Remove(recipe);
            if (ReferenceEquals(SelectedRecipe, recipe))
            {
                SelectedRecipe = Recipes.FirstOrDefault();
            }

            if (ReferenceEquals(SelectedRecipeForProduct, recipe))
            {
                SelectedRecipeForProduct = Recipes.FirstOrDefault();
            }

            SyncRelationshipMetadata();
        }

        private void RemoveProduct(ProductViewModel product)
        {
            Products.Remove(product);
            if (ReferenceEquals(SelectedProduct, product))
            {
                SelectedProduct = Products.FirstOrDefault();
            }

            RaiseDashboardProperties();
            RaisePlannerProperties();
        }

        private void RemoveSupplier(SupplierItemViewModel supplier)
        {
            Suppliers.Remove(supplier);
            if (ReferenceEquals(SelectedSupplier, supplier))
            {
                SelectedSupplier = Suppliers.FirstOrDefault();
            }

            SyncRelationshipMetadata();
        }

        private void AttachIngredient(IngredientCostItemViewModel ingredient) => ingredient.PropertyChanged += OnIngredientPropertyChanged;
        private void AttachRecipe(RecipeViewModel recipe) => recipe.PropertyChanged += OnRecipePropertyChanged;
        private void AttachProduct(ProductViewModel product) => product.PropertyChanged += OnProductPropertyChanged;
        private void AttachSupplier(SupplierItemViewModel supplier) => supplier.PropertyChanged += OnSupplierPropertyChanged;
        private void DetachIngredient(IngredientCostItemViewModel ingredient) => ingredient.PropertyChanged -= OnIngredientPropertyChanged;
        private void DetachRecipe(RecipeViewModel recipe) => recipe.PropertyChanged -= OnRecipePropertyChanged;
        private void DetachProduct(ProductViewModel product) => product.PropertyChanged -= OnProductPropertyChanged;
        private void DetachSupplier(SupplierItemViewModel supplier) => supplier.PropertyChanged -= OnSupplierPropertyChanged;

        private void OnIngredientPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            SyncRelationshipMetadata();
            this.RaisePropertyChanged(nameof(FilteredIngredients));
            RaiseSearchProperties();
            RaiseSelectedIngredientProperties();
            RaiseDashboardProperties();
            RaisePlannerProperties();
        }

        private void OnRecipePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is RecipeViewModel recipe && ReferenceEquals(recipe, SelectedRecipe))
            {
                SyncRecipeEditorState(recipe);
            }

            SyncRelationshipMetadata();
            RaiseSearchProperties();
            RaiseRecipeSummaryProperties();
            RaiseSelectedIngredientProperties();
            RaiseDashboardProperties();
            RaisePlannerProperties();
        }

        private void OnProductPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            RaiseSearchProperties();
            RaisePricingProperties();
            RaiseDashboardProperties();
            RaisePlannerProperties();
        }

        private void OnSupplierPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            RaiseDashboardProperties();
        }
        private void OnIngredientsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            SyncCollection<IngredientCostItemViewModel>(e, AttachIngredient, DetachIngredient);
            if (SelectedCatalogIngredient == null && Ingredients.Count > 0)
            {
                SelectedCatalogIngredient = Ingredients[0];
            }

            SyncRelationshipMetadata();
            this.RaisePropertyChanged(nameof(FilteredIngredients));
            RaiseSearchProperties();
            RaiseDashboardProperties();
        }

        private void OnRecipesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            SyncCollection<RecipeViewModel>(e, AttachRecipe, DetachRecipe);
            if (SelectedRecipe == null && Recipes.Count > 0)
            {
                SelectedRecipe = Recipes[0];
            }

            if (SelectedRecipeForProduct == null && Recipes.Count > 0)
            {
                SelectedRecipeForProduct = Recipes[0];
            }

            SyncRelationshipMetadata();
            RaiseSearchProperties();
            RaiseRecipeSummaryProperties();
            RaisePlannerProperties();
        }

        private void OnProductsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            SyncCollection<ProductViewModel>(e, AttachProduct, DetachProduct);
            if (SelectedProduct == null && Products.Count > 0)
            {
                SelectedProduct = Products[0];
            }

            RaiseSearchProperties();
            RaiseDashboardProperties();
            RaisePlannerProperties();
        }

        private void OnSuppliersCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            SyncCollection<SupplierItemViewModel>(e, AttachSupplier, DetachSupplier);
            if (SelectedSupplier == null && Suppliers.Count > 0)
            {
                SelectedSupplier = Suppliers[0];
            }

            SyncRelationshipMetadata();
            RaiseDashboardProperties();
        }

        private static void SyncCollection<T>(NotifyCollectionChangedEventArgs e, Action<T> attach, Action<T> detach)
        {
            if (e.NewItems != null)
            {
                foreach (T item in e.NewItems)
                {
                    attach(item);
                }
            }

            if (e.OldItems != null)
            {
                foreach (T item in e.OldItems)
                {
                    detach(item);
                }
            }
        }

        private void SyncRelationshipMetadata()
        {
            foreach (var ingredient in Ingredients)
            {
                ingredient.UsedInCount = Recipes.Count(recipe => recipe.Items.Any(item => ReferenceEquals(item.Ingredient, ingredient)));
            }

            foreach (var supplier in Suppliers)
            {
                var supplierIngredients = Ingredients.Where(ingredient => string.Equals(ingredient.SupplierName, supplier.Name, StringComparison.OrdinalIgnoreCase)).ToArray();
                supplier.IngredientCount = supplierIngredients.Length;
                supplier.AveragePriceBand = supplierIngredients.Length switch
                {
                    0 => "-",
                    _ when supplierIngredients.Average(ingredient => ingredient.PackPrice) < 15000m => "Cheap",
                    _ when supplierIngredients.Average(ingredient => ingredient.PackPrice) < 40000m => "Medium",
                    _ => "Premium",
                };
            }

            RaiseSelectedIngredientProperties();
            RaiseDashboardProperties();
            RaisePlannerProperties();
        }

        private void SyncRecipeEditorState(RecipeViewModel? recipe)
        {
            var name = recipe?.Name ?? string.Empty;
            if (_recipeName != name)
            {
                this.RaiseAndSetIfChanged(ref _recipeName, name, nameof(RecipeName));
            }

            this.RaisePropertyChanged(nameof(PortionCount));
        }

        private IEnumerable<PlannerRequirementItemViewModel> BuildPlannerRequirements()
        {
            if (SelectedProduct == null || PlannerScaleFactor <= 0m)
            {
                return Array.Empty<PlannerRequirementItemViewModel>();
            }

            var wasteMultiplier = 1m + (WastePercentage / 100m);

            return SelectedProduct.Components
                .SelectMany(component => component.Recipe.Items.Select(item => new
                {
                    item.IngredientName,
                    item.UnitName,
                    Quantity = item.UsageQuantity * component.Multiplier * PlannerScaleFactor * wasteMultiplier,
                }))
                .GroupBy(item => new { item.IngredientName, item.UnitName })
                .Select(group => new PlannerRequirementItemViewModel
                {
                    IngredientName = group.Key.IngredientName,
                    UnitName = group.Key.UnitName,
                    Quantity = Math.Round(group.Sum(item => item.Quantity), 2),
                })
                .OrderBy(item => item.IngredientName)
                .ToArray();
        }

        private IEnumerable<BarDataItemViewModel> BuildIngredientHistoryBars()
        {
            var ingredient = SelectedCatalogIngredient;
            if (ingredient == null)
            {
                return Array.Empty<BarDataItemViewModel>();
            }

            var values = new[]
            {
                (Label: "Jan", Value: ingredient.PackPrice * 0.86m),
                (Label: "Feb", Value: ingredient.PackPrice * 0.94m),
                (Label: "Mar", Value: ingredient.PackPrice),
            };

            var max = values.Max(item => item.Value);
            return values.Select(item => new BarDataItemViewModel
            {
                Label = item.Label,
                ValueText = $"{Currency} {item.Value:N0}",
                BarWidth = max <= 0m ? 0d : (double)(item.Value / max * 180m),
            }).ToArray();
        }

        private IEnumerable<BarDataItemViewModel> BuildIngredientUsageBars()
        {
            var topIngredients = Ingredients.OrderByDescending(ingredient => ingredient.UsedInCount).Take(4).ToArray();
            var max = topIngredients.Length == 0 ? 0 : topIngredients.Max(ingredient => ingredient.UsedInCount);
            return topIngredients.Select(ingredient => new BarDataItemViewModel
            {
                Label = ingredient.Name,
                ValueText = ingredient.UsedInSummary,
                BarWidth = max == 0 ? 0d : ingredient.UsedInCount / (double)max * 220d,
            }).ToArray();
        }

        private IEnumerable<BarDataItemViewModel> BuildProductProfitBars()
        {
            var topProducts = Products.OrderByDescending(product => product.ProfitPerUnit).Take(4).ToArray();
            var max = topProducts.Length == 0 ? 0m : topProducts.Max(product => product.ProfitPerUnit);
            return topProducts.Select(product => new BarDataItemViewModel
            {
                Label = product.Name,
                ValueText = $"{Currency} {product.ProfitPerUnit:N0}",
                BarWidth = max <= 0m ? 0d : (double)(product.ProfitPerUnit / max * 220m),
            }).ToArray();
        }

        private IEnumerable<BarDataItemViewModel> BuildBreakdownBars(decimal ingredientValue, decimal packagingValue, decimal laborValue, decimal overheadValue, string prefixA = "Ingredient", string prefixB = "Packaging", string prefixC = "Labor", string prefixD = "Overhead")
        {
            var values = new[]
            {
                (Label: prefixA, Value: Math.Round(ingredientValue, 2)),
                (Label: prefixB, Value: Math.Round(packagingValue, 2)),
                (Label: prefixC, Value: Math.Round(laborValue, 2)),
                (Label: prefixD, Value: Math.Round(overheadValue, 2)),
            };

            var max = values.Max(item => item.Value);
            return values.Select(item => new BarDataItemViewModel
            {
                Label = item.Label,
                ValueText = $"{Currency} {item.Value:N0}",
                BarWidth = max <= 0m ? 0d : (double)(item.Value / max * 220m),
            }).ToArray();
        }

        private bool MatchesGlobalSearch(params string[] values)
        {
            if (string.IsNullOrWhiteSpace(GlobalSearchText))
            {
                return false;
            }

            return values.Any(value => value.Contains(GlobalSearchText, StringComparison.OrdinalIgnoreCase));
        }

        private void RaiseSectionProperties()
        {
            this.RaisePropertyChanged(nameof(CurrentSectionTitle));
            this.RaisePropertyChanged(nameof(CurrentSectionDescription));
            this.RaisePropertyChanged(nameof(IsDashboardSelected));
            this.RaisePropertyChanged(nameof(IsIngredientsSelected));
            this.RaisePropertyChanged(nameof(IsRecipesSelected));
            this.RaisePropertyChanged(nameof(IsProductsSelected));
            this.RaisePropertyChanged(nameof(IsCostingSelected));
            this.RaisePropertyChanged(nameof(IsPlannerSelected));
            this.RaisePropertyChanged(nameof(IsAnalyticsSelected));
            this.RaisePropertyChanged(nameof(IsSuppliersSelected));
            this.RaisePropertyChanged(nameof(IsReportsSelected));
            this.RaisePropertyChanged(nameof(IsSettingsSelected));
        }

        private void RaiseOnboardingProperties()
        {
            this.RaisePropertyChanged(nameof(IsWelcomeStep));
            this.RaisePropertyChanged(nameof(IsBusinessSetupStep));
            this.RaisePropertyChanged(nameof(IsIngredientSetupStep));
            this.RaisePropertyChanged(nameof(OnboardingTitle));
            this.RaisePropertyChanged(nameof(OnboardingSubtitle));
        }

        private void RaiseSearchProperties()
        {
            this.RaisePropertyChanged(nameof(SearchIngredients));
            this.RaisePropertyChanged(nameof(SearchRecipes));
            this.RaisePropertyChanged(nameof(SearchProducts));
            this.RaisePropertyChanged(nameof(HasSearchIngredientResults));
            this.RaisePropertyChanged(nameof(HasSearchRecipeResults));
            this.RaisePropertyChanged(nameof(HasSearchProductResults));
            this.RaisePropertyChanged(nameof(HasGlobalSearchResults));
        }

        private void RaiseSelectedIngredientProperties()
        {
            this.RaisePropertyChanged(nameof(SelectedIngredientRecipeNames));
            this.RaisePropertyChanged(nameof(HasSelectedIngredientRecipes));
            this.RaisePropertyChanged(nameof(SelectedIngredientHistory));
        }

        private void RaiseRecipeSummaryProperties()
        {
            this.RaisePropertyChanged(nameof(RecipeItems));
            this.RaisePropertyChanged(nameof(RecipeName));
            this.RaisePropertyChanged(nameof(PortionCount));
            this.RaisePropertyChanged(nameof(RecipeIngredientCount));
            this.RaisePropertyChanged(nameof(TotalRecipeIngredientCost));
            this.RaisePropertyChanged(nameof(SelectedRecipePackagingCost));
            this.RaisePropertyChanged(nameof(SelectedRecipeLaborCost));
            this.RaisePropertyChanged(nameof(SelectedRecipeOverheadCost));
            this.RaisePropertyChanged(nameof(TotalModal));
            this.RaisePropertyChanged(nameof(HppPerPortion));
            this.RaisePropertyChanged(nameof(SelectedRecipeScaledCost));
        }

        private void RaisePricingProperties()
        {
            this.RaisePropertyChanged(nameof(IsMarginMode));
            this.RaisePropertyChanged(nameof(IsMarkupMode));
            this.RaisePropertyChanged(nameof(IsTargetProfitMode));
            this.RaisePropertyChanged(nameof(SelectedProductIngredientCost));
            this.RaisePropertyChanged(nameof(SelectedProductPackagingCost));
            this.RaisePropertyChanged(nameof(SelectedProductLaborCost));
            this.RaisePropertyChanged(nameof(SelectedProductOverheadCost));
            this.RaisePropertyChanged(nameof(SelectedProductTotalCost));
            this.RaisePropertyChanged(nameof(SelectedProductHpp));
            this.RaisePropertyChanged(nameof(SuggestedSellingPrice));
            this.RaisePropertyChanged(nameof(ProfitPerUnit));
            this.RaisePropertyChanged(nameof(PricingInsight));
        }

        private void RaiseDashboardProperties()
        {
            this.RaisePropertyChanged(nameof(TotalIngredients));
            this.RaisePropertyChanged(nameof(TotalRecipes));
            this.RaisePropertyChanged(nameof(TotalProducts));
            this.RaisePropertyChanged(nameof(TotalSuppliers));
            this.RaisePropertyChanged(nameof(AverageHpp));
            this.RaisePropertyChanged(nameof(AverageMargin));
            this.RaisePropertyChanged(nameof(IngredientAlert));
            this.RaisePropertyChanged(nameof(StockAlert));
            this.RaisePropertyChanged(nameof(MarginAlert));
            this.RaisePropertyChanged(nameof(NotificationItems));
            this.RaisePropertyChanged(nameof(TopProfitableProducts));
            this.RaisePropertyChanged(nameof(MostExpensiveIngredients));
            this.RaisePropertyChanged(nameof(MostProfitableProduct));
            this.RaisePropertyChanged(nameof(HighestHppProduct));
            this.RaisePropertyChanged(nameof(MostExpensiveIngredient));
            this.RaisePropertyChanged(nameof(HighlightNarrative));
            this.RaisePropertyChanged(nameof(DashboardCostBreakdown));
            this.RaisePropertyChanged(nameof(IngredientUsageBars));
            this.RaisePropertyChanged(nameof(ProductProfitBars));
            this.RaisePropertyChanged(nameof(CostTrendBars));
        }

        private void RaisePlannerProperties()
        {
            this.RaisePropertyChanged(nameof(PlannerScaleFactor));
            this.RaisePropertyChanged(nameof(PlannerRequirements));
            this.RaisePropertyChanged(nameof(PlannerNarrative));
        }
    }
}
