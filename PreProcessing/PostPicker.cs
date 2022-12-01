using System.Globalization;
using CsvHelper;

class PostPicker
{
    Utility _utility = new Utility();

    public async Task PostPickBuilder()
    {
        List<SentenceLable> sentenceLabels;
        //fa
        string[] desiredArticles = GetCurDirFileLines("PostPick.txt");
        sentenceLabels = GetFaSentenceLabels(desiredArticles);
        BuildSentenceLablesCsv(sentenceLabels);

        //en
        var enLinks = GetCurDirFileLines("EnTitles.txt");
        sentenceLabels = await GetEnSentenceLabels(enLinks);
        BuildSentenceLablesCsv(sentenceLabels, "En");
    }

    string[] GetCurDirFileLines(string fileName)
    {
        string[] lines = File.ReadAllLines(
            Path.Combine(Directory.GetCurrentDirectory(), fileName));
        return lines;
    }

    async Task<List<SentenceLable>> GetEnSentenceLabels(string[] enLinks)
    {
        var sentencesCount = new int[enLinks.Length];
        var sentenceLables = new List<SentenceLable>();

        for (int i = 0; i < enLinks.Length; i++)
        {
            var content = await _utility.GetLinkContent(enLinks[i]);
            int priorIdx = 0;

            var paras = _utility.GetParas(content, ref priorIdx);
            if (paras == null)
            {
                System.Console.WriteLine(enLinks[i]);
                continue;
            }

            sentencesCount[i] = paras!.Count;

            for (int j = 0; j < paras.Count; j++)
            {
                sentenceLables.Add(new SentenceLable(paras[j], i + 1));
            }

        }

        return sentenceLables;
    }

    List<SentenceLable> GetFaSentenceLabels(string[] desiredArticles)
    {
        var sentencesPath = Path.Combine(Directory.GetCurrentDirectory(), "Sentences");
        List<SentenceLable> sentenceLables = new();
        var label = 0;
        foreach (var article in desiredArticles)
        {
            try
            {
                var sentences = File.ReadAllLines(Path.Combine(sentencesPath, article + ".txt"));
                ++label;
                for (int i = 0; i < sentences.Length; i++)
                {
                    sentenceLables.Add(new SentenceLable(sentences[i], label));
                }
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine(e.Message);
            }
        }

        return sentenceLables;
    }

    void BuildSentenceLablesCsv(List<SentenceLable> sentenceLables, string name = "Fa")
    {
        using (var writer = new StreamWriter(
            Path.Combine(Directory.GetCurrentDirectory(), $"{name} Dataset.csv")))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteRecords<SentenceLable>(sentenceLables);
        }
    }
}

//read the post pick csv.
//get the sentences.
//give the final file.