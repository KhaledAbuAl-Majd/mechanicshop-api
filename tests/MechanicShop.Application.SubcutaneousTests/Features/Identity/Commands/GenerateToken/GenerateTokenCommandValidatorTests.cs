using MechanicShop.Application.Features.Identity.Commands.GenerateToken;

namespace MechanicShop.Application.SubcutaneousTests.Features.Identity.Commands.GenerateToken
{
    public class GenerateTokenCommandValidatorTests
    {
        private readonly GenerateTokenCommandValidator _validator = new();


        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("dfdda")]
        public async Task Validate_ShouldFail_WhenInvalidEmail(string? email)
        {
            var ct = CancellationToken.None;

            var command = new GenerateTokenCommand(email!, "123434");

            var result = await _validator.ValidateAsync(command, ct);

            Assert.False(result.IsValid);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_ShouldFail_WhenPassword(string? password)
        {
            var ct = CancellationToken.None;

            var command = new GenerateTokenCommand("khaled@gmail.com"!, password!);

            var result = await _validator.ValidateAsync(command, ct);

            Assert.False(result.IsValid);
        }

        [Fact]
        public async Task Validate_ShouldSuccess_WhenValidData()
        {
            var ct = CancellationToken.None;

            var command = new GenerateTokenCommand("khaled@gmail.com"!, "12343");

            var result = await _validator.ValidateAsync(command, ct);

            Assert.True(result.IsValid);
        }
    }
}
