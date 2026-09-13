using System;
using System.Collections.Generic;
using System.Text;
using MechanicShop.Application.Features.Customers.Commands.RemoveCustomer;
using MechanicShop.Application.Features.Customers.Queries.GetCustomerById;
using MechanicShop.Domain.Customers;

namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Queries.GetCustomerById;

public class GetCustomerByIdQueryValidatorTests
{
    private readonly GetCustomerByIdQueryValidator _validator = new();

    [Fact]
    public async Task Validate_ShouldFail_WhenCustomerIdInvalid()
    {
        CancellationToken ct = CancellationToken.None;
        var query = new GetCustomerByIdQuery(Guid.Empty);

        var result = await _validator.ValidateAsync(query, ct);

        Assert.False(result.IsValid);
        Assert.Equal(CustomerErrors.IdRequired.Code, result.Errors[0].ErrorCode);
    }


    [Fact]
    public async Task Validate_ShouldSuccess_WhenValidData()
    {
        CancellationToken ct = CancellationToken.None;
        var query = new GetCustomerByIdQuery(Guid.NewGuid());

        var result = await _validator.ValidateAsync(query, ct);

        Assert.True(result.IsValid);
    }
}
