int input = 0;

do
{
    System.Console.WriteLine("1. prepick    2. postpick");
} while (!int.TryParse(Console.ReadLine(), out input));

if (input == 1)
{
    var prePicer = new PrePicker();
    await prePicer.PrePickBuilder();
}
else if (input == 2)
{
    var postPicker = new PostPicker();
    await postPicker.PostPickBuilder();
}


