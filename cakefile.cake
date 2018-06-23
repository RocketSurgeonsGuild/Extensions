#load "nuget:?package=Rocket.Surgery.Cake.Library&version=0.4.2";

Task("Default")
    .IsDependentOn("dotnet");

RunTarget(Target);
