#load "nuget:?package=Rocket.Surgery.Cake.Library&version=0.1.7";

Task("Default")
    .IsDependentOn("dotnet");

RunTarget(Target);
