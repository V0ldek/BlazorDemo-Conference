namespace Conference.Data;

public class Paper
{
    public int Id { get; init; }

    public string Name { get; init; }

    public string Classification { get; init; }

    public string AuthorNames => string.Join(", ", Authors.Select(a => a.DisplayName));

    public HashSet<Author> Authors { get; init; } = new HashSet<Author>();

    public Paper(int paperId, string name, string classification) =>
        (Id, Name, Classification) = (paperId, name, classification);
}