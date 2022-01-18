namespace EDiff;

public class Diff
{
    public Operation Operation { get; }
    public string Text { get; }

    public Diff(Operation op, string text)
    {
        Operation = op;
        Text = text;
    }

    public override string ToString()
    {
        return $"Diff({Operation},\"{Text}\")";
    }
}
