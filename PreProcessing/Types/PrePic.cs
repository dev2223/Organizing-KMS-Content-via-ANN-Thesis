class PrePic : IPrePic
{
    public PrePic(string title, int sentencesCount, string category1,
        string? category2, string? category3, string link, string enTitle,
        string enCat1, string? enCat2, string? enCat3, string enLink)
    {
        Title = title;
        SentencesCount = sentencesCount;
        Category1 = category1;
        Category2 = category2;
        Category3 = category3;
        Link = link;
        EnLink = enLink;
        EnTitle = enTitle;
        EnCat1 = enCat1;
        EnCat2 = enCat2;
        EnCat3 = enCat3;
    }

    public string Title { get; set; }
    public int SentencesCount { get; set; }
    public string Category1 { get; set; }
    public string? Category2 { get; set; }
    public string? Category3 { get; set; }
    public string Link { get; set; }

    public string EnTitle { get; set; }
    public string EnCat1 { get; set; }
    public string? EnCat2 { get; set; }
    public string? EnCat3 { get; set; }
    public string EnLink { get; set; }
}