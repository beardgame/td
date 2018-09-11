
var target = Argument("target", "Default");
var buildDir = Directory("./bin");
var releaseConfig = "Release";
var solutionFile = File("TD.sln");
var tdProjectName = "Bearded.TD";
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

string getTdVersionFromBinary(string config)
{
    var assemblyPath = buildDir + Directory(tdProjectName) + Directory(config);
    var assembly = Assembly.LoadFrom(assemblyPath);
    var configType = assembly.GetType("Bearded.TD.Config");
    var versionProperty = configType.GetProperty("VersionString");

    return (string)versionProperty.getValue(null, null);
}

Task("Clean")
    .Does(() => CleanDirectory(buildDir));

Task("NuGet.Restore")
    .IsDependentOn("Clean")
    .Does(() => NuGetRestore(solutionFile));

Task("Build.Release")
    .IsDependentOn("NuGet.Restore")
    .Does(() =>
    {
        build(releaseConfig);

        var tdBinDir = getOutDir(tdProjectName, releaseConfig);

        var tdVersion = getTdVersionFromBinary(releaseConfig);
        var tdZipFile = artifactDir + File($"bearded.td-{tdVersion}.zip");

        Zip(tdBinDir, tdZipFile)
    });


Task("Default")
    .IsDependentOn("Build.Release");

RunTarget(target);
