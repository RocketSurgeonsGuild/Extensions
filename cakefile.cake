#load "nuget:?package=Rocket.Surgery.Cake.Library&version=0.9.2";

Task("Default")
    .IsDependentOn("dotnetcore");

RunTarget(Target);
