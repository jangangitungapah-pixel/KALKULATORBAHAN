using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using FluentAvalonia.UI.Controls;
using KALKULATORBAHAN.ViewModels;

namespace KALKULATORBAHAN.Views
{
    public partial class MainWindow : Window
    {
        private RecipeIngredientItemViewModel? _draggedRecipeItem;

        public MainWindow()
        {
            InitializeComponent();
            Opened += OnOpened;
        }

        private void OnOpened(object? sender, System.EventArgs e)
        {
            if (RootNavigation.SelectedItem is null)
            {
                RootNavigation.SelectedItem = DashboardItem;
            }

            ApplySelectedSection();
        }

        private void OnNavigationSelectionChanged(object? sender, NavigationViewSelectionChangedEventArgs e)
        {
            ApplySelectedSection(e.SelectedItemContainer as NavigationViewItem);
        }

        private void ApplySelectedSection(NavigationViewItem? selectedItem = null)
        {
            if (DataContext is not MainWindowViewModel viewModel)
            {
                return;
            }

            var item = selectedItem ?? RootNavigation.SelectedItem as NavigationViewItem;
            viewModel.SelectSection(item?.Tag?.ToString());
        }

        private async void OnRecipeIngredientDragStart(object? sender, PointerPressedEventArgs e)
        {
            if (sender is not Control { DataContext: RecipeIngredientItemViewModel item })
            {
                return;
            }

            _draggedRecipeItem = item;
#pragma warning disable CS0618
            var data = new DataObject();
            data.Set(DataFormats.Text, item.IngredientName);
            await DragDrop.DoDragDrop(e, data, DragDropEffects.Move);
#pragma warning restore CS0618
            _draggedRecipeItem = null;
        }

        private void OnRecipeIngredientDragOver(object? sender, DragEventArgs e)
        {
            if (_draggedRecipeItem == null ||
                sender is not Control { DataContext: RecipeIngredientItemViewModel targetItem } ||
                ReferenceEquals(_draggedRecipeItem, targetItem))
            {
                e.DragEffects = DragDropEffects.None;
                return;
            }

            e.DragEffects = DragDropEffects.Move;
            e.Handled = true;
        }

        private void OnRecipeIngredientDrop(object? sender, DragEventArgs e)
        {
            if (_draggedRecipeItem == null ||
                sender is not Control { DataContext: RecipeIngredientItemViewModel targetItem } ||
                DataContext is not MainWindowViewModel viewModel ||
                viewModel.SelectedRecipe == null)
            {
                return;
            }

            viewModel.SelectedRecipe.MoveIngredient(_draggedRecipeItem, targetItem);
            _draggedRecipeItem = null;
            e.Handled = true;
        }

        private void OnRecipeIngredientDropTargetAttached(object? sender, VisualTreeAttachmentEventArgs e)
        {
            if (sender is not Control control)
            {
                return;
            }

            DragDrop.SetAllowDrop(control, true);
            DragDrop.AddDragOverHandler(control, OnRecipeIngredientDragOver);
            DragDrop.AddDropHandler(control, OnRecipeIngredientDrop);
        }

        private void OnRecipeIngredientDropTargetDetached(object? sender, VisualTreeAttachmentEventArgs e)
        {
            if (sender is not Control control)
            {
                return;
            }

            DragDrop.RemoveDragOverHandler(control, OnRecipeIngredientDragOver);
            DragDrop.RemoveDropHandler(control, OnRecipeIngredientDrop);
        }
    }
}
