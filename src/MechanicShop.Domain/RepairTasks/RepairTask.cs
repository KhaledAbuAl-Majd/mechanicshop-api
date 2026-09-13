using MechanicShop.Domain.Common;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks.Enums;
using MechanicShop.Domain.RepairTasks.Parts;

namespace MechanicShop.Domain.RepairTasks
{
    public sealed class RepairTask : AuditableEntity
    {
        public string Name { get; private set; }
        public decimal LaborCost { get; private set; }
        public RepairDurationInMinutes EstimatedDurationInMins { get; private set; }

        private readonly List<Part> _parts = [];
        public IReadOnlyCollection<Part> Parts => _parts.AsReadOnly();

        public decimal TotalCost => LaborCost + _parts.Sum(p => p.Quantity * p.Cost);

#pragma warning disable CS8618
        private RepairTask() { }
#pragma warning restore CS8618

        private RepairTask(Guid id, string name, decimal laborCost, RepairDurationInMinutes estimatedDurationInMins, List<Part> parts) : base(id)
        {
            Name = name;
            LaborCost = laborCost;
            EstimatedDurationInMins = estimatedDurationInMins;
            _parts = parts ?? [];
        }

        public static Result<RepairTask> Create(Guid id, string name, decimal laborCost, RepairDurationInMinutes estimatedDurationInMins, List<Part> parts)
        {
            if (id == Guid.Empty)
                return RepairTaskErrors.IdRequired;

            if (string.IsNullOrWhiteSpace(name))
                return RepairTaskErrors.NameRequired;

            if (laborCost < RepairTaskConstant.MinLaborCost || laborCost > RepairTaskConstant.MaxLaborCost)
                return RepairTaskErrors.LaborCostInvalid;

            if (!Enum.IsDefined(estimatedDurationInMins))
                return RepairTaskErrors.DurationInvalid;

            if (parts is null || parts.Count == 0)
                return RepairTaskErrors.PartsRequired;

            if (parts.DistinctBy(p => p?.Name?.Trim(), StringComparer.OrdinalIgnoreCase).Count() < parts.Count)
                return RepairTaskErrors.PartsDuplicateName;

            return new RepairTask(id, name.Trim(), laborCost, estimatedDurationInMins, parts);
        }

        public Result<Updated> Update(string name, decimal laborCost, RepairDurationInMinutes estimatedDurationInMins)
        {
            if (string.IsNullOrWhiteSpace(name))
                return RepairTaskErrors.NameRequired;

            if (laborCost < RepairTaskConstant.MinLaborCost || laborCost > RepairTaskConstant.MaxLaborCost)
                return RepairTaskErrors.LaborCostInvalid;

            if (!Enum.IsDefined(estimatedDurationInMins))
                return RepairTaskErrors.DurationInvalid;

            Name = name.Trim();
            LaborCost = laborCost;
            EstimatedDurationInMins = estimatedDurationInMins;

            return Result.Updated;
        }

        public Result<Updated> UpsertParts(List<Part> incomingParts)
        {
            if (incomingParts is null || incomingParts.Count == 0)
                return RepairTaskErrors.PartsRequired;

            if (incomingParts.DistinctBy(p => p?.Name?.Trim(), StringComparer.OrdinalIgnoreCase).Count() < incomingParts.Count)
                return RepairTaskErrors.PartsDuplicateName;

            //remove deleted parts
            _parts.RemoveAll(existing => incomingParts.All(p => p.Id != existing.Id));

            foreach (var incoming in incomingParts)
            {
                var existing = _parts.FirstOrDefault(p => p.Id == incoming.Id);

                if (existing is null)
                {
                    _parts.Add(incoming);
                }
                else
                {
                    var updateResult = existing.Update(incoming.Name!, incoming.Cost, incoming.Quantity);

                    if (updateResult.IsError)
                        return updateResult.Errors;
                }
            }

            return Result.Updated;
        }
    }
}
