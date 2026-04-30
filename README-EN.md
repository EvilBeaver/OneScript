# OneScript #

[![Join telegram chat](https://img.shields.io/badge/chat-telegram-blue?style=flat&logo=telegram)](https://t.me/oscript_library) [![DEV Build Status](https://build.oscript.io/buildStatus/icon?job=1Script%2Fdevelop&style=flat-square&subject=dev)](https://build.oscript.io/job/1Script/job/develop/) [![STABLE Build Status](https://build.oscript.io/buildStatus/icon?job=1Script%2Fmaster&style=flat-square&subject=stable)](https://build.oscript.io/job/1Script/job/master/)

## The project is an independent cross-platform implementation of a virtual machine that executes scripts written in the 1C:Enterprise language ##

![Logo](.github/logo-small-2.png) ![Logo](.github/logo-small.png)

The 1C:Enterprise system libraries are not used and installation of the 1C:Enterprise system on the target machine is not required.

In other words, this is a tool for writing and executing programs in the 1C language without using the 1C:Enterprise platform.

## Name and pronunciation ##

The project is called OneScript, can be abbreviated to the title 1Script when writing. Pronounced as `[uanskript]`.

OneScript allows you to create and execute text scripts written in a language familiar to any specialist working with the 1C:Enterprise system. Using a familiar language for script automation significantly increases a specialist's productivity by simplifying the automation of manual operations.

## Project site ##

Main information about the project, releases and technical documentation are located on the official website

[https://oscript.io](https://oscript.io)

## Library of useful scripts ##

The OneScript distribution already includes a set of the most commonly used packages. These, as well as other packages, are located in the [oscript-library](https://github.com/oscript-library) repository and are available to everyone. There is a package manager [opm](https://github.com/oscript-library/opm).

## Installation ##

### Windows ###

- (interactively) download from the [official website](https://oscript.io) or installer from the [Releases](https://github.com/EvilBeaver/OneScript/releases) section and run it. Then, Next, Done.

### Linux ###

- Download the ZIP archive for Linux from the [Releases](https://github.com/EvilBeaver/OneScript/releases) section or from the [official website](https://oscript.io).
- Extract the archive to a convenient directory.
- Set executable permissions:
  ```bash
  chmod +x oscript
  ```

### MacOS ###

- Download the ZIP archive for macOS (x64 or arm64) from the [Releases](https://github.com/EvilBeaver/OneScript/releases) section or from the [official website](https://oscript.io).
- Extract the archive to a convenient directory.
- Perform additional configuration to remove quarantine and sign the binary:
  ```bash
  chmod +x ./oscript
  xattr -d com.apple.quarantine *.dylib oscript
  codesign -s - ./oscript
  ```


# Manual local build

## Preparation

To build the project you need:

* [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0) (the project's target framework is `net8.0`).
* A C++ compiler — only required to build the native bridge `ScriptEngine.NativeApi` (support for native add-ins compatible with the 1C NativeApi). On Windows the easiest way to get one is to install [MS Build Tools](https://visualstudio.microsoft.com/visual-studio-build-tools/) or Visual Studio with the "Desktop development with C++" workload. If a C++ compiler is not available, see the `NoCppCompiler` parameter below.

> Distribution links may change over time, their relevance is not guaranteed.

## Build

The build is driven by MSBuild via the `Build.csproj` script in the repository root. Commands can be run with `msbuild` (Developer Command Prompt installed together with MS Build Tools/Visual Studio) or with `dotnet msbuild` (cross-platform).

Main targets:

* `CleanAll` — clean previous build results;
* `BuildAll` — build binary files for distribution (FDD, SCD, debugger; native components are also built when a C++ compiler is available);
* `MakeCPP`, `MakeFDD`, `MakeSCD`, `BuildDebugger` — individual targets that build specific parts of the distribution;
* `GatherLibrary` — download and stage the base set of libraries (`opm`, `asserts`, `logos`, `fs`, `tempfiles`, `cli`);
* `PrepareDistributionFiles` — build full distribution contents (including libraries and documentation);
* `PackDistributions` — pack the distribution contents into ZIP archives for all supported platforms;
* `BuildDocumentation` — generate the platform reference (markdown + json);
* `CreateNuget` / `PublishNuget` — build and publish NuGet packages;
* `Test` (`UnitTests`, `ScriptedTests`) — run unit and acceptance (BSL) tests.

**Build parameters**

* `VersionPrefix` — main part of the release number, for example `2.0.0` (defaults to `2.0.0`);
* `VersionSuffix` — optional SemVer suffix, for example `beta-786`;
* `NoCppCompiler` — when set to `True`, native C++ components (NativeApi) are not built and not included in the distribution (use it when no C++ compiler is installed);
* `Configuration` — build configuration, defaults to `Release`. For a debug build on Linux use `LinuxDebug`.

All build artifacts are placed in the `built` directory at the repository root.

### Building distribution contents in a separate directory

```bat
dotnet msbuild Build.csproj /t:CleanAll;PrepareDistributionFiles
```

### Building with manual version specification

```bat
dotnet msbuild Build.csproj /t:CleanAll;PrepareDistributionFiles /p:VersionPrefix=2.0.0
```

### Building ZIP distributions

```bat
dotnet msbuild Build.csproj /t:CleanAll;PrepareDistributionFiles;PackDistributions /p:VersionPrefix=2.0.0 /p:VersionSuffix=preview223
```

### Building without C++ components (no NativeApi)

```bat
dotnet msbuild Build.csproj /t:CleanAll;PrepareDistributionFiles /p:NoCppCompiler=True
```

### Documentation generation

```bat
dotnet msbuild Build.csproj /t:BuildDocumentation
```

# Testing

The project has two layers of tests:

* **C# unit tests** — located in `src/Tests/*` (xUnit/NUnit). Run them via `dotnet test` in the corresponding test project directory or all at once:
  ```bat
  dotnet msbuild Build.csproj /t:UnitTests
  ```
* **BSL acceptance tests** — located in the `tests/` directory and executed by `testrunner.os` against a freshly built `oscript`. The repository includes wrapper scripts:
  ```bat
  rem Windows
  tests\run-bsl-tests.cmd src\oscript\bin\Debug\net8.0\oscript.exe
  ```

  ```bash
  # Linux/macOS
  tests/run-bsl-tests.sh src/oscript/bin/Debug/net8.0/oscript
  ```

  Before running the acceptance tests, build `oscript`:
  ```bat
  dotnet build src/oscript/oscript.csproj
  ```

# Developer documentation

If you want to contribute to the project, take a look at the additional documents in the [`docs/`](docs/) directory:

* [`docs/developer_docs.md`](docs/developer_docs.md) — project architecture, solution layout and guided tour of the source code.
* [`docs/contexts.md`](docs/contexts.md) — a practical guide to adding BSL contexts, methods, properties and global functions.
* [`CODESTYLE.md`](CODESTYLE.md) — C# code style requirements.

