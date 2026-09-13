using FluentValidation;
using FluentValidation.Results;
using MechanicShop.Application.Common.Behaviours;
using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Application.Features.WorkOrders.Mappers;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Tests.Common.WorkOrders;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace MechanicShop.Application.UnitTests.Behaviours;

public class ValidationBehaviourTests
{
    private readonly ILogger<ValidationBehaviour<DummyCommand, Result<WorkOrderDto>>> _logger;
    private readonly List<IValidator<DummyCommand>> _mockValidators;
    private readonly RequestHandlerDelegate<Result<WorkOrderDto>> _mockNextBehaviour;

    private readonly ValidationBehaviour<DummyCommand, Result<WorkOrderDto>> _sut;

    public ValidationBehaviourTests()
    {
        _logger = Substitute.For<ILogger<ValidationBehaviour<DummyCommand, Result<WorkOrderDto>>>>();
        _mockValidators =
         [
             Substitute.For<IValidator<DummyCommand>>(),
            Substitute.For<IValidator<DummyCommand>>()
         ];
        _mockNextBehaviour = Substitute.For<RequestHandlerDelegate<Result<WorkOrderDto>>>();
        _sut = new ValidationBehaviour<DummyCommand, Result<WorkOrderDto>>(_logger, _mockValidators);
    }

    [Fact]
    public async Task Handle_ShouldInvokeNext_WhenValidatorsResultIsValid()
    {
        var command = new DummyCommand();
        var response = WorkOrderFactory.CreateWorkOrder().Value.ToDto();

        foreach (var validator in _mockValidators)
        {
            validator.ValidateAsync(command, Arg.Any<CancellationToken>()).Returns(new ValidationResult());
        }

        _mockNextBehaviour.Invoke().Returns(response);

        var result = await _sut.Handle(command, _mockNextBehaviour, CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _mockNextBehaviour.Received(1).Invoke();
        Assert.Equal(response, result.Value);
    }

    [Fact]
    public async Task Handle_ShouldReturnListOfError_WhenValidatorsResultIsNotValid()
    {
        var command = new DummyCommand();
        var response = WorkOrderFactory.CreateWorkOrder().Value.ToDto();

        var error1 = Error.Validation();
        var error2 = Error.Conflict();

        List<ValidationFailure> validationFailures = [new ValidationFailure(error1.Code, error1.Description), new(error2.Code, error2.Description)];

        foreach (var validator in _mockValidators)
        {
            validator.ValidateAsync(command, Arg.Any<CancellationToken>()).Returns(new ValidationResult(validationFailures));
        }

        var result = await _sut.Handle(command, _mockNextBehaviour, CancellationToken.None);

        Assert.False(result.IsSuccess);
        await _mockNextBehaviour.DidNotReceive().Invoke();

        foreach (var error in validationFailures)
        {
            Assert.Contains(result.Errors, e => e.Code == error.PropertyName && e.Description == error.ErrorMessage);
        }
    }

    [Fact]
    public async Task Handle_ShouldReturnListOfError_WhenAtLeastValidatorResultIsNotValid()
    {
        var command = new DummyCommand();
        var response = WorkOrderFactory.CreateWorkOrder().Value.ToDto();

        foreach (var validator in _mockValidators)
        {
            validator.ValidateAsync(command, Arg.Any<CancellationToken>()).Returns(new ValidationResult());
        }

        var error1 = Error.Validation();
        var error2 = Error.Conflict();

        List<ValidationFailure> validationFailures = [new ValidationFailure(error1.Code, error1.Description), new(error2.Code, error2.Description)];

        _mockValidators.Last().ValidateAsync(command, Arg.Any<CancellationToken>()).Returns(new ValidationResult(validationFailures));

        var result = await _sut.Handle(command, _mockNextBehaviour, CancellationToken.None);

        Assert.False(result.IsSuccess);
        await _mockNextBehaviour.DidNotReceive().Invoke();

        foreach (var error in validationFailures)
        {
            Assert.Contains(result.Errors, e => e.Code == error.PropertyName && e.Description == error.ErrorMessage);
        }
    }

    [Theory]
    [MemberData(nameof(GetNoValidators))]
    public async Task Handle_ShouldInvokeNext_WhenNoValidators(List<IValidator<DummyCommand>>? validators)
    {
        var command = new DummyCommand();
        var response = WorkOrderFactory.CreateWorkOrder().Value.ToDto();

        var validationBehaviour = new ValidationBehaviour<DummyCommand, Result<WorkOrderDto>>(_logger, validators);

        _mockNextBehaviour.Invoke().Returns(response);

        var result = await validationBehaviour.Handle(command, _mockNextBehaviour, CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _mockNextBehaviour.Received(1).Invoke();
        Assert.Equal(response, result.Value);
    }

    public static TheoryData<List<IValidator<DummyCommand>>?> GetNoValidators() => new TheoryData<List<IValidator<DummyCommand>>?>()
    {
        null,
        new List<IValidator<DummyCommand>>()
    };

    public class DummyCommand;
}
