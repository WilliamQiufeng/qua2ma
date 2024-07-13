using CommandLine;

class Options
{
    [Value(0, MetaName = "qua", HelpText = "Path to .qua file")]
    public string? QuaPath { get; set; }
}