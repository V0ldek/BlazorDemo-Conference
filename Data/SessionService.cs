using System.Data;
using Dapper;

namespace Conference.Data;

public sealed class SessionService
{
    private readonly IDbConnection _dbConnection;

    public SessionService(IDbConnection dbConnection) =>
        _dbConnection = dbConnection;




    public IReadOnlyList<Session> GetSessions() {

        var lectureDictionary = new Dictionary<int, Lecture>();

        var d_session_chair=_dbConnection.Query<Session, Author, Lecture,Session>(
            @"SELECT s.id AS sessionId, s.when, a.id AS authorId, a.name, a.surname 
		, l.id AS lectureId, l.when, l.paper_id as paperId,
            l.session_id as sessionId,l.speaker_id AS speakerId
                FROM session s
                JOIN author a
                    ON s.chair_id = a.id
                LEFT JOIN lecture l 
                    ON s.id=l.session_id",
            (s, a,l) =>
            {
                s.Chair = a;
                if(l is not null){
                    s.Lectures.Add(l);
                }
                return s;
            },
            splitOn: "authorId,lectureId")
            .ToList();

        


            return d_session_chair;
    }

    public void CreateSession(CreateSession model)
    {
        if (model.ChairId is null)
        {
            throw new ArgumentNullException();
        }

        _dbConnection.Execute(
            $"INSERT INTO session (\"when\", chair_id) VALUES (@When, @ChairId);",
            new { model.When, model.ChairId });
    }
}