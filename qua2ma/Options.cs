using CommandLine;

class Options
{
    [Value(0, MetaName = "qp", HelpText = "Path to .qp file")]
    public string? QpOrDirPath { get; set; }

    [Option('o', "output", Required = false, HelpText = "Output Directory")]
    public string? OutputDirectory { get; set; }

    [Option('l', "language", Required = false, HelpText = "Language/语言 (Supported: zh-hans-cn)")]
    public string? Language { get; set; }
}