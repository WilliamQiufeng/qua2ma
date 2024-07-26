using CommandLine;
using CommandLine.Text;

namespace qua2ma;

class Options
{
    [Value(0, MetaName = "qp", HelpText = "Path to .qp file")]
    public string? QpOrDirPath { get; set; }

    [Option('h', "help", Required = false, HelpText = "Display help")]
    public bool Help { get; set; }

    [Option('o', "output", Required = false, HelpText = "Output Directory")]
    public string? OutputDirectory { get; set; }

    [Option('l', "language", Required = false, HelpText = "Language/语言 (Supported: zh-hans-cn)")]
    public string? Language { get; set; }

    [Option("ogg", Required = false, Default = false, HelpText = "Convert audio files to ogg")]
    public bool ConvertToOgg { get; set; }

    [Usage(ApplicationAlias = "qua2ma")]
    public static IEnumerable<Example> Examples
    {
        get
        {
            yield return new(resources.Example_ConvertSingleQp, new Options { QpOrDirPath = "path_to_qp.qp" });
            yield return new(resources.Example_ConvertToOgg,
                new Options { QpOrDirPath = "path_to_qp.qp", ConvertToOgg = true });
            yield return new(resources.Example_ConvertToDirectory,
                new Options { QpOrDirPath = "path_to_qp.qp", OutputDirectory = "out/dir/" });
            yield return new(resources.Example_ConvertQpDir, new Options { QpOrDirPath = "path/to/qps/" });
            yield return new(resources.Example_Language,
                new Options { QpOrDirPath = "path/to/qps/", Language = "zh-hans-cn" });
        }
    }
}