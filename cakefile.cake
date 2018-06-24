#load "nuget:?package=Rocket.Surgery.Cake.Library&version=0.5.0";

Task("Default")
    .IsDependentOn("dotnet");

RunTarget(Target);
