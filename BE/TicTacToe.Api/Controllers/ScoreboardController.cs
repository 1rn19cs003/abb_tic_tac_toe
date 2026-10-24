using Microsoft.AspNetCore.Mvc;
using TicTacToe.Api.Models.DTOs;
using TicTacToe.Api.Services;

namespace TicTacToe.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScoreboardController : ControllerBase
{
    private readonly IScoreboardService _scoreboardService;

    public ScoreboardController(IScoreboardService scoreboardService)
    {
        _scoreboardService = scoreboardService;
    }

    [HttpGet]
    public ActionResult<ScoreboardDto> GetScoreboard()
    {
        var scoreboard = _scoreboardService.GetScoreboard();
        return Ok(scoreboard);
    }

    [HttpPost("reset")]
    public ActionResult<ScoreboardDto> ResetScoreboard()
    {
        var scoreboard = _scoreboardService.ResetScoreboard();
        return Ok(scoreboard);
    }
}
