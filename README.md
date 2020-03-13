# SAFE Plugin Template [POC]
It's a POC of a template for [SAFE](https://github.com/SAFE-Stack/SAFE-template).

# How to use it?
* install template as a .net core template from nuget (not available right now),
* install template as a .net core template locally.

# How to install it as a template from nuget (not available right now)?
* run `dotnet new -i SAFE.Plugin.Template`,
* create new project executing `dotnet new SAFEPlugin -n PROJECT_NAME -o FOLDER_NAME -lang F# ...`.

# How to install it as a template locally?
* download repository,
* run command `dotnet new -i PATH`, where PATH is a path to a folder where project is located,
* create new project executing `dotnet new SAFEPlugin -n PROJECT_NAME -o FOLDER_NAME -lang F# ...` (since there is a [known issue in dotnet cli](https://github.com/dotnet/templating/issues/1401) `-lang F#` parameter is currently required),
* `dotnet restore`,
* `dotnet build`.

# Available parameters
* `-creator` - name of a creator of nuget packages for Runner and Plugin, default value is `Robert Kubica EEEEEE`,
* `-target` - which part of SAFE the plugin should target, available options are: `server`, `client`, `shared`. Default is `shared`
