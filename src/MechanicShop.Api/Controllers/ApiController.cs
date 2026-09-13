using MechanicShop.Domain.Common.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MechanicShop.Api.Controllers
{
    [ApiController]
    public abstract class ApiController : ControllerBase
    {
        protected ActionResult Problem(List<Error> errors)
        {
            if (errors is { Count: 0 })
            {
                return Problem();// 500 - internal server error 
            }

            if (errors.All(e => e.Type is ErrorKind.Validation))
            {
                return ValidationProblem(errors);
            }

            return Problem(errors[0]);
        }

        private ObjectResult Problem(Error error)
        {
            var (statusCode, title) = error.Type switch
            {
                ErrorKind.Conflict => (StatusCodes.Status409Conflict, "Conflict"),
                ErrorKind.Validation => (StatusCodes.Status400BadRequest, "Bad Request"),
                ErrorKind.NotFound => (StatusCodes.Status404NotFound, "Not Found"),
                ErrorKind.Unauthorized => (StatusCodes.Status401Unauthorized, "Unauthorized"),
                ErrorKind.Forbidden => (StatusCodes.Status403Forbidden, "Forbidden"),
                _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
            };

            return Problem(statusCode: statusCode, title: title, detail: error.Description);
        }

        private ActionResult ValidationProblem(List<Error> errors)
        {
            var modelStateDictionary = new ModelStateDictionary();

            foreach (var error in errors)
            {
                modelStateDictionary.AddModelError(error.Code, error.Description);
            }

            return ValidationProblem(modelStateDictionary);
        }
    }
}
