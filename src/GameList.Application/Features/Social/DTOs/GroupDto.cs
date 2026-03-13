namespace GameList.Application.Features.Social.DTOs;

/// <summary>
/// DTO con los datos de un grupo social de usuarios.
/// </summary>
/// <param name="Id">Identificador interno del grupo.</param>
/// <param name="Name">Nombre del grupo.</param>
/// <param name="InviteCode">Código de invitación para unirse al grupo.</param>
/// <param name="MemberUsernames">Lista de nombres de usuario de los miembros del grupo.</param>
public sealed record GroupDto(int Id, string Name, string InviteCode, IReadOnlyList<string> MemberUsernames);
