using MechanicShop.Domain.Common;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.RepairTasks.Parts
{
    public sealed class Part : AuditableEntity
    {
        public string? Name { get; private set; }
        public decimal Cost { get; private set; }
        public int Quantity { get; private set; }

        private Part() { }

        private Part(Guid id, string name, decimal cost, int quantity) : base(id)
        {
            Name = name;
            Cost = cost;
            Quantity = quantity;
        }


        public static Result<Part> Create(Guid id, string name, decimal cost, int quantity)
        {
            if (id == Guid.Empty)
                return PartErrors.IdRequired;

            if (string.IsNullOrWhiteSpace(name))
                return PartErrors.NameRequired;

            if (cost <= PartConstant.ExclusiveMinCost || cost > PartConstant.MaxCost)
                return PartErrors.CostInvalid;

            if (quantity < PartConstant.MinQuantity || quantity > PartConstant.MaxQuantity)
                return PartErrors.QuantityInvalid;

            return new Part(id, name.Trim(), cost, quantity);
        }
        public Result<Updated> Update(string name, decimal cost, int quantity)
        {
            if (string.IsNullOrWhiteSpace(name))
                return PartErrors.NameRequired;

            if (cost <= PartConstant.ExclusiveMinCost || cost > PartConstant.MaxCost)
                return PartErrors.CostInvalid;

            if (quantity < PartConstant.MinQuantity || quantity > PartConstant.MaxQuantity)
                return PartErrors.QuantityInvalid;

            Name = name.Trim();
            Cost = cost;
            Quantity = quantity;

            return Result.Updated;
        }
    }
}
