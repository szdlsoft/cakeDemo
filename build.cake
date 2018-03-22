#addin "nuget:?package=Cake.SquareLogo"
//#addin "nuget:?package=Cake.GithubUtility"

var target = Argument("target", "Default");
var npi = EnvironmentVariable("npi_watch" );

//Task("Create-Logo").Does(() => {
//    var settings = new LogoSettings { Background = "Green" };
//    CreateLogo("Github", "Assets/logo.png", settings);
//});

Task("Publish-Nuget")
    .IsDependentOn("Create-Nuget-Package")
    .Description("Push nuget")
    .Does(() => {
        var nupkg = new DirectoryInfo("./nuget").GetFiles("*.nupkg").LastOrDefault();
        var package = nupkg.FullName;
        Information( $"apikey: {npi}" );
        NuGetPush(package, new NuGetPushSettings {
            Source = "https://www.nuget.org/api/v2/package",
            ApiKey = npi
        });
    });

Task("Build-Release")
    .Description("Build")
    .Does(() => {
        DotNetBuild("./Cake.Watch.sln", settings =>
            settings.SetConfiguration("Release")
            //.SetVerbosity(Core.Diagnostics.Verbosity.Minimal)
            .WithTarget("Build")
            .WithProperty("TreatWarningsAsErrors","true"));
    });

// https://github.com/cake-addin/cake-square-logo/raw/master/Screen/alfresco.png

Task("Create-Nuget-Package")
    .Description("Create pack")
    .IsDependentOn("Build-Release")
    .Does(() => {

        Func<string, string> abs = (path) => new FileInfo(path).FullName;

        CleanDirectory("./nuget");
        var dll = "./Cake.Watch/bin/Release/Cake.Watch.dll";
        var full = abs(dll);
        var xml = abs(dll.Replace(".dll", ".XML"));
        //var logo = CreateRawPath("cake-addin", "cake-watch", "Assets/logo.png");
        var logo = "https://github.com/cake-addin/cake-watch/raw/master/Assets/logo.png";

        var version = ParseAssemblyInfo("./Cake.Watch/Properties/AssemblyInfo.cs").AssemblyVersion;
        var settings   = new NuGetPackSettings {
                                        //ToolPath                = "./tools/nuget.exe",
                                        Id                      = "Cake.Watch_Advance",
                                        Version                 = version,
                                        Title                   = "Cake_Watch_Advance",
                                        Authors                 = new[] {"wk"},
                                        Owners                  = new[] {"wk"},
                                        Description             = "Cake_Watch_Advance",
                                        //NoDefaultExcludes       = true,
                                        Summary                 = "Watch file change",
                                        ProjectUrl              = new Uri("https://github.com/cake-addin/cake-watch"),
                                        IconUrl                 = new Uri(logo),
                                        LicenseUrl              = new Uri("https://github.com/cake-addin/cake-watch"),
                                        Copyright               = "MIT",
                                        ReleaseNotes            = new [] { "New version"},
                                        Tags                    = new [] {"Cake", "Watch" },
                                        RequireLicenseAcceptance= false,
                                        Symbols                 = false,
                                        NoPackageAnalysis       = true,
                                        Files                   = new [] {
                                                                             new NuSpecContent { Source = full, Target = "bin/net46" },
                                                                             new NuSpecContent { Source = xml , Target = "bin/net46" },
                                                                          },
                                        BasePath                = "./",
                                        OutputDirectory         = "./nuget"
                                    };
        NuGetPack(settings);
    });

Task("Default")
 .IsDependentOn("Publish-Nuget");

RunTarget(target);