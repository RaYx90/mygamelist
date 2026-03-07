namespace GameList.Application.Features.Social.DTOs;

public sealed record GroupDto(int Id, string Name, string InviteCode, IReadOnlyList<string> MemberUsernames);
