namespace QuizGame.Core.Enums;

public enum SkillScoreConfidence
{
    Unrated,    // fewer than 5 quizzes
    Low,        // 5-19 quizzes
    Medium,     // 20-49 quizzes
    High,       // 50-99 quizzes
    VeryHigh    // 100+ quizzes
}
