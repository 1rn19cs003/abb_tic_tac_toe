using TicTacToe.Api.Models.DTOs;
using TicTacToe.Api.Models.Enums;

namespace TicTacToe.Api.Services;

public interface IGameService
{
    GameStateResponse CreateGame(GameMode mode);
    GameStateResponse? GetGameState(Guid id);
    GameStateResponse MakeMove(Guid id, string player, int row, int col);
    GameStateResponse UndoMove(Guid id);
    GameStateResponse ResetGame(Guid id);
}
