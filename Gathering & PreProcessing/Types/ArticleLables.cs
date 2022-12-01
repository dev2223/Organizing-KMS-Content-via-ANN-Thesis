class SentenceLable
{
    public SentenceLable(string sentence, int label)
    {
        Sentence = sentence;
        Label = label;
    }

    public string Sentence { get; set; }
    public int Label { get; set; }
}