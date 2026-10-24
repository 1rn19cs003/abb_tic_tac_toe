using TicTacToe.Api.Models.Domain;
using TicTacToe.Api.Models.Enums;

namespace TicTacToe.Api.Repositories;

public interface IGameRepository
{
    GameSession CreateGame(GameMode mode);
    GameSession? GetGame(Guid id);
    void SaveGame(GameSession game);
    Scoreboard GetScoreboard();
    void ResetScoreboard();
}
