namespace KALKULATORBAHAN.ViewModels
{
    public class PlannerRequirementItemViewModel
    {
        public required string IngredientName { get; init; }

        public required string UnitName { get; init; }

        public decimal Quantity { get; init; }

        public string QuantityText => $"{Quantity:N2} {UnitName}";
    }
}
