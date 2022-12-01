public class Article
{
    public Article(string link)
    {
        FaLink = link;
    }

    public string? FaTitle { get; set; }
    public string? EnTitle { get; set; }
    public string FaLink { get; set; }
    public string? EnLink { get; set; }
    public int SentencesCount { get; set; }
}