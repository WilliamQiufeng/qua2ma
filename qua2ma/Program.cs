// See https://aka.ms/new-console-template for more information

using System.Globalization;
using System.IO.Compression;
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

Parser.Default.ParseArguments<Options>(args)
    .WithParsed(RunOptions)
    .WithNotParsed(HandleParseError);
return;

DirectoryInfo DecompressToTemp(string qpPath)
{
    var tempDir = Directory.CreateTempSubdirectory();
    ZipFile.ExtractToDirectory(qpPath, tempDir.FullName);
    return tempDir;
}

bool Compress(DirectoryInfo directoryInfo, string outFileName)
{
    if (File.Exists(outFileName))
    {
        Console.Write(resources.FileExists_AskOverwrite, outFileName);
        var answer = Console.ReadLine();
        if (answer?.ToLowerInvariant() == "y")
            File.Delete(outFileName);
        else return false;
    }

    ZipFile.CreateFromDirectory(directoryInfo.FullName, outFileName);
    return true;
}

bool ConvertQua(string quaPath, bool delete = false)
{
    try
    {
        var qua = Qua.Parse(quaPath);
        var converter = new QuaToMalodyConverter(qua);
        converter.Generate();

        var outputPath = Path.ChangeExtension(quaPath, ".mc");
        var resultJson = JsonConvert.SerializeObject(converter.MalodyFile, jsonSettings);
        using var outFile = File.OpenWrite(outputPath);
        outFile.Write(Encoding.UTF8.GetBytes(resultJson));
        outFile.Close();

        if (delete)
            File.Delete(quaPath);

        return true;
    }
    catch (Exception e)
    {
        Console.WriteLine(resources.Fail_Convert_Qua, Path.GetFileName(quaPath), e);
    }

    return false;
}

void ConvertQp(string qpPath, string outputDirectory)
{
    var qpFileName = Path.GetFileName(qpPath);
    try
    {
        var failCount = 0;
        var dir = DecompressToTemp(qpPath);
        foreach (var quaFile in dir.EnumerateFiles("*.qua"))
        {
            var result = ConvertQua(quaFile.FullName, true);
            if (!result) failCount++;
        }

        var mczFileName = Path.GetFileName(Path.ChangeExtension(qpPath, ".mcz"));
        var outMczPath = Path.Combine(outputDirectory, mczFileName);
        if (!Compress(dir, outMczPath))
        {
            Console.WriteLine(resources.Skip_FileExists, qpFileName, outMczPath);
            return;
        }

        if (failCount == 0)
            Console.WriteLine(resources.Success_ConvertQp, qpFileName, outMczPath);
        else
            Console.WriteLine(resources.Success_SomeFailed, qpFileName, outMczPath, failCount);
    }
    catch (Exception e)
    {
        Console.WriteLine(resources.Fail_ConvertQp, qpFileName, e);
    }
}

void RunOptions(Options opts)
{
    if (opts.Language != null)
    {
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo(opts.Language);
    }

    var qpOrDirPath = opts.QpOrDirPath;
    if (qpOrDirPath == null)
    {
        return;
    }
    var parentDir = qpOrDirPath;
    var qps = new List<string>();
    if (File.Exists(qpOrDirPath))
    {
        qps.Add(qpOrDirPath);
        parentDir = Path.GetDirectoryName(qpOrDirPath) ??
                    throw new Exception($"Cannot get directory of '{qpOrDirPath}'");
    }
    else if (Directory.Exists(qpOrDirPath))
    {
        qps.AddRange(Directory.EnumerateFiles(qpOrDirPath, "*.qp", SearchOption.AllDirectories));
    }
    else
    {
        Console.WriteLine(resources.Fail_InvalidPath);
        return;
    }

    var outDir = opts.OutputDirectory ?? parentDir;
    Parallel.ForEach(qps, qpPath => ConvertQp(qpPath, outDir));
}

void HandleParseError(IEnumerable<Error> errs)
{
    //handle errors
    foreach (var error in errs)
    {
        Console.WriteLine(resources.Error, error);
    }
}