namespace Conference.Data;

public class Lecture 
{
    public int Id { get; init; }

    public DateTime When { get; set; }

    public int PaperId {get; set;}

    public int SessionId {get; set;}

    public int SpeakerId {get; set;}

    public Lecture(int lectureId, DateTime when, int paperId, int sessionId,int speakerId) => 
        (Id, When,PaperId,SessionId,SpeakerId) = (lectureId, when,paperId,sessionId,speakerId);
}