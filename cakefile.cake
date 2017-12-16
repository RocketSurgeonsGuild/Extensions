#load "nuget:?package=Rocket.Surgery.Cake.Library&version=0.1.10";

Task("Default")
    .IsDependentOn("dotnet");

RunTarget(Target);
