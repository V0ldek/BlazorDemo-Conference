namespace Conference.Data;

public class Lecture 
{
    public int Id { get; init; }

    public DateTime When { get; set; }

    public Paper? Paper { get; set; }
    public Author? Speaker { get; set; }

    public int SessionId { get; set; }

    public Lecture(int lectureId, DateTime when, int sessionId) => 
        (Id, When, SessionId) = (lectureId, when, sessionId);
}