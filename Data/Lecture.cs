namespace Conference.Data;

public class Lecture 
{
    public int Id { get; init; }
    
    public DateTime When { get; set; }

    public Author? Speaker { get; set; }

    public Paper? Paper { get; set; }

    public Lecture(int lectureId, DateTime when) => 
        (Id, When) = (lectureId, when);

    public Lecture(int lectureId, DateTime when, Author speaker, Paper paper) => 
        (Id, When, Speaker, Paper) = (lectureId, when, speaker, paper);
}