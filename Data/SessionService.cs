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
        var sessionDictionary = new Dictionary<int, Session>();
        var lecturesDictionary = new Dictionary<int, Lecture>();
        
        var sessions = _dbConnection.Query<Session, Author, Lecture, Author, Paper, Author, Session>(
            @"SELECT  s.id AS sessionId, s.when, 
                a.id AS authorId, a.name, a.surname, 
                l.id as lectureId, l.when, 
                a2.id as authorId, a2.name, a2.surname, 
                p.id as paperId, p.name, p.classification,
                a3.id as authorId, a3.name, a3.surname
                        FROM session s
                        JOIN author a
                            ON s.chair_id = a.id
                        join lecture l 
                            on s.id = l.session_id
                        join author a2 
                            on a2.id = l.speaker_id
                        join paper p 
                            on p.id = l.paper_id
                        join paper_author pa 
                            on p.id = pa.paper_id
                        join author a3 
                            on a3.id = pa.author_id;",
            (s, ch, l, sp, p, a) =>
            {
                Session? session;

                if (!sessionDictionary.TryGetValue(s.Id, out session))
                {
                    l.Speaker = sp;
                    l.Paper = p;

                    if (a is not null) 
                    {
                        l.Paper.Authors.Add(a);
                    }
                    
                    s.Lectures.Add(l);
                    s.Chair = ch;

                    session = s;
                    sessionDictionary.Add(s.Id, session);
                    lecturesDictionary.Add(s.Id, l);
                } 
                else 
                {
                    Lecture? lecture;
                    
                    if (!lecturesDictionary.TryGetValue(s.Id, out lecture))
                    {
                        l.Speaker = sp;
                        l.Paper = p;

                        if (a is not null) 
                        {
                            l.Paper.Authors.Add(a);
                        }
                        
                        session.Lectures.Add(l);
                        lecturesDictionary.Add(s.Id, l);
                    } 
                    else if (a is not null) 
                    {
                        lecture.Paper.Authors.Add(a);
                    }
                }

                
                return session;
            },
            splitOn: "authorId,lectureId,authorId,paperId,authorId");
            return sessions.Distinct().ToList();
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