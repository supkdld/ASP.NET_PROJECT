using Microsoft.AspNetCore.Mvc;

namespace ProjectASPNET.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    protected const int MaxPageSize = 100;

    protected static List<T> GetPage<T>(IQueryable<T> orderedQuery, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        return orderedQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    protected ActionResult FieldError(string field, string message)
    {
        ModelState.AddModelError(field, message);
        return ValidationProblem(ModelState);
    }

    protected ActionResult ConflictError(string message)
    {
        return Problem(detail: message, statusCode: StatusCodes.Status409Conflict, title: "Conflict");
    }
}
