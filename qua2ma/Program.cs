// See https://aka.ms/new-console-template for more information

using System.Text;
using CommandLine;
using Newtonsoft.Json;
using qua2ma;
using Quaver.API.Maps;

var jsonSettings = new JsonSerializerSettings
{
    DefaultValueHandling = DefaultValueHandling.Ignore,
    Formatting = Formatting.None
};

void RunOptions(Options opts)
{
    var inputPath = opts.QuaPath;
    if (inputPath == null) return;
    var qua = Qua.Parse(inputPath);
    var converter = new QuaToMalodyConverter(qua);
    converter.Generate();

    var outputPath = Path.ChangeExtension(inputPath, ".mc");
    var resultJson = JsonConvert.SerializeObject(converter.MalodyFile, jsonSettings);
    using var outFile = File.OpenWrite(outputPath);
    outFile.Write(Encoding.UTF8.GetBytes(resultJson));
    outFile.Close();
}
void HandleParseError(IEnumerable<Error> errs)
{
    //handle errors
    foreach (var error in errs)
    {
        Console.WriteLine($"Error: {error}");
    }
}

Parser.Default.ParseArguments<Options>(args)
    .WithParsed(RunOptions)
    .WithNotParsed(HandleParseError);