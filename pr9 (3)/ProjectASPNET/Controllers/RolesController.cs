using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectASPNET.DTOs;
using ProjectASPNET.Mappers;
using ProjectASPNET.Models;

namespace ProjectASPNET.Controllers;

[ApiController]
[Route("api/roles")]
[Produces("application/json")]
public class RolesController : ApiControllerBase
{
    private readonly PharmacyContext _context;

    public RolesController(PharmacyContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RoleDto>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<RoleDto>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _context.Roles
            .AsNoTracking()
            .OrderBy(r => r.IdRole);

        return Ok(GetPage(query, page, pageSize).Select(RoleMapper.ToDto).ToList());
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<RoleDto> GetById(int id)
    {
        var role = _context.Roles
            .AsNoTracking()
            .FirstOrDefault(r => r.IdRole == id);

        if (role == null)
        {
            return NotFound();
        }

        return Ok(RoleMapper.ToDto(role));
    }

    [HttpPost]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult<RoleDto> Create(CreateRoleDto dto)
    {
        if (NameExists(dto.NameOfRole))
        {
            return ConflictError($"Роль '{dto.NameOfRole}' уже существует.");
        }

        var role = new Role
        {
            NameOfRole = dto.NameOfRole
        };

        _context.Roles.Add(role);
        _context.SaveChanges();

        var result = RoleMapper.ToDto(role);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Update(int id, UpdateRoleDto dto)
    {
        var role = _context.Roles.Find(id);

        if (role == null)
        {
            return NotFound();
        }

        if (NameExists(dto.NameOfRole, id))
        {
            return ConflictError($"Роль '{dto.NameOfRole}' уже существует.");
        }

        role.NameOfRole = dto.NameOfRole;

        _context.SaveChanges();

        return NoContent();
    }

    [HttpPatch("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Patch(int id, PatchRoleDto dto)
    {
        var role = _context.Roles.Find(id);

        if (role == null)
        {
            return NotFound();
        }

        if (dto.NameOfRole != null)
        {
            if (NameExists(dto.NameOfRole, id))
            {
                return ConflictError($"Роль '{dto.NameOfRole}' уже существует.");
            }

            role.NameOfRole = dto.NameOfRole;
        }

        _context.SaveChanges();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Delete(int id)
    {
        var role = _context.Roles.Find(id);

        if (role == null)
        {
            return NotFound();
        }

        if (_context.Users.Any(u => u.RoleId == id))
        {
            return ConflictError("Нельзя удалить роль, которая назначена пользователям.");
        }

        _context.Roles.Remove(role);
        _context.SaveChanges();

        return NoContent();
    }

    private bool NameExists(string name, int? excludeId = null)
    {
        return _context.Roles
            .Any(r => r.NameOfRole == name && (excludeId == null || r.IdRole != excludeId.Value));
    }
}
