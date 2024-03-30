using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;
using JetBrains.Profiler.SelfApi;
using RunMode = BenchmarkDotNet.Diagnosers.RunMode;

#nullable enable

namespace Mediator.Benchmarks;

[AttributeUsage(AttributeTargets.Class)]
internal sealed class DotMemoryDiagnoserAttribute : Attribute, IConfigSource
{
    public IConfig Config { get; }

    public DotMemoryDiagnoserAttribute()
    {
        Config = ManualConfig.CreateEmpty().AddDiagnoser(new DotMemoryDiagnoser());
    }

    public DotMemoryDiagnoserAttribute(Uri? nugetUrl = null, string? toolsDownloadFolder = null)
    {
        Config = ManualConfig.CreateEmpty().AddDiagnoser(new DotMemoryDiagnoser(nugetUrl, toolsDownloadFolder));
    }
}

internal sealed class DotMemoryDiagnoser : IDiagnoser
{
    private readonly Uri? nugetUrl;
    private readonly string? toolsDownloadFolder;

    public DotMemoryDiagnoser(Uri? nugetUrl = null, string? toolsDownloadFolder = null)
    {
        this.nugetUrl = nugetUrl;
        this.toolsDownloadFolder = toolsDownloadFolder;
    }

    public IEnumerable<string> Ids => new[] { "DotMemory" };
    public string ShortName => "DotMemory";

    public RunMode GetRunMode(BenchmarkCase benchmarkCase)
    {
        // return IsSupported(benchmarkCase.Job.Environment.Runtime.RuntimeMoniker) ? RunMode.ExtraRun : RunMode.None;
        return RunMode.NoOverhead;
    }

    private readonly List<string> snapshotFilePaths = new();

    public void Handle(HostSignal signal, DiagnoserActionParameters parameters)
    {
        var job = parameters.BenchmarkCase.Job;
        // bool isInProcess = job.Infrastructure.Toolchain.IsInProcess;
        var isInProcess = false;
        var logger = parameters.Config.GetCompositeLogger();
        if (isInProcess)
            throw new Exception("Inprocess not supported");
        DotMemoryToolBase tool = new ExternalDotMemoryTool(logger, nugetUrl, downloadTo: toolsDownloadFolder);

        // var runtimeMoniker = job.Environment.Runtime.RuntimeMoniker;
        // if (!IsSupported(runtimeMoniker))
        // {
        //     logger.WriteLineError($"Runtime '{runtimeMoniker}' is not supported by DotMemory");
        //     return;
        // }

        switch (signal)
        {
            case HostSignal.BeforeAnythingElse:
                tool.Init(parameters);
                break;
            case HostSignal.BeforeActualRun:
                snapshotFilePaths.Add(tool.Start(parameters));
                break;
            case HostSignal.AfterActualRun:
                tool.Stop(parameters);
                break;
        }
    }

    public IEnumerable<IExporter> Exporters => Enumerable.Empty<IExporter>();
    public IEnumerable<IAnalyser> Analysers => Enumerable.Empty<IAnalyser>();

    public IEnumerable<ValidationError> Validate(ValidationParameters validationParameters)
    {
        // var runtimeMonikers = validationParameters
        //     .Benchmarks.Select(b => b.Job.Environment.Runtime.RuntimeMoniker)
        //     .Distinct();
        // foreach (var runtimeMoniker in runtimeMonikers)
        // {
        //     if (!IsSupported(runtimeMoniker))
        //         yield return new ValidationError(true, $"Runtime '{runtimeMoniker}' is not supported by DotMemory");
        // }
        return Enumerable.Empty<ValidationError>();
    }

    internal static bool IsSupported(RuntimeMoniker runtimeMoniker)
    {
        switch (runtimeMoniker)
        {
            case RuntimeMoniker.HostProcess:
            case RuntimeMoniker.Net80:
            case RuntimeMoniker.Net90:
                return true;
            default:
                return false;
        }
    }

    public IEnumerable<Metric> ProcessResults(DiagnoserResults results) => ImmutableArray<Metric>.Empty;

    public void DisplayResults(ILogger logger)
    {
        if (snapshotFilePaths.Any())
        {
            logger.WriteLineInfo("The following DotMemory snapshots were generated:");
            foreach (string snapshotFilePath in snapshotFilePaths)
                logger.WriteLineInfo($"* {snapshotFilePath}");
        }
    }
}

internal abstract class DotMemoryToolBase
{
    private readonly ILogger logger;
    private readonly Uri? nugetUrl;
    private readonly NuGetApi nugetApi;
    private readonly string? downloadTo;

    protected DotMemoryToolBase(
        ILogger logger,
        Uri? nugetUrl = null,
        NuGetApi nugetApi = NuGetApi.V3,
        string? downloadTo = null
    )
    {
        this.logger = logger;
        this.nugetUrl = nugetUrl;
        this.nugetApi = nugetApi;
        this.downloadTo = downloadTo;
    }

    public void Init(DiagnoserActionParameters parameters)
    {
        try
        {
            logger.WriteLineInfo("Ensuring that DotMemory prerequisite is installed...");
            var progress = new Progress(logger, "Installing DotMemory");
            DotMemory.EnsurePrerequisiteAsync(progress, nugetUrl, nugetApi, downloadTo).Wait();
            logger.WriteLineInfo("DotMemory prerequisite is installed");
            logger.WriteLineInfo($"DotMemory runner path: {GetRunnerPath()}");
        }
        catch (Exception e)
        {
            logger.WriteLineError(e.ToString());
        }
    }

    protected abstract bool AttachOnly { get; }
    protected abstract void Attach(DiagnoserActionParameters parameters, string snapshotFile);
    protected abstract void StartCollectingData();
    protected abstract void SaveData();
    protected abstract void Detach();

    public string Start(DiagnoserActionParameters parameters)
    {
        string snapshotFile = GetFilePath(parameters, "snapshots", DateTime.Now, "dmw", ".0000".Length);
        string? snapshotDirectory = Path.GetDirectoryName(snapshotFile);
        logger.WriteLineInfo($"Target snapshot file: {snapshotFile}");
        if (!Directory.Exists(snapshotDirectory) && snapshotDirectory != null)
        {
            try
            {
                Directory.CreateDirectory(snapshotDirectory);
            }
            catch (Exception e)
            {
                logger.WriteLineError($"Failed to create directory: {snapshotDirectory}");
                logger.WriteLineError(e.ToString());
            }
        }

        try
        {
            logger.WriteLineInfo("Attaching DotMemory to the process...");
            Attach(parameters, snapshotFile);
            logger.WriteLineInfo("DotMemory is successfully attached");
        }
        catch (Exception e)
        {
            logger.WriteLineError(e.ToString());
            return snapshotFile;
        }

        if (!AttachOnly)
        {
            try
            {
                logger.WriteLineInfo("Start collecting data using dataTrace...");
                StartCollectingData();
                logger.WriteLineInfo("Data collecting is successfully started");
            }
            catch (Exception e)
            {
                logger.WriteLineError(e.ToString());
            }
        }

        return snapshotFile;
    }

    public void Stop(DiagnoserActionParameters parameters)
    {
        if (!AttachOnly)
        {
            try
            {
                logger.WriteLineInfo("Saving DotMemory snapshot...");
                SaveData();
                logger.WriteLineInfo("DotMemory snapshot is successfully saved to the artifact folder");
            }
            catch (Exception e)
            {
                logger.WriteLineError(e.ToString());
            }

            try
            {
                logger.WriteLineInfo("Detaching DotMemory from the process...");
                Detach();
                logger.WriteLineInfo("DotMemory is successfully detached");
            }
            catch (Exception e)
            {
                logger.WriteLineError(e.ToString());
            }
        }
    }

    protected string GetRunnerPath()
    {
        var consoleRunnerPackageField = typeof(DotMemory).GetField(
            "ConsoleRunnerPackage",
            BindingFlags.NonPublic | BindingFlags.Static
        );
        if (consoleRunnerPackageField == null)
            throw new InvalidOperationException("Field 'ConsoleRunnerPackage' not found.");

        object? consoleRunnerPackage = consoleRunnerPackageField.GetValue(null);
        if (consoleRunnerPackage == null)
            throw new InvalidOperationException("Unable to get value of 'ConsoleRunnerPackage'.");

        var consoleRunnerPackageType = consoleRunnerPackage.GetType();
        var getRunnerPathMethod = consoleRunnerPackageType.GetMethod("GetRunnerPath");
        if (getRunnerPathMethod == null)
            throw new InvalidOperationException("Method 'GetRunnerPath' not found.");

        string? runnerPath = getRunnerPathMethod.Invoke(consoleRunnerPackage, null) as string;
        if (runnerPath == null)
            throw new InvalidOperationException("Unable to invoke 'GetRunnerPath'.");

        return runnerPath;
    }

    internal static string GetFilePath(
        DiagnoserActionParameters details,
        string? subfolder,
        DateTime? creationTime,
        string fileExtension,
        int reserve
    )
    {
        string fileName = details.BenchmarkCase.Descriptor.WorkloadMethod.Name;
        if (creationTime.HasValue)
            fileName += "_" + creationTime.Value.ToString("yyyy-MM-dd_HH-mm-ss-fff");

        string filePath = Path.Combine(
            "BenchmarkDotNet.Artifacts",
            subfolder ?? string.Empty,
            fileName + "." + fileExtension
        );
        if (reserve > 0)
            filePath = filePath.Insert(filePath.Length - reserve, ".");

        return filePath;
    }
}

file sealed class Progress : IProgress<double>
{
    private static readonly TimeSpan ReportInterval = TimeSpan.FromSeconds(0.1);

    private readonly ILogger logger;
    private readonly string title;

    public Progress(ILogger logger, string title)
    {
        this.logger = logger;
        this.title = title;
    }

    private int lastProgress;
    private Stopwatch? stopwatch;

    public void Report(double value)
    {
        int progress = (int)Math.Floor(value);
        bool needToReport =
            stopwatch == null || (stopwatch != null && stopwatch?.Elapsed > ReportInterval) || progress == 100;

        if (lastProgress != progress && needToReport)
        {
            logger.WriteLineInfo($"{title}: {progress}%");
            lastProgress = progress;
            stopwatch = Stopwatch.StartNew();
        }
    }
}

file sealed class ExternalDotMemoryTool : DotMemoryToolBase
{
    private static readonly TimeSpan AttachTimeout = TimeSpan.FromMinutes(5);

    public ExternalDotMemoryTool(
        ILogger logger,
        Uri? nugetUrl = null,
        NuGetApi nugetApi = NuGetApi.V3,
        string? downloadTo = null
    )
        : base(logger, nugetUrl, nugetApi, downloadTo) { }

    protected override bool AttachOnly => true;

    protected override void Attach(DiagnoserActionParameters parameters, string snapshotFile)
    {
        var logger = parameters.Config.GetCompositeLogger();

        string runnerPath = GetRunnerPath();
        int pid = parameters.Process.Id;
        string arguments = $"attach {pid} --save-to-file=\"{snapshotFile}\" --service-output";

        logger.WriteLineInfo($"Starting process: '{runnerPath} {arguments}'");

        var processStartInfo = new ProcessStartInfo
        {
            FileName = runnerPath,
            WorkingDirectory = "",
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        var attachWaitingTask = new TaskCompletionSource<bool>();
        var process = new Process { StartInfo = processStartInfo };
        try
        {
            process.OutputDataReceived += (_, args) =>
            {
                string? content = args.Data;
                if (content != null)
                {
                    logger.WriteLineInfo("[DotMemory] " + content);
                    if (content.Contains("##DotMemory[\"started\""))
                        attachWaitingTask.TrySetResult(true);
                }
            };
            process.ErrorDataReceived += (_, args) =>
            {
                string? content = args.Data;
                if (content != null)
                    logger.WriteLineError("[DotMemory] " + args.Data);
            };
            process.Exited += (_, _) =>
            {
                attachWaitingTask.TrySetResult(false);
            };
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
        }
        catch (Exception e)
        {
            attachWaitingTask.TrySetResult(false);
            logger.WriteLineError(e.ToString());
        }

        if (!attachWaitingTask.Task.Wait(AttachTimeout))
            throw new Exception(
                $"Failed to attach DotMemory to the target process (timeout: {AttachTimeout.TotalSeconds} sec"
            );
        if (!attachWaitingTask.Task.Result)
            throw new Exception($"Failed to attach DotMemory to the target process (ExitCode={process.ExitCode})");
    }

    protected override void StartCollectingData() { }

    protected override void SaveData() { }

    protected override void Detach() { }
}
