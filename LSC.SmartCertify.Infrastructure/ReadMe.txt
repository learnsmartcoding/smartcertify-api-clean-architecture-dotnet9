To check the installed Entity Framework (EF) versions on your machine, you can use the following steps:

Open a command prompt (cmd) or PowerShell window.
Use the following command to list all installed EF packages for all .NET Core/.NET projects on your machine:

dotnet tool list --global | findstr "dotnet-ef"

if you already have some version and wanted to upgrade use this cmd
dotnet tool update --global dotnet-ef --version 9.0.0
or
dotnet tool update --global dotnet-ef 
(this installs latest version)

To install latest EF use 
dotnet tool install --global dotnet-ef

To un install 
dotnet tool uninstall --global dotnet-ef

*********************************

To Scaffold database as model to local project use below cmd.


use this for locally installed SQL Express Dev DB
dotnet ef dbcontext scaffold "Server=localhost\SQLEXPRESS;Initial Catalog=SmartCertify;Integrated Security=SSPI; MultipleActiveResultSets=true;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer -o EntitiesNew -d




using (var connection = new SqlConnection(connectionString))
{
    var command = new SqlCommand("UpdateExamCorrectness", connection)
    {
        CommandType = CommandType.StoredProcedure
    };
    command.Parameters.AddWithValue("@ExamId", examId);
    connection.Open();
    command.ExecuteNonQuery();
}


CREATE PROCEDURE UpdateExamResults
    @ExamId INT
AS
BEGIN
    UPDATE eq
    SET eq.IsCorrect = CASE 
                           WHEN c.ChoiceId = eq.SelectedChoiceId AND c.IsCorrect = 1 THEN 1
                           ELSE 0
                       END
    FROM ExamQuestions eq
    INNER JOIN Choices c
        ON eq.QuestionId = c.QuestionId
    WHERE eq.ExamId = @ExamId;
END;


await _context.Database.ExecuteSqlRawAsync("EXEC UpdateExamResults @ExamId = {0}", examId);
