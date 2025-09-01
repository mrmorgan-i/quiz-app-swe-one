using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net;

// Data models for API response
public class TriviaResponse
{
    [JsonPropertyName("response_code")]
    public int ResponseCode { get; set; }

    [JsonPropertyName("results")]
    public List<TriviaQuestion> Results { get; set; } = new();
}

public class TriviaQuestion
{
    [JsonPropertyName("category")]
    public string Category { get; set; } = "";

    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("difficulty")]
    public string Difficulty { get; set; } = "";

    [JsonPropertyName("question")]
    public string Question { get; set; } = "";

    [JsonPropertyName("correct_answer")]
    public string CorrectAnswer { get; set; } = "";

    [JsonPropertyName("incorrect_answers")]
    public List<string> IncorrectAnswers { get; set; } = new();
}

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("ULTIMATE TRIVIA CHALLENGE");
        Console.WriteLine("================================");

        try
        {
            // Get user preferences
            var numQuestions = GetNumberOfQuestions();
            var difficulty = GetDifficulty();

            Console.WriteLine("\nFetching your trivia questions...\n");

            // Fetch questions from API
            var questions = await FetchTriviaQuestions(numQuestions, difficulty);

            if (questions.Count == 0)
            {
                Console.WriteLine("Sorry, couldn't fetch questions. Please try again!");
                return;
            }

            // Run the quiz
            var score = RunQuiz(questions);

            // Show final results
            ShowFinalResults(score, questions.Count);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    // Helper methods
    static int GetNumberOfQuestions()
    {
        while (true)
        {
            Console.Write("How many questions would you like? (5-20): ");
            if (int.TryParse(Console.ReadLine(), out int num) && num >= 5 && num <= 20)
            {
                return num;
            }
            Console.WriteLine("Please enter a number between 5 and 20.");
        }
    }

    static string GetDifficulty()
    {
        Console.WriteLine("\nChoose difficulty:");
        Console.WriteLine("1. Easy");
        Console.WriteLine("2. Medium");
        Console.WriteLine("3. Hard");
        Console.WriteLine("4. Mixed (Random)");

        while (true)
        {
            Console.Write("Enter your choice (1-4): ");
            var choice = Console.ReadLine();

            return choice switch
            {
                "1" => "easy",
                "2" => "medium",
                "3" => "hard",
                "4" => "",
                _ => ""
            };
        }
    }

    static async Task<List<TriviaQuestion>> FetchTriviaQuestions(int amount, string difficulty)
    {
        using var client = new HttpClient();

        var url = $"https://opentdb.com/api.php?amount={amount}&type=multiple";
        if (!string.IsNullOrEmpty(difficulty))
        {
            url += $"&difficulty={difficulty}";
        }

        try
        {
            var response = await client.GetStringAsync(url);
            var triviaResponse = JsonSerializer.Deserialize<TriviaResponse>(response);

            return triviaResponse?.Results ?? new List<TriviaQuestion>();
        }
        catch
        {
            return new List<TriviaQuestion>();
        }
    }

    static int RunQuiz(List<TriviaQuestion> questions)
    {
        int score = 0;

        for (int i = 0; i < questions.Count; i++)
        {
            var question = questions[i];

            Console.WriteLine($"Question {i + 1}/{questions.Count}");
            Console.WriteLine($"Category: {WebUtility.HtmlDecode(question.Category)}");
            Console.WriteLine($"Difficulty: {question.Difficulty.ToUpper()}");
            Console.WriteLine();

            // Decode HTML entities in question and answers
            var decodedQuestion = WebUtility.HtmlDecode(question.Question);
            var correctAnswer = WebUtility.HtmlDecode(question.CorrectAnswer);
            var incorrectAnswers = question.IncorrectAnswers
                .Select(ans => WebUtility.HtmlDecode(ans ?? string.Empty))
                .ToList();

            Console.WriteLine(decodedQuestion);
            Console.WriteLine();

            // Mix up the answers
            var allAnswers = new List<string>(incorrectAnswers) { correctAnswer };
            var random = new Random();
            allAnswers = allAnswers.OrderBy(x => random.Next()).ToList();

            // Display options
            var correctIndex = -1;
            for (int j = 0; j < allAnswers.Count; j++)
            {
                Console.WriteLine($"{j + 1}. {allAnswers[j]}");
                if (allAnswers[j] == correctAnswer)
                {
                    correctIndex = j + 1;
                }
            }

            // Get user answer
            Console.Write("\nYour answer (1-4): ");
            var userInput = Console.ReadLine();

            if (int.TryParse(userInput, out int userAnswer) && userAnswer == correctIndex)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Correct!");
                score++;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Wrong! The correct answer was: {correctAnswer}");
            }

            Console.ResetColor();

            if (i < questions.Count - 1)
            {
                Console.WriteLine("\nPress any key for the next question...");
                Console.ReadKey();
                Console.Clear();
            }
        }

        return score;
    }

    static void ShowFinalResults(int score, int totalQuestions)
    {
        Console.Clear();
        Console.WriteLine("QUIZ COMPLETE!");
        Console.WriteLine("===================");
        Console.WriteLine();

        double percentage = (double)score / totalQuestions * 100;

        Console.WriteLine($"Final Score: {score}/{totalQuestions} ({percentage:F1}%)");
        Console.WriteLine();

        // Fun results based on score
        if (percentage >= 90)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("INCREDIBLE! You're a trivia master!");
        }
        else if (percentage >= 70)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Great job! You know your stuff!");
        }
        else if (percentage >= 50)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Not bad! Room for improvement!");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Time to hit the books!");
        }

        Console.ResetColor();
    }
}