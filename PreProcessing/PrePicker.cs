using System.Globalization;
using CsvHelper;

public class PrePicker
{
    List<Category> _cats = new List<Category>();
    HashSet<string> _titles = new HashSet<string>();
    Utility _utility = new Utility();
    List<PrePic>? _prePics;

    public async Task PrePickBuilder()
    {
        AnalyzeTxt();
        await AnalyzeCats();
        await AnalyzeArticles(_cats);

        _prePics = new List<PrePic>();
        BuildCsvList(_cats, new Category[0]);

        //create the csv file
        using (var writer = new StreamWriter(
            Path.Combine(Directory.GetCurrentDirectory(), "PrePick.csv")))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteRecords<PrePic>(_prePics);
        }
    }

    void AnalyzeTxt()
    {
        //read the content
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Input.txt");
        var contents = File.ReadAllLines(path);
        var map = new List<Category>(4);

        //Analyze the Content
        var length = contents.Length;
        for (int i = 0; i < length; i++)
        {
            var content = contents[i];

            if (int.TryParse(content[0].ToString(), out int level))
            {
                var link = content.Substring(1, content.Length - 1);
                var cat = new Category(link);

                if (level == 1)
                {
                    _cats.Add(cat);
                    map = new List<Category>(4);
                    map.Add(cat);
                }
                else
                {
                    map[level - 2].Categories.Add(cat);

                    if (map.Count >= level)
                    {
                        map[level - 1] = cat;
                        if (map.Count > level)
                        {
                            map.RemoveRange(level, map.Count - level - 1);
                        }
                    }
                    else
                    {
                        map.Add(cat);
                    }
                }
            }
            else
            {
                map[^1].Categories.Add(
                    new Category(content)
                );
            }
        }
    }

    async Task AnalyzeCats()
    {
        //provide this with en
        await AnalyzeCats(_cats, true);

        async Task AnalyzeCats(List<Category> cats, bool is1stLevel = false)
        {
            var length = cats.Count;
            for (int i = 0; i < length; i++)
            {
                var cat = cats[i];

                if (cat.Categories.Count > 0)
                {
                    if (is1stLevel) await AnalyzeCatLink(cat);
                    else
                    {
                        int priorIdx = 0;
                        var content = await _utility.GetLinkContent(cat.Link);
                        SetTitles(content, ref priorIdx, cat);
                    }

                    await AnalyzeCats(cat.Categories);
                }
                else
                {
                    await AnalyzeCatLink(cat);
                }
            }
        }

        async Task AnalyzeCatLink(Category cat)
        {
            var content = await _utility.GetLinkContent(cat.Link);
            var priorIdx = 0;

            //getting title
            SetTitles(content, ref priorIdx, cat);

            //getting links
            content = _utility.GetTargetContent(
                content, "id=\"mw-pages\"", "id=\"catlinks\"", ref priorIdx);
            priorIdx = 0;
            string subCatLink;
            while ((
                subCatLink = _utility.GetTargetContent(content, "<li><a href=\"/wiki/", "\" ",
                ref priorIdx)) != "")
            {
                cat.Articles.Add(new Article("https://fa.wikipedia.org/wiki/" + subCatLink));
            }
        }

        void SetTitles(string content, ref int priorIdx, Category cat)
        {
            cat.FaTitle = _utility.GetTargetContent(
                content, "<title>رده:", " - ویکی‌پدیا", ref priorIdx);
            var enTitle = _utility.GetTargetContent(
                content, "https://en.wikipedia.org/wiki/Category:", "\"", ref priorIdx);
            cat.EnTitle = enTitle.Replace("_", " ");
        }
    }

    async Task AnalyzeArticles(List<Category> cats)
    {
        var length = cats.Count;
        for (int i = 0; i < length; i++)
        {
            var cat = cats[i];

            if (cat.Articles.Count > 0)
            {
                for (int j = 0; j < cat.Articles.Count; j++)
                {
                    if (await AnalyzeArticleLinks(cat.Articles[j], cat))
                    {
                        cat.Articles.RemoveAt(j--);
                    }
                }
            }

            if (cat.Categories.Count > 0)
            {
                await AnalyzeArticles(cat.Categories);
            }
        }

        //returns whether delete the article or not
        async Task<bool> AnalyzeArticleLinks(Article article, Category cat)
        {
            var link = article.FaLink;
            var content = await _utility.GetLinkContent(link);
            var priorIdx = 0;

            //getting fa title
            var title = _utility.GetTargetContent(content, "<title>", " - ویکی‌پدیا", ref priorIdx);
            if (_titles.Contains(title))
            {
                return true;
            }

            _titles.Add(title);
            article.FaTitle = title;

            //getting en link
            link = _utility.GetTargetContent(
                content, "interwiki-en mw-list-item\"><a href=\"", "\"", ref priorIdx);
            article.EnLink = link;
            if (link == "") return true;

            //getting en title
            int discard = 0;
            title = _utility.GetTargetContent(link, "/wiki/", "", ref discard);
            article.EnTitle = title.Replace("_", " ");

            //getting sentence
            List<string>? paras = _utility.GetParas(content, ref priorIdx);
            if (paras == null)
            {
                return true;
            }
            else
            {
                article.SentencesCount = paras.Count;
                File.WriteAllLines(Path.Combine(
                    Directory.GetCurrentDirectory(), "Sentences", $"{article.FaTitle}.txt"),
                    paras);
            }

            return false;
        }
    }

    void BuildCsvList(List<Category> cats, Category[] upperMap)
    {
        var length = cats.Count;
        var map = new Category[upperMap.Length + 1];
        upperMap.CopyTo(map, 0);

        for (int i = 0; i < length; i++)
        {
            var cat = cats[i];
            map[upperMap.Length] = cat;

            if (cat.Articles.Count > 0)
            {
                var articleLength = cat.Articles.Count;
                for (int j = 0; j < articleLength; j++)
                {
                    var article = cat.Articles[j];
                    string? cat2 = null, cat3 = null, enCat2 = null, enCat3 = null;
                    if (map.Length > 1)
                    {
                        cat2 = map[1].FaTitle!;
                        enCat2 = map[1].EnTitle!;

                        if (map.Length > 2)
                        {
                            cat3 = map[2].FaTitle!;
                            enCat3 = map[2].EnTitle!;
                        }
                    }

                    var prePic = new PrePic(article.FaTitle!, article.SentencesCount, map[0].FaTitle!,
                        cat2, cat3, article.FaLink, article.EnTitle!, map[0].EnTitle!, enCat2, enCat3,
                        article.EnLink!);
                    _prePics!.Add(prePic);
                }
            }

            if (cat.Categories.Count > 0)
            {
                BuildCsvList(cat.Categories, map);
            }
        }
    }



    //analyze the txt & get all the info based on the type.
    //read the links and get all the page links inside them.
    //open the links, get the sentences, and store them if they are in adequate amount.
    //give a csv and let me pick after making sure they have desired amount of sentences.
    //get the sentences of the picked articles.
}