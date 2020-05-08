#addin "Cake.FileHelpers"

using System.Reflection;
using static System.Reflection.BindingFlags;

var target = Argument("target", "Default");
var buildDir = Directory("./bin");
var releaseConfig = "Release";
var solutionFile = File("TD.sln");
var tdProjectName = "Bearded.TD";
var tdBinaryFile = File($"{tdProjectName}.exe");
var tdTestProjectName = "Bearded.TD.Tests";
var weaverTeastProjectName = "Bearded.TD.Weavers.Tests";
var artifactDir = Directory(EnvironmentVariable("ARTIFACT_OUT_DIR") ?? "./bin/artifacts");

void build(string config)
{
    var settings = new MSBuildSettings {
        Configuration = config,
    };

    MSBuild(solutionFile, settings);
}

ConvertableDirectoryPath getOutDir(string project, string config)
{
    return buildDir + Directory(project) + Directory(config);
}

string getTDVersionFromBinary(string config)
{
    var assemblyPath = buildDir + Directory(tdProjectName) + Directory(config) + tdBinaryFile;

    Debug($"Loading binary at '{assemblyPath}'...");
    var assembly = Assembly.LoadFrom(assemblyPath);

    Debug($"Looking for config type...");
    var configType = assembly.GetType("Bearded.TD.Config");

    Debug($"Looking for version property...");
    var versionProperty = configType.GetProperty("VersionString", Static | Public);

    Debug($"Reading version property...");
    return (string)versionProperty.GetValue(null);
}

Task("PrepareDirectories")
    .Does(() => CreateDirectory(artifactDir));

Task("Clean")
    .Does(() => CleanDirectory(buildDir));

Task("NuGet.Restore")
    .IsDependentOn("Clean")
    .Does(() => NuGetRestore(solutionFile));

Task("Build")
    .IsDependentOn("PrepareDirectories")
    .IsDependentOn("NuGet.Restore")
    .Does(() =>
    {
        Information($"Building solution with {releaseConfig} config...");
        build(releaseConfig);
    });

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
    {
        var testProjects = GetFiles("./**/*.Tests.csproj");
        var testResultsDir = Directory(EnvironmentVariable("BITRISE_TEST_RESULT_DIR"));

        foreach (var projectPath in testProjects)
        {
            Information($"Running tests for {projectPath.FullPath}");

            var testName = projectPath.GetFilenameWithoutExtension().FullPath;
            var projectTestResultsDir = testResultsDir + Directory(testName);

            CreateDirectory(projectTestResultsDir);
            FileWriteText(projectTestResultsDir + File("test-info.json"), $"{{ \"test-name\" : \"{testName}\" }}");

            var xmlOutFile = (FilePath) (projectTestResultsDir + File("results.xml"));

            DotNetCoreTool(
                projectPath: projectPath.FullPath,
                command: "xunit",
                arguments: new ProcessArgumentBuilder() 
                    .Append($"-configuration {releaseConfig}")
                    .Append("-nobuild")
                    .Append($"-xml {xmlOutFile.FullPath}")
            );
            Debug($"Test results should now be available at {projectTestResultsDir.FullPath}");
        }
    });

Task("Pack")
    .IsDependentOn("Build")
    .Does(() =>
    {
        var tdBinDir = getOutDir(tdProjectName, releaseConfig);
        Debug($"{tdProjectName} should now be available at '{tdBinDir}'.");

        Debug($"Extracting {tdProjectName} version from binary...");
        var tdVersion = getTDVersionFromBinary(releaseConfig);
        Debug($"Found {tdProjectName} version to be '{tdVersion}'.");

        var tdZipFile = artifactDir + File($"bearded.td.{tdVersion}.zip");

        Debug($"Archiving '{tdBinDir}' to '{tdZipFile}'...");
        CreateDirectory(artifactDir);
        Zip(tdBinDir, tdZipFile);

        Information($"Done archiving! Find your artifact at '{tdZipFile}'.");
    });

Task("Default")
    .IsDependentOn("Pack")
    .IsDependentOn("Test");

RunTarget(target);
