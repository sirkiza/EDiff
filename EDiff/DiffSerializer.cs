using System.Runtime.CompilerServices;
using System.Text;
using System.Web;

namespace EDiff
{
    public static class DiffSerializer
    {
        public static string Serialize(IEnumerable<Diff> diffs)
        {
            StringBuilder text = new();

            foreach (var diff in diffs)
            {
                switch (diff.Operation)
                {
                    case Operation.Insert:
                        text.Append('+');
                        break;
                    case Operation.Delete:
                        text.Append('-');
                        break;
                    case Operation.Equal:
                        text.Append(' ');
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                text.Append(HttpUtility.UrlEncode(diff.Text)).Append('\n');
            }

            return text.ToString();
        }

        public static IEnumerable<Diff> Deserialize(string diffs)
        {
            StringReader reader = new(diffs);
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrEmpty(line)) continue;
                var content = HttpUtility.UrlDecode(line[1..]);
                yield return line.First() switch
                {
                    ' ' => new Diff(Operation.Equal, content),
                    '-' => new Diff(Operation.Delete, content),
                    '+' => new Diff(Operation.Insert, content),
                    _ => throw new FormatException("Wrong diff format")
                };
            }
        }
    }
}
