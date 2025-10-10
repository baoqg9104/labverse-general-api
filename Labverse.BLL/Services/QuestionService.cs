using Labverse.BLL.DTOs.Labs;
using Labverse.BLL.Interfaces;
using Labverse.DAL.EntitiesModels;
using Labverse.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Labverse.BLL.Services;

public class QuestionService : IQuestionService
{
    private readonly IUnitOfWork _unitOfWork;

    public QuestionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    private static string BuildCorrectAnswerJson(QuestionType type, string? correctText, string[]? correctOptions, bool? correctBool)
    {
        return type switch
        {
            QuestionType.SingleChoice => JsonSerializer.Serialize(correctText ?? string.Empty),
            QuestionType.MultipleChoice => JsonSerializer.Serialize(correctOptions ?? Array.Empty<string>()),
            QuestionType.TrueFalse => JsonSerializer.Serialize(correctBool ?? false),
            QuestionType.ShortText => JsonSerializer.Serialize((correctText ?? string.Empty).Trim()),
            _ => "[]"
        };
    }

    private static string? BuildChoicesJson(string[]? choices)
    {
        return choices == null ? null : JsonSerializer.Serialize(choices);
    }

    public async Task<LabQuestionDto> AddQuestionAsync(int labId, CreateLabQuestionDto dto)
    {
        var lab =
            await _unitOfWork.Labs.GetByIdAsync(labId)
            ?? throw new KeyNotFoundException("Lab not found");

        var question = new LabQuestion
        {
            LabId = labId,
            QuestionText = dto.QuestionText,
            Type = dto.Type,
            ChoicesJson = BuildChoicesJson(dto.Choices),
            CorrectAnswerJson = BuildCorrectAnswerJson(dto.Type, dto.CorrectText, dto.CorrectOptions, dto.CorrectBool)
        };
        await _unitOfWork.LabQuestions.AddAsync(question);
        await _unitOfWork.SaveChangesAsync();
        return new LabQuestionDto
        {
            Id = question.Id,
            LabId = labId,
            QuestionText = question.QuestionText,
            Type = question.Type,
            ChoicesJson = question.ChoicesJson
        };
    }

    public async Task<LabQuestionDto> UpdateQuestionAsync(int questionId, UpdateLabQuestionDto dto)
    {
        var q =
            await _unitOfWork.LabQuestions.GetByIdAsync(questionId)
            ?? throw new KeyNotFoundException("Question not found");
        if (!string.IsNullOrWhiteSpace(dto.QuestionText))
            q.QuestionText = dto.QuestionText;
        if (dto.Type.HasValue)
            q.Type = dto.Type.Value;
        if (dto.Choices != null)
            q.ChoicesJson = BuildChoicesJson(dto.Choices);
        if (dto.CorrectText != null || dto.CorrectOptions != null || dto.CorrectBool.HasValue)
            q.CorrectAnswerJson = BuildCorrectAnswerJson(q.Type, dto.CorrectText, dto.CorrectOptions, dto.CorrectBool);
        _unitOfWork.LabQuestions.Update(q);
        await _unitOfWork.SaveChangesAsync();
        return new LabQuestionDto
        {
            Id = q.Id,
            LabId = q.LabId,
            QuestionText = q.QuestionText,
            Type = q.Type,
            ChoicesJson = q.ChoicesJson,
        };
    }

    public async Task DeleteQuestionAsync(int questionId)
    {
        var q =
            await _unitOfWork.LabQuestions.GetByIdAsync(questionId)
            ?? throw new KeyNotFoundException("Question not found");
        _unitOfWork.LabQuestions.Remove(q);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<LabQuestionDto>> GetLabQuestionsAsync(int labId)
    {
        var questions = await _unitOfWork
            .LabQuestions.Query()
            .Where(q => q.LabId == labId)
            .ToListAsync();
        return questions.Select(q => new LabQuestionDto
        {
            Id = q.Id,
            LabId = q.LabId,
            QuestionText = q.QuestionText,
            Type = q.Type,
            ChoicesJson = q.ChoicesJson,
        });
    }

    public async Task<SubmitAnswerResponse> SubmitAnswerAsync(
        int userId,
        int labId,
        int questionId,
        SubmitAnswerRequest request
    )
    {
        // validate lab and question
        var question = await _unitOfWork.LabQuestions.GetByIdAsync(questionId);
        if (question == null || question.LabId != labId)
            throw new InvalidOperationException("Invalid question or lab");

        var user =
            await _unitOfWork.Users.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found");

        bool isCorrect = EvaluateAnswer(question, request.AnswerJson);

        // Upsert user answer
        var existing = await _unitOfWork
            .UserLabAnswers.Query()
            .FirstOrDefaultAsync(a => a.UserId == userId && a.QuestionId == questionId);

        var wasCorrectBefore = false;
        if (existing == null)
        {
            existing = new UserLabAnswer
            {
                UserId = userId,
                LabId = labId,
                QuestionId = questionId,
                AnswerJson = request.AnswerJson,
                IsCorrect = isCorrect,
            };
            await _unitOfWork.UserLabAnswers.AddAsync(existing);
        }
        else
        {
            // track if it was previously correct
            wasCorrectBefore = existing.IsCorrect;
            existing.AnswerJson = request.AnswerJson;
            existing.IsCorrect = existing.IsCorrect || isCorrect;
            _unitOfWork.UserLabAnswers.Update(existing);
        }

        int awarded = 0;

        // XP: +10 when newly correct
        if (isCorrect && !wasCorrectBefore)
        {
            user.Points += 10; // Points used as XP storage
            awarded += 10;
        }

        // Track streak (daily activity when submitting an answer)
        bool streakIncreased = UpdateStreak(user);

        // Streak milestone: +100 XP at 7-day streak (and multiples)
        if (streakIncreased && user.StreakCurrent >= 7 && user.StreakCurrent % 7 == 0)
        {
            if (user.LastStreakBonusAtDays < user.StreakCurrent)
            {
                user.Points += 100;
                awarded += 100;
                user.LastStreakBonusAtDays = user.StreakCurrent;
            }
        }

        // Check if lab completed (all questions for the lab answered correctly)
        var questionIds = await _unitOfWork
            .LabQuestions.Query()
            .Where(q => q.LabId == labId)
            .Select(q => q.Id)
            .ToListAsync();
        bool labCompleted = false;
        if (questionIds.Count > 0)
        {
            var correctCount = await _unitOfWork
                .UserLabAnswers.Query()
                .Where(a =>
                    a.UserId == userId
                    && a.LabId == labId
                    && a.IsCorrect
                    && questionIds.Contains(a.QuestionId)
                )
                .Select(a => a.QuestionId)
                .Distinct()
                .CountAsync();
            if (correctCount == questionIds.Count)
            {
                labCompleted = true;

                // Ensure we only award completion bonus once using UserProgress
                var progress = await _unitOfWork
                    .UserProgresses.Query()
                    .FirstOrDefaultAsync(up => up.UserId == userId && up.LabId == labId);

                if (progress == null || progress.Status != ProgressStatus.Completed)
                {
                    user.Points += 50; // +50 XP for completing a lab
                    awarded += 50;

                    // mark progress completed
                    if (progress == null)
                    {
                        progress = new UserProgress
                        {
                            UserId = userId,
                            LabId = labId,
                            Status = ProgressStatus.Completed,
                            StartedAt = DateTime.UtcNow,
                            CompletedAt = DateTime.UtcNow,
                        };
                        await _unitOfWork.UserProgresses.AddAsync(progress);
                    }
                    else
                    {
                        progress.Status = ProgressStatus.Completed;
                        progress.CompletedAt = DateTime.UtcNow;
                        _unitOfWork.UserProgresses.Update(progress);
                    }
                }
            }
        }

        // Level up logic based on XP thresholds
        var beforeLevel = user.Level;
        ApplyLevelUps(user);
        var afterLevel = user.Level;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return new SubmitAnswerResponse
        {
            IsCorrect = isCorrect,
            AwardedXp = awarded,
            LabCompleted = labCompleted,
            TotalUserXp = user.Points,
            NewLevel = afterLevel,
        };
    }

    private static bool EvaluateAnswer(LabQuestion question, string answerJson)
    {
        try
        {
            switch (question.Type)
            {
                case QuestionType.TrueFalse:
                case QuestionType.SingleChoice:
                case QuestionType.ShortText:
                    {
                        var a = JsonDocument
                            .Parse(answerJson)
                            .RootElement.ToString()
                            .Trim()
                            .ToLowerInvariant();
                        var c = JsonDocument
                            .Parse(question.CorrectAnswerJson)
                            .RootElement.ToString()
                            .Trim()
                            .ToLowerInvariant();
                        return a == c;
                    }
                case QuestionType.MultipleChoice:
                    {
                        var a = JsonSerializer.Deserialize<List<string>>(answerJson) ?? new();
                        var c =
                            JsonSerializer.Deserialize<List<string>>(question.CorrectAnswerJson)
                            ?? new();
                        a.Sort();
                        c.Sort();
                        return a.SequenceEqual(c);
                    }
                default:
                    return false;
            }
        }
        catch
        {
            return false;
        }
    }

    private static bool UpdateStreak(User user)
    {
        var today = DateTime.UtcNow.Date;
        var increased = false;
        if (user.LastActiveAt == null)
        {
            user.StreakCurrent = 1;
            increased = true;
        }
        else
        {
            var last = user.LastActiveAt.Value.Date;
            if (last == today)
            {
                // same day, do not change
            }
            else if (last == today.AddDays(-1))
            {
                user.StreakCurrent += 1;
                increased = true;
            }
            else
            {
                user.StreakCurrent = 1;
                increased = true;
            }
        }
        user.LastActiveAt = DateTime.UtcNow;
        if (user.StreakCurrent > user.StreakBest)
            user.StreakBest = user.StreakCurrent;
        return increased;
    }

    // Increment user level based on XP thresholds and award badges at milestones
    private void ApplyLevelUps(User user)
    {
        // Example thresholds: level n requires XP >= 100 * n * (n-1) / 2 (increasing pace)
        // Simpler: level up every 100 XP
        var thresholds = new Func<int, int>(lvl => lvl * 100);
        var leveledUp = false;
        while (user.Points >= thresholds(user.Level))
        {
            user.Level += 1;
            leveledUp = true;
            TryAwardLevelBadge(user);
        }
        if (leveledUp)
        {
            user.UpdatedAt = DateTime.UtcNow;
        }
    }

    private void TryAwardLevelBadge(User user)
    {
        // Example milestones
        var milestones = new HashSet<int> { 3, 5, 10, 20 };
        if (!milestones.Contains(user.Level)) return;

        // Find a badge by name pattern or create if needed
        var badgeName = $"Level {user.Level}";
        var badge = _unitOfWork.Badges.Query().FirstOrDefault(b => b.Name == badgeName);
        if (badge == null)
        {
            badge = new DAL.EntitiesModels.Badge
            {
                Name = badgeName,
                Description = $"Reached level {user.Level}",
                IconUrl = string.Empty
            };
            // Persist badge
            _unitOfWork.Badges.AddAsync(badge).GetAwaiter().GetResult();
        }

        // Check if user already has it
        var hasIt = _unitOfWork.UserBadges.Query().Any(ub => ub.UserId == user.Id && ub.BadgeId == badge.Id);
        if (!hasIt)
        {
            var userBadge = new DAL.EntitiesModels.UserBadge
            {
                UserId = user.Id,
                BadgeId = badge.Id,
                DateAwarded = DateTime.UtcNow
            };
            _unitOfWork.UserBadges.AddAsync(userBadge).GetAwaiter().GetResult();
        }
    }
}
