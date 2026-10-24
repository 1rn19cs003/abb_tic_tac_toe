using TicTacToe.Api.Models.DTOs;
using TicTacToe.Api.Repositories;

namespace TicTacToe.Api.Services;

public class ScoreboardService : IScoreboardService
{
    private readonly IGameRepository _repository;

    public ScoreboardService(IGameRepository repository)
    {
        _repository = repository;
    }

    public ScoreboardDto GetScoreboard()
    {
        var scoreboard = _repository.GetScoreboard();
        return new ScoreboardDto(scoreboard.XWins, scoreboard.OWins, scoreboard.Draws);
    }

    public ScoreboardDto ResetScoreboard()
    {
        _repository.ResetScoreboard();
        var scoreboard = _repository.GetScoreboard();
        return new ScoreboardDto(scoreboard.XWins, scoreboard.OWins, scoreboard.Draws);
    }
}
