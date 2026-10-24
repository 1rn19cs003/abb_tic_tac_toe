using Microsoft.AspNetCore.Mvc;
using TicTacToe.Api.Models.Domain;
using TicTacToe.Api.Models.DTOs;
using TicTacToe.Api.Models.Enums;
using TicTacToe.Api.Services;

namespace TicTacToe.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
    private readonly IGameService _gameService;

    public GamesController(IGameService gameService)
    {
        _gameService = gameService;
    }

    [HttpPost]
    public ActionResult<GameStateResponse> CreateGame([FromBody] CreateGameRequest? request)
    {
        var mode = request?.Mode ?? GameMode.TwoPlayer;
        var result = _gameService.CreateGame(mode);
        return CreatedAtAction(nameof(GetGame), new { id = result.GameId }, result);
    }

    [HttpGet("{id:guid}")]
    public ActionResult<GameStateResponse> GetGame(Guid id)
    {
        var result = _gameService.GetGameState(id);
        if (result == null)
        {
            return NotFound(new { message = $"Game session with ID '{id}' was not found." });
        }
        return Ok(result);
    }

    [HttpPost("{id:guid}/moves")]
    public ActionResult<GameStateResponse> MakeMove(Guid id, [FromBody] MakeMoveRequest request)
    {
        if (request == null)
        {
            return BadRequest(new { message = "Move request payload cannot be empty." });
        }

        try
        {
            var result = _gameService.MakeMove(id, request.Player, request.Row, request.Col);
            return Ok(result);
        }
        catch (GameOperationException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/undo")]
    public ActionResult<GameStateResponse> UndoMove(Guid id)
    {
        try
        {
            var result = _gameService.UndoMove(id);
            return Ok(result);
        }
        catch (GameOperationException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/reset")]
    public ActionResult<GameStateResponse> ResetGame(Guid id)
    {
        try
        {
            var result = _gameService.ResetGame(id);
            return Ok(result);
        }
        catch (GameOperationException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }
}
