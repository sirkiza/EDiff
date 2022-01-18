using System.Text;

namespace EDiff;

public static class EasyDiff
{
    public static List<Diff> GenerateDiff(ReadOnlySpan<char> text1, ReadOnlySpan<char> text2)
    {
        List<Diff> diffs = new();

        if (text1 == text2)
        {
            if (text1.Length != 0)
            {
                diffs.Add(new Diff(Operation.Equal, text1.ToString()));
            }

            return diffs;
        }

        var commonPrefixLength = ComputeCommonPrefix(text1, text2);
        var commonPrefix = text1[..commonPrefixLength];
        text1 = text1[commonPrefixLength..];
        text2 = text2[commonPrefixLength..];

        var commonSuffixLength = ComputeCommonSuffix(text1, text2);
        var commonSuffix = text1[^commonSuffixLength..];
        text1 = text1[..^commonSuffixLength];
        text2 = text2[..^commonSuffixLength];

        diffs = ComputeDiff(text1, text2);

        if (commonPrefixLength > 0)
        {
            diffs.Insert(0, new Diff(Operation.Equal, commonPrefix.ToString()));
        }
        if (commonSuffixLength > 0)
        {
            diffs.Add(new Diff(Operation.Equal, commonSuffix.ToString()));
        }

        return diffs;
    }

    public static string ApplyDiff(IEnumerable<Diff> diffs)
    {
        StringBuilder result = new();
        foreach (var diff in diffs)
        {
            switch (diff.Operation)
            {
                case Operation.Insert:
                case Operation.Equal:
                    result.Append(diff.Text);
                    break;
                case Operation.Delete:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return result.ToString();
    }

    private static int ComputeCommonPrefix(ReadOnlySpan<char> text1, ReadOnlySpan<char> text2)
    {
        var min = Math.Min(text1.Length, text2.Length);
        for (var i = 0; i < min; i++)
        {
            if (text1[i] != text2[i])
            {
                return i;
            }
        }

        return min;
    }
    private static int ComputeCommonSuffix(ReadOnlySpan<char> text1, ReadOnlySpan<char> text2)
    {
        var min = Math.Min(text1.Length, text2.Length);
        for (var i = 1; i <= min; i++)
        {
            if (text1[^i] != text2[^i])
            {
                return i - 1;
            }
        }

        return min;
    }

    private static List<Diff> ComputeDiff(ReadOnlySpan<char> text1, ReadOnlySpan<char> text2)
    {
        List<Diff> diffs = new();

        if (text1.Length == 0)
        {
            diffs.Add(new Diff(Operation.Insert, text2.ToString()));
            return diffs;
        }

        if (text2.Length == 0)
        {
            diffs.Add(new Diff(Operation.Delete, text1.ToString()));
            return diffs;
        }

        var longText = text1.Length > text2.Length ? text1 : text2;
        var shortText = text1.Length > text2.Length ? text2 : text1;
        var i = longText.IndexOf(shortText);

        if (i != -1)
        {
            var op = (text1.Length > text2.Length) ?
                Operation.Delete : Operation.Insert;
            diffs.Add(new Diff(op, longText[..i].ToString()));
            diffs.Add(new Diff(Operation.Equal, shortText.ToString()));
            diffs.Add(new Diff(op, longText[(i + shortText.Length)..].ToString()));
            return diffs;
        }

        if (shortText.Length != 1) return ComputeBisectDiff(text1, text2);
        diffs.Add(new Diff(Operation.Delete, text1.ToString()));
        diffs.Add(new Diff(Operation.Insert, text2.ToString()));
        return diffs;

    }

    /// <remarks>http://btn1x4.inf.uni-bayreuth.de/publications/dotor_buchmann/SCM/ChefRepo/DiffUndMerge/DAlgorithmVariations.pdf</remarks>
    private static List<Diff> ComputeBisectDiff(ReadOnlySpan<char> text1, ReadOnlySpan<char> text2)
    {
        var maxEdit = (text1.Length + text2.Length + 1) / 2;
        var rowIndLength = 2 * maxEdit;
        var v1 = new int[rowIndLength];
        var v2 = new int[rowIndLength];
        for (var i = 0; i < rowIndLength; i++)
        {
            v1[i] = -1;
            v2[i] = -1;
        }

        v1[maxEdit + 1] = 0;
        v2[maxEdit + 1] = 0;
        var delta = text1.Length - text2.Length;
        var front = delta % 2 != 0;
        var k1Start = 0;
        var k1End = 0;
        var k2Start = 0;
        var k2End = 0;

        for (var d = 0; d < maxEdit; d++)
        {
            for (var k1 = -d + k1Start; k1 <= d - k1End; k1 += 2)
            {
                var k1Offset = maxEdit + k1;
                int x1;
                if (k1 == -d || k1 != d && v1[k1Offset - 1] < v1[k1Offset + 1])
                {
                    x1 = v1[k1Offset + 1];
                }
                else
                {
                    x1 = v1[k1Offset - 1] + 1;
                }

                var y1 = x1 - k1;
                while (x1 < text1.Length && y1 < text2.Length
                                         && text1[x1] == text2[y1])
                {
                    x1++;
                    y1++;
                }

                v1[k1Offset] = x1;
                if (x1 <= text1.Length)
                {
                    if (y1 <= text2.Length)
                    {
                        if (!front) continue;
                        var k2Offset = maxEdit + delta - k1;
                        if (k2Offset < 0 || k2Offset >= rowIndLength || v2[k2Offset] == -1) continue;
                        var x2 = text1.Length - v2[k2Offset];
                        if (x1 >= x2)
                        {
                            return ComputeBisectSplitDiff(text1, text2, x1, y1);
                        }
                    }
                    else
                    {
                        k1Start += 2;
                    }
                }
                else
                {
                    k1End += 2;
                }
            }

            for (var k2 = -d + k2Start; k2 <= d - k2End; k2 += 2)
            {
                var k2Offset = maxEdit + k2;
                int x2;
                if (k2 == -d || k2 != d && v2[k2Offset - 1] < v2[k2Offset + 1])
                {
                    x2 = v2[k2Offset + 1];
                }
                else
                {
                    x2 = v2[k2Offset - 1] + 1;
                }
                var y2 = x2 - k2;
                while (x2 < text1.Length && y2 < text2.Length 
                       && text1[^(x2 + 1)] == text2[^(y2 + 1)])
                {
                    x2++;
                    y2++;
                }
                v2[k2Offset] = x2;

                if (x2 <= text1.Length)
                {
                    if (y2 <= text2.Length)
                    {
                        if (front) continue;
                        var k1Offset = maxEdit + delta - k2;
                        if (k1Offset < 0 || k1Offset >= rowIndLength || v1[k1Offset] == -1) continue;
                        var x1 = v1[k1Offset];
                        var y1 = maxEdit + x1 - k1Offset;
                        x2 = text1.Length - v2[k2Offset];
                        if (x1 >= x2)
                        {
                            return ComputeBisectSplitDiff(text1, text2, x1, y1);
                        }
                    }
                    else
                    {
                        k2Start += 2;
                    }
                }
                else
                {
                    k2End += 2;
                }
            }
        }

        return new List<Diff>
        {
            new Diff(Operation.Delete, text1.ToString()),
            new Diff(Operation.Insert, text2.ToString())
        };
    }

    private static List<Diff> ComputeBisectSplitDiff(ReadOnlySpan<char> text1, ReadOnlySpan<char> text2, int x, int y)
    {
        var text1A = text1[..x];
        var text2A = text2[..y];
        var text1B = text1[x..];
        var text2B = text2[y..];

        var diffs = GenerateDiff(text1A, text2A);
        var diffSb = GenerateDiff(text1B, text2B);

        diffs.AddRange(diffSb);
        return diffs;
    }
}
