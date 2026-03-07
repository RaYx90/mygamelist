namespace GameList.Domain.Exceptions;

public sealed class GameNotFoundException : DomainException
{
    public GameNotFoundException(int id) : base($"Game with id '{id}' was not found.") { }
}
