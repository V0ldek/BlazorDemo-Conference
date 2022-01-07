using System.Data;
using Dapper;

namespace Conference.Data;

public sealed class SessionService
{
    private readonly IDbConnection _dbConnection;

    public SessionService(IDbConnection dbConnection) =>
        _dbConnection = dbConnection;

    public IReadOnlyList<Session> GetSessions() {
        var sessionDictionary = new Dictionary<int, Session>();

        var sessions = _dbConnection.Query<Session, Author, Lecture, Author, Paper, Author, Session>(
            @"SELECT s.id AS sessionId, s.""when"", 
                     ch.id AS authorId, ch.name, ch.surname, 
                     l.id AS lectureId, l.""when"",
                     sp.id AS authorId, sp.name, sp.surname,
                     p.id AS paperId, p.name, p.classification,
                     a.id AS authorId, a.name, a.surname
              FROM ""session"" s
              JOIN author ch
                ON s.chair_id = ch.id
              LEFT JOIN lecture l
                ON l.session_id = s.id
              LEFT JOIN paper p
                ON p.id = l.paper_id
              LEFT JOIN author sp
                ON sp.id = l.speaker_id
              LEFT JOIN paper_author pa
                ON pa.paper_id = p.id
              LEFT JOIN author a
                ON a.id = pa.author_id",
            (s, ch, l, sp, p, a) =>
            {
                Session? session;

                if (!sessionDictionary.TryGetValue(s.Id, out session)) {
                    session = s;
                    sessionDictionary.Add(session.Id, session);
                }

                if (!session.Lectures.Exists(a => a.Id == l.Id)) {
                    p.Authors.Add(a);
                    l.Paper = p;
                    l.Speaker = sp;
                    session.Lectures.Add(l);
                }
                else {
                    Lecture? lecture = session.Lectures.Find(x => x.Id == l.Id);
                    if (lecture is not null && lecture.Paper is not null) {
                        lecture.Paper.Authors.Add(a);
                    }
                }
                session.Chair = ch;

                return session;
            },
            splitOn:"authorId,lectureId,authorId,paperId,authorId"
        );

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