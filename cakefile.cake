#load "nuget:?package=Rocket.Surgery.Cake.Library&version=0.8.4";

Task("Default")
    .IsDependentOn("dotnetcore");

RunTarget(Target);
