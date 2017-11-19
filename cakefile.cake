#load "nuget:?package=Rocket.Surgery.Cake.Library&version=0.1.8";

Task("Default")
    .IsDependentOn("dotnet");

RunTarget(Target);
