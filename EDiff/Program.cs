using EDiff;

const string help = @"
SYNOPSIS
    EDiff diff <path> <path>
    EDiff patch <path_to_file> <path_to_ediff_output>

EDiff supports 2 types of commands:
    diff        Compares 2 files and produce EDiff output;
    patch       Update old file using EDiff output;
";

switch (args.Length)
{
    case < 1:
        Console.WriteLine("EDiff: missing operand after 'diff'");
        Console.WriteLine(help);
        return;
    case < 3:
        Console.WriteLine(args[0] != "--help" ? "Please verify your input.\n\n" : "EDiff tool.\n");
        Console.WriteLine(help);
        return;
}

var command = args[0];
var path1 = args[1];
var path2 = args[2];

if (!File.Exists(path1))
{
    Console.WriteLine($"File {path1} does not exist.");
    return;
}
if (!File.Exists(path2))
{
    Console.WriteLine($"File {path2} does not exist.");
    return;
}

if (command == "diff")
{
    Console.WriteLine(DiffSerializer.Serialize(EasyDiff.GenerateDiff(File.ReadAllText(path1), File.ReadAllText(path2))));
    return;
}
if (command == "patch")
{
    var diffs = DiffSerializer.Deserialize(File.ReadAllText(path2));
    Console.WriteLine(EasyDiff.ApplyDiff(diffs));
    return;
}

Console.WriteLine("Command not supported.\n\n");
Console.WriteLine(help);
