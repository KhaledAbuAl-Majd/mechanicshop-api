using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.RepairTasks.Parts
{
    public static class PartErrors
    {
        public static readonly Error IdRequired =
            Error.Validation("Part.Id.Required", "Part ID is required.");

        public static readonly Error NameRequired =
       Error.Validation("Part.Name.Required", "Part name is required.");

        public static readonly Error CostInvalid =
            Error.Validation(
                "Part.Cost.Invalid",
                $"Part cost must be greater than {PartConstant.ExclusiveMinCost} and up to {PartConstant.MaxCost:N0}.");

        public static readonly Error QuantityInvalid =
            Error.Validation(
                "Part.Quantity.Invalid",
                $"Quantity must be between {PartConstant.MinQuantity} and {PartConstant.MaxQuantity}.");
    }
}
