using System.Data;
using Dapper;

namespace Conference.Data;

public class LectureService
{
    private readonly IDbConnection _dbConnection;

    public LectureService(IDbConnection dbConnection) => _dbConnection = dbConnection;
    public IReadOnlyList<Lecture> GetLectures() =>
        _dbConnection.Query<Lecture, Paper, Author, Lecture>(
            @"SELECT l.id AS lectureId, l.when, l.speaker_id as speakerId, l.session_id, p.id AS paperId, p.name, p.classification,a.id as authorId, a.name,a.surname
                FROM lecture l
                JOIN paper p
                    ON l.paper_id = p.id
                JOIN author a
                    ON a.id = l.speakerId",
    
            (l, p, a) =>
            {
                l.Speaker = a;
                l.Paper = p;
                return l;
            },
            splitOn: "paperId,authorId")
            .ToList();
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
