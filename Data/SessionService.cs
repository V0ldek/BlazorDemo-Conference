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
        var sessions = new Dictionary<int, Session>();
        var authors = new Dictionary<int, Author>();
        var lectures = new Dictionary<int, Lecture>();
        var papers = new Dictionary<int, Paper>();

        var q = _dbConnection.Query<Session, Author, Lecture, Author, Paper, Author, Session>(
            @"SELECT 
                  session.id AS sessionId, session.when,
                  chair.id AS authorId, chair.name, chair.surname,
                  lecture.id AS lectureId, lecture.when,
                  speaker.id AS authorId, speaker.name, speaker.surname,
                  paper.id AS paperId, paper.name, paper.classification,
                  author.id AS authorId, author.name, author.surname
                FROM session
                JOIN author chair
                    ON session.chair_id = chair.id
                LEFT JOIN lecture
                    ON lecture.session_id = session.id
                LEFT JOIN author speaker
                    ON lecture.speaker_id = speaker.id
                LEFT JOIN paper
                    ON lecture.paper_id = paper.id
                LEFT JOIN paper_author pa
                    ON pa.paper_id = paper.id
                LEFT JOIN author
                    ON pa.author_id = author.id
                ",
            (session, chair, lecture, speaker, paper, author) =>
            {
                var sessionEntity = GetOrAdd(sessions, session, s => s.Id);
                var chairEntity = GetOrAdd(authors, chair, c => c.Id);
                var lectureEntity = GetOrAdd(lectures, lecture, l => l.Id);
                var speakerEntity = GetOrAdd(authors, speaker, s => s.Id);
                var paperEntity = GetOrAdd(papers, paper, p => p.Id);
                var authorEntity = GetOrAdd(authors, author, a => a.Id);

                sessionEntity!.Chair = chairEntity;

                if (lectureEntity is not null)
                {
                    lectureEntity.Speaker = speakerEntity;
                    lectureEntity.Paper = paperEntity;
                    sessionEntity.Lectures.Add(lectureEntity);
                }

                if (paperEntity is not null && authorEntity is not null)
                {
                    paperEntity.Authors.Add(authorEntity);
                }

                return sessionEntity;
            },
            splitOn: "authorId,lectureId,authorId,paperId,authorId");

        return q.Distinct().ToList();
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

    private TEntity? GetOrAdd<TEntity>(
        Dictionary<int, TEntity> dictionary,
        TEntity? record,
        Func<TEntity, int> keySelector) where TEntity : class
    {
        if (record is null)
        {
            return null;
        }

        var key = keySelector(record);
        if (!dictionary.TryGetValue(key, out var entity))
        {
            dictionary.Add(key, record);
            entity = record;
        }

        return entity;
    }
}