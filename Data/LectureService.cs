using System.Data;
using Dapper;

namespace Conference.Data;

public class LectureService
{
    private readonly IDbConnection _dbConnection;

    public LectureService(IDbConnection dbConnection) => _dbConnection = dbConnection;

    public void CreateLecture(CreateLecture model)
    {
        if (model.PaperId is null || model.SpeakerId is null)
        {
            throw new ArgumentNullException(nameof(model));
        }

        _dbConnection.Execute(
            @$"INSERT INTO lecture (""when"", speaker_id, paper_id, session_id) 
               VALUES (@When, @SpeakerId, @PaperId, @SessionId);",
            new { When = model.When, model.SpeakerId, model.PaperId, model.SessionId });
    }
}
