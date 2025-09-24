
var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

var projects = new List<string>
	{
		"Fabrica.Api.Support",
		"Fabrica.App",
		"Fabrica.App.Endpoints",
		"Fabrica.App.Persistence",
		"Fabrica.Aws",
		"Fabrica.Core",
		"Fabrica.Endpoints",
		"Fabrica.Http",
		"Fabrica.Identity.Client",
		"Fabrica.Identity.Client.DynamoDb",		
		"Fabrica.Identity.Keycloak",
		"Fabrica.Mediator",
		"Fabrica.One",
		"Fabrica.Patch",
		"Fabrica.Persistence.Ef",
		"Fabrica.Rql",
		"Fabrica.Rql.Parser",
		"Fabrica.Rules",
		"Fabrica.Watch",
		"Fabrica.Watch.Http",
		"Fabrica.Watch.Mongo",
		"Fabrica.Watch.Realtime"
	};

var version = "";


//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory("./packages");
});


Task("Build")
    .IsDependentOn("Clean")
    .Does(() =>
{
    DotNetBuild("./fabrica-thin.sln", new DotNetBuildSettings
    {
        Configuration = configuration,
    });
	
});


Task("Version")
    .IsDependentOn("Build")
    .Does(() =>
{

	var propsFile = "./fabrica-thin.build.props";
	var readedVersion = XmlPeek(propsFile, "//Version");
	var currentVersion = new Version(readedVersion);

	var semVersion = new Version(currentVersion.Major, currentVersion.Minor, currentVersion.Build + 1);
	version = semVersion.ToString();

	XmlPoke(propsFile, "//Version", version);

});


Task("Pack")
    .IsDependentOn("Version")
    .Does(() =>
{


	foreach( var project in projects )
	{

		DotNetPack( $"./{project}/{project}.csproj", new DotNetPackSettings
		{
			Configuration = configuration,
			NoBuild = true,
			OutputDirectory = "./packages",
			IncludeSymbols = false,
			ArgumentCustomization = args => args.Append($"/p:PackageVersion={version}")
		});

	}

});

Task("Push")
    .IsDependentOn("Pack")
    .Does(() =>
{

	var pushUrl = EnvironmentVariable("MYGET_FABRICA_NIGHTLY_URL");

	foreach( var project in projects )
	{

		DotNetNuGetPush( $"./packages/{project}.{version}.nupkg", new DotNetNuGetPushSettings
		{
			Source = pushUrl
		});

	}

});


Task("Default")
    .IsDependentOn("Push");

RunTarget(target);