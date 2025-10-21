using Labverse.API.Helpers;
using Labverse.BLL.DTOs.Labs;
using Labverse.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Labverse.API.Controllers;

[Route("api/labs/{labId:int}/questions")]
[ApiController]
public class QuestionsController : ControllerBase
{
    private readonly IQuestionService _questionService;

    public QuestionsController(IQuestionService questionService)
    {
        _questionService = questionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetQuestions([FromRoute] int labId)
    {
        try
        {
            var questions = await _questionService.GetLabQuestionsAsync(labId);
            return Ok(questions);
        }
        catch (Exception ex)
        {
            return ApiErrorHelper.Error("GET_QUESTIONS_ERROR", ex.Message, 500);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateQuestion(
        [FromRoute] int labId,
        [FromBody] CreateLabQuestionDto dto
    )
    {
        try
        {
            var created = await _questionService.AddQuestionAsync(labId, dto);
            return Ok(created);
        }
        catch (KeyNotFoundException ex)
        {
            return ApiErrorHelper.Error("LAB_NOT_FOUND", ex.Message, 404);
        }
        catch (Exception ex)
        {
            return ApiErrorHelper.Error("CREATE_QUESTION_ERROR", ex.Message, 500);
        }
    }

    [HttpPatch("{questionId:int}")]
    public async Task<IActionResult> UpdateQuestion(
        [FromRoute] int labId,
        [FromRoute] int questionId,
        [FromBody] UpdateLabQuestionDto dto
    )
    {
        try
        {
            var updated = await _questionService.UpdateQuestionAsync(questionId, dto);
            return Ok(updated);
        }
        catch (KeyNotFoundException ex)
        {
            return ApiErrorHelper.Error("QUESTION_NOT_FOUND", ex.Message, 404);
        }
        catch (Exception ex)
        {
            return ApiErrorHelper.Error("UPDATE_QUESTION_ERROR", ex.Message, 500);
        }
    }

    [HttpDelete("{questionId:int}")]
    public async Task<IActionResult> DeleteQuestion(
        [FromRoute] int labId,
        [FromRoute] int questionId
    )
    {
        try
        {
            await _questionService.DeleteQuestionAsync(questionId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return ApiErrorHelper.Error("QUESTION_NOT_FOUND", ex.Message, 404);
        }
        catch (Exception ex)
        {
            return ApiErrorHelper.Error("DELETE_QUESTION_ERROR", ex.Message, 500);
        }
    }

    [HttpPost("{questionId:int}/answers")]
    [Authorize(Roles = "user")]
    public async Task<IActionResult> SubmitAnswer(
        [FromRoute] int labId,
        [FromRoute] int questionId,
        [FromBody] SubmitAnswerRequest request
    )
    {
        try
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdStr) || !int.TryParse(userIdStr, out var userId))
                return ApiErrorHelper.Error("UNAUTHORIZED", "User not authenticated", 401);

            var result = await _questionService.SubmitAnswerAsync(
                userId,
                labId,
                questionId,
                request
            );
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return ApiErrorHelper.Error("INVALID_INPUT", ex.Message, 400);
        }
        catch (KeyNotFoundException ex)
        {
            return ApiErrorHelper.Error("NOT_FOUND", ex.Message, 404);
        }
        catch (Exception ex)
        {
            return ApiErrorHelper.Error("SUBMIT_ANSWER_ERROR", ex.Message, 500);
        }
    }
}
