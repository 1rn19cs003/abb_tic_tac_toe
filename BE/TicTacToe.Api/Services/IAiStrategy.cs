using TicTacToe.Api.Models.Domain;

namespace TicTacToe.Api.Services;

public interface IAiStrategy
{
    CellPosition? DetermineMove(string?[,] board, string aiPlayer = "O", string humanPlayer = "X");
}
