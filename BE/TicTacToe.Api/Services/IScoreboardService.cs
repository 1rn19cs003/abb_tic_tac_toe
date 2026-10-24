using TicTacToe.Api.Models.DTOs;

namespace TicTacToe.Api.Services;

public interface IScoreboardService
{
    ScoreboardDto GetScoreboard();
    ScoreboardDto ResetScoreboard();
}
