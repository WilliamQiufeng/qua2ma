// See https://aka.ms/new-console-template for more information

using System.Collections.Concurrent;
using System.Globalization;
using System.IO.Compression;
using System.Text;
using CommandLine;
using CommandLine.Text;
using FFMpegCore;
using Newtonsoft.Json;
using qua2ma;
using Quaver.API.Maps;

var jsonSettings = new JsonSerializerSettings
{
    DefaultValueHandling = DefaultValueHandling.Ignore,
    Formatting = Formatting.None
};

var parserResult = Parser.Default.ParseArguments<Options>(args);
parserResult
    .WithParsed(RunOptions)
    .WithNotParsed(HandleParseError);
return;

DirectoryInfo GetTemporaryDirectory()
{
    while (true)
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        if (File.Exists(tempDirectory) || Directory.Exists(tempDirectory))
        {
            continue;
        }

        return Directory.CreateDirectory(tempDirectory);
    }
}

void PrintHelp()
{
    var helpText = HelpText.AutoBuild(parserResult, h => h, e => e);
    Console.WriteLine(helpText.ToString());
}

DirectoryInfo DecompressToTemp(string qpPath)
{
    var tempDir = GetTemporaryDirectory();
    ZipFile.ExtractToDirectory(qpPath, tempDir.FullName);
    return tempDir;
}

void Compress(DirectoryInfo directoryInfo, string outFileName)
{
    ZipFile.CreateFromDirectory(directoryInfo.FullName, outFileName);
}

async Task<bool> ConvertQua(string quaPath, ConcurrentDictionary<string, string> pathReplacements, bool delete = false)
{
    try
    {
        var qua = Qua.Parse(quaPath);
        var converter = new QuaToMalodyConverter(qua, pathReplacements);
        await converter.Generate();

        var outputPath = Path.ChangeExtension(quaPath, ".mc");
        var resultJson = JsonConvert.SerializeObject(converter.MalodyFile, jsonSettings);
        await using var outFile = File.OpenWrite(outputPath);
        var writeAsync = outFile.WriteAsync(Encoding.UTF8.GetBytes(resultJson));

        if (delete)
            File.Delete(quaPath);

        await writeAsync;
        return true;
    }
    catch (Exception e)
    {
        Console.WriteLine(resources.Fail_Convert_Qua, Path.GetFileName(quaPath), e);
    }

    return false;
}

async Task<bool> ConvertToOgg(string path, ConcurrentDictionary<string, string> pathReplacements)
{
    try
    {
        var oggPath = Path.ChangeExtension(path, ".ogg");
        await FFMpegArguments.FromFileInput(path)
            .OutputToFile(oggPath)
            .ProcessAsynchronously();
        pathReplacements.TryAdd(path, oggPath);
        File.Delete(path);
        Console.WriteLine(resources.Success_ConvertOgg, path);
        return true;
    }
    catch (Exception e)
    {
        Console.WriteLine(resources.Fail_ConvertOgg, path, e.Message);
        return false;
    }
}

IEnumerable<Task> ConvertIncompatibleFiles(DirectoryInfo dir, bool convertToOgg,
    ConcurrentDictionary<string, string> pathReplacements)
{
    foreach (var fileInfo in dir.EnumerateFiles())
    {
        switch (fileInfo.Extension)
        {
            case ".mp3":
            case ".wav":
                if (convertToOgg)
                {
                    yield return ConvertToOgg(fileInfo.FullName, pathReplacements);
                }

                break;
        }
    }
}

async Task ConvertQp(string qpPath, string outputDirectory, bool convertToOgg)
{
    var qpFileName = Path.GetFileName(qpPath);
    var mczFileName = Path.GetFileName(Path.ChangeExtension(qpPath, ".mcz"));
    var outMczPath = Path.Combine(outputDirectory, mczFileName);
    if (File.Exists(outMczPath))
    {
        Console.Write(resources.FileExists_AskOverwrite, outMczPath);
        var answer = Console.ReadLine();
        if (answer?.ToLowerInvariant() == "y")
            File.Delete(outMczPath);
        else
        {
            Console.WriteLine(resources.Skip_FileExists, qpFileName, outMczPath);
            return;
        }
    }

    try
    {
        var failCount = 0;
        var dir = DecompressToTemp(qpPath);
        var pathReplacements = new ConcurrentDictionary<string, string>();

        await Task.WhenAll(ConvertIncompatibleFiles(dir, convertToOgg, pathReplacements).ToArray());
        await Task.WhenAll(dir.EnumerateFiles("*.qua").Select(async quaFile =>
        {
            var result = await ConvertQua(quaFile.FullName, pathReplacements, true);
            if (!result) failCount++;
        }).ToArray());

        Compress(dir, outMczPath);

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

    if (opts.Help)
    {
        PrintHelp();
        return;
    }

    var qpOrDirPath = opts.QpOrDirPath;
    if (string.IsNullOrWhiteSpace(qpOrDirPath))
    {
        Console.WriteLine(resources.Path_NoneGiven);
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
        qps.AddRange(Directory.EnumerateFiles(qpOrDirPath, "*.qp", SearchOption.TopDirectoryOnly));
    }
    else
    {
        Console.WriteLine(resources.Fail_InvalidPath);
        return;
    }

    var outDir = opts.OutputDirectory ?? parentDir;
    Task.WaitAll(qps.Select(qpPath => ConvertQp(qpPath, outDir, opts.ConvertToOgg)).ToArray());
}

void HandleParseError(IEnumerable<Error> errs)
{
    //handle errors
    foreach (var error in errs)
    {
        Console.WriteLine(resources.Error, error);
    }
}