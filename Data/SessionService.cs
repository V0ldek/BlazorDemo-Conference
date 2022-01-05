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
        
        var lecturesDictionary = new Dictionary<int,Lecture>();
        var lectures = _dbConnection.Query<Lecture, Paper, Author, Lecture>(
            @"SELECT l.id AS lectureId, l.when, l.session_id as sessionId, p.id AS paperId, p.name, p.classification,a.id as authorId, a.name,a.surname
                FROM lecture l
                JOIN paper p
                    ON l.paper_id = p.id
                JOIN author a
                    ON a.id = l.speaker_Id",
    
            (l, p, a) =>
            {
                l.Speaker = a;
                Paper? paper;
                if (!paperDictionary.TryGetValue(p.Id, out paper))
                {
                    
                }
                l.Paper = paper;
                lecturesDictionary.Add(l.Id, l);
                return l;
            },
            splitOn: "paperId,authorId")
            .ToList();

        var sessionsDictionary = new Dictionary<int, Session>();
        //Query<Session, Author, Lecture, Author, Paper, Author, Session>
        var sessions = _dbConnection.Query<Session, Author, Lecture, Session>(
            @"SELECT s.id AS sessionId, s.when, a.id AS authorId, a.name, a.surname, l.id AS lectureId, l.when,l.session_id as sessionId
                FROM session s
                JOIN author a
                    ON s.chair_id = a.id
                LEFT JOIN lecture l
                    ON s.id = l.session_id",
            (s, a,l ) =>
            {
                s.Chair = a;
                Session? session;
                if (!sessionsDictionary.TryGetValue(s.Id, out session)) 
                {
                    session = s;
                    sessionsDictionary.Add(session.Id,session);
                }
                if (l is not null) 
                {
                    Lecture? lec;
                    if (lecturesDictionary.TryGetValue(l.Id,out lec))
                        session.Lectures.Add(lec);
                }

                return s;
            },
            splitOn: "authorId,lectureId");

            return sessionsDictionary.Values.ToList();
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