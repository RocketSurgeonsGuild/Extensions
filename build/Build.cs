#:package Microsoft.VisualStudio.SolutionPersistence
#:package Sourcy.Git
#:package Sourcy.DotNet
#:package Rocket.Surgery.ModularPipelines.Extensions
#:property ImportConventions=true
#:property JsonSerializerIsReflectionEnabledByDefault=true

using Build;
using ModularPipelines;
using ModularPipelines.Plugins;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.ModularPipelines.Extensions;

var pipelineBuilder = Pipeline.CreateBuilder(args);
PluginRegistry.Register(new ConventionsPlugin(ConventionContextBuilder.Create(Imports.Instance)
.AddIfMissing(nameof(MyAssembly.Project.BuildScriptsRoot), MyAssembly.Project.BuildScriptsRoot)));
await pipelineBuilder.Build().RunAsync();
