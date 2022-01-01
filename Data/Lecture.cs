namespace Conference.Data;

public class Lecture
{
  public int Id { get; init; }

  public DateTime When { get; set; }

  public Author Speaker { get; set; }

  public Author Chair { get; set; }

  public Paper Paper_used { get; set; }

  public Lecture(int lectureId,
                 DateTime when,
                 Author speaker,
                 Paper paper,
                 Author session) =>
      (Id, When, Speaker, Chair, Paper_used) =
          (lectureId, when, speaker, session, paper);
}
