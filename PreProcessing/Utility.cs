public class Utility
{
    HttpClient _client = new HttpClient();

    ~Utility()
    {
        _client.Dispose();
    }

    public List<string>? GetParas(string content, ref int priorIdx, bool isFa = true)
    {
        content = GetTargetContent(
            content, "id=\"bodyContent\"", "id=\"catlinks\"", ref priorIdx);
        if (content == "")
        {
            return null;
        }

        priorIdx = 0;
        string paragraph;
        List<string> paras = new();
        while ((paragraph = GetTargetContent(content, "<p>", "\n</p>", ref priorIdx)) != "")
        {
            if (RemoveTagContents(ref paragraph, "<", ">") ||
                RemoveTagContents(ref paragraph, "&#91;", "&#93;"))
            {
                return null;
            }

            paragraph = paragraph.Replace('\n', ' ');
            //var parSentences = paragraph.Split(".");

            // for (int i = 0; i < parSentences.Length; i++)
            // {
            //     var parWords = parSentences[i].Split(" ");
            //     if (parWords.Length >= 10)
            //     {
            //         sentences.Add(parSentences[i]);
            //     }
            // }

            var words = paragraph.Split(' ').ToList();
            if (words.Count <= 25)
            {
                continue;
            }

            var count = 0;
            Func<string, bool> checker = isFa ? FaWordCheck : EnWordCheck;
            for (int j = 0; j < words.Count; j++)
            {
                var word = words[j];
                word = word.ToLower();

                if (String.IsNullOrEmpty(word) ||
                    word.Contains("{") ||
                    checker(word))
                {
                    words.RemoveAt(j--);
                    continue;
                }

                if (word.Length > 3)
                    count++;
            }

            if (count >= 15)
            {
                paragraph = String.Join(' ', words);
                paras.Add(paragraph);
            }

        }

        if (paras.Count >= 15)
        {
            return paras;
        }
        else
        {
            return null;
        }
    }

    public bool EnWordCheck(string word)
    {
        return !((word[0] is (>= 'a' and <= 'z') or '"' or '(' or '\'' or '“'));
    }

    public bool FaWordCheck(string word)
    {
        return !((word[0] is (>= 'آ' and <= 'ی') or '«' or '(' or '"' or '\''));
    }

    public string GetTargetContent(string content, string start, string end, ref int searchStartIdx)
    {
        var behindIndex = content.IndexOf(start, searchStartIdx);
        if (behindIndex == -1) return "";
        var startIndex = behindIndex + start.Length;
        int frontIndex;
        if (end != "")
        {
            frontIndex = content.IndexOf(end, startIndex);
            if (frontIndex == -1) return "";
        }
        else
        {
            frontIndex = content.Length;
        }
        searchStartIdx = frontIndex + end.Length;
        return content.Substring(startIndex, frontIndex - startIndex);
    }

    public bool RemoveTagContents(ref string content, string first, string last)
    {
        int removeIdx;
        while ((removeIdx = content.IndexOf(first)) != -1)
        {
            var endIdx = content.IndexOf(last, removeIdx + 1);
            if (endIdx == -1) return true;

            content = content.Remove(
                removeIdx, endIdx + last.Length - removeIdx);
        }
        return false;
    }

    public async Task<string> GetLinkContent(string link)
    {
        var fail = false;
        var content = "";

        do
        {
            try
            {
                content = await _client!.GetStringAsync(link);
            }
            catch (System.Exception e)
            {
                fail = true;
                Thread.Sleep(1_000);
                System.Console.WriteLine(e.Message);
            }
        } while (fail);

        return content;
    }
}