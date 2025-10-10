using Labverse.BLL.DTOs.Labs;

namespace Labverse.BLL.Interfaces;

public interface IQuestionService
{
    Task<IEnumerable<LabQuestionDto>> GetLabQuestionsAsync(int labId);
    Task<LabQuestionDto> AddQuestionAsync(int labId, CreateLabQuestionDto dto);
    Task<LabQuestionDto> UpdateQuestionAsync(int questionId, UpdateLabQuestionDto dto);
    Task DeleteQuestionAsync(int questionId);
    Task<SubmitAnswerResponse> SubmitAnswerAsync(int userId, int labId, int questionId, SubmitAnswerRequest request);
}
