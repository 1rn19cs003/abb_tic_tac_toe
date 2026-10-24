using System.Collections.Concurrent;
using TicTacToe.Api.Models.Domain;
using TicTacToe.Api.Models.Enums;

namespace TicTacToe.Api.Repositories;

public class InMemoryGameRepository : IGameRepository
{
    private readonly ConcurrentDictionary<Guid, GameSession> _games = new();
    private readonly Scoreboard _scoreboard = new();

    public GameSession CreateGame(GameMode mode)
    {
        var game = new GameSession(mode);
        _games[game.Id] = game;
        return game;
    }

    public GameSession? GetGame(Guid id)
    {
        _games.TryGetValue(id, out var game);
        return game;
    }

    public void SaveGame(GameSession game)
    {
        _games[game.Id] = game;
    }

    public Scoreboard GetScoreboard()
    {
        return _scoreboard;
    }

    public void ResetScoreboard()
    {
        _scoreboard.Reset();
    }
}
