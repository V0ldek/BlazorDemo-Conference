using System.Data;
using Dapper;

namespace Conference.Data;

public sealed class SessionService
{
    private readonly IDbConnection _dbConnection;

    public SessionService(IDbConnection dbConnection) =>
        _dbConnection = dbConnection;

    public IReadOnlyList<Session> GetSessions()
    {
        List<Session> lst = _dbConnection.Query<Session, Author, Session>(
            @"SELECT s.id AS sessionId, s.when, a.id AS authorId, a.name, a.surname 
                FROM session s
                JOIN author a
                    ON s.chair_id = a.id",
            (s, a) =>
            {
                s.Chair = a;
                return s;
            },
            splitOn: "authorId")
            .ToList();

        var paperDictionary = new Dictionary<int, Paper>();

        var papers = _dbConnection.Query<Paper, Author, Paper>(
            @"SELECT p.id AS paperId, p.name, p.classification, a.id AS authorId, a.name, a.surname 
                FROM paper p
                LEFT JOIN paper_author pa
                    ON p.id = pa.paper_id
                LEFT JOIN author a
                    ON pa.author_id = a.id",
            (p, a) =>
            {
                Paper? paper;

                if (!paperDictionary.TryGetValue(p.Id, out paper))
                {
                    paper = p;
                    paperDictionary.Add(paper.Id, paper);
                }

                if (a is not null)
                {
                    paper.Authors.Add(a);
                }

                return paper;
            },
            splitOn: "authorId");

        foreach (Session session in lst)
        {
            session.Lectures = _dbConnection.Query<Lecture, Author, Paper, Lecture>(
                @"SELECT l.id AS lectureId, l.when, s.id AS authorId, s.name, s.surname, p.id AS paperId, p.name, p.classification
                    FROM lecture l
                    JOIN author s
                        ON l.speaker_id = s.id
                    JOIN paper p
                        ON l.paper_id = p.id
                    WHERE l.session_id = " + session.Id.ToString(),
                    (l, sp, p) =>
                    {
                        l.Speaker = sp;
                        Paper? paper;
                        paperDictionary.TryGetValue(p.Id, out paper);
                        l.Paper_used = paper;
                        l.Session = session;
                        return l;
                    },
            splitOn: "authorId, paperId"
            ).ToList();
        }

        return lst;
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