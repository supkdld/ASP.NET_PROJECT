using ProjectASPNET.DTOs;
using ProjectASPNET.Models;

namespace ProjectASPNET.Mappers;

public static class RoleMapper
{
    public static RoleDto ToDto(Role role)
    {
        return new RoleDto
        {
            Id = role.IdRole,
            NameOfRole = role.NameOfRole
        };
    }
}
