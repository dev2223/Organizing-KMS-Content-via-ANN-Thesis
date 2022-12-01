class Category
{
    public Category(string link)
    {
        Link = link;
    }

    public string? FaTitle { get; set; }
    public string? EnTitle { get; set; }
    public List<Category> Categories { get; set; } = new List<Category>();
    public List<Article> Articles { get; set; } = new List<Article>();
    public string Link { get; set; }
}