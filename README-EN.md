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

Links to distributions are provided below, however, please note that links may change over time and their relevance is not guaranteed. You need dotnet SDK and C++ compiler, which can be downloaded from anywhere you can find.

* Install [MS BuildTools](https://visualstudio.microsoft.com/ru/thank-you-downloading-visual-studio/?sku=buildtools&rel=16), when installing enable targeting for .net6, .net4.8, install C++ compiler.

## Build

Launch Developer Command Prompt (will appear in the Start menu after installing MSBuildTools or Visual Studio). Navigate to the OneScript repository directory. The following are commands in the Developer Command Prompt console.
Build is performed using msbuild. Targets:

* CleanAll - clean previous build results
* BuildAll - prepare files for distribution
* MakeCPP;MakeFDD;MakeSCD;BuildDebugger - separate build targets for preparing different types of distributions
* PrepareDistributionFiles - build full distribution packages (including libraries)
* PackDistributions - prepare ZIP archives for distribution
* CreateNuget - create packages for publishing to NuGet

**Build parameters**

* VersionPrefix - release number prefix, its main part, for example, 2.0.0
* VersionSuffix - version suffix, which usually acts as an arbitrary versioning suffix according to semver, for example, beta-786 (optional)
* NoCppCompiler - if True - C++ compiler is not installed, C++ components (NativeApi support) will not be added to the build

All distribution files will be placed in the `built` directory at the root of the 1Script repository

### Building distribution contents in a separate directory

```bat
msbuild Build.csproj /t:CleanAll;PrepareDistributionFiles
```

### Building with manual version specification

```bat
msbuild Build.csproj /t:CleanAll;PrepareDistributionFiles /p:VersionPrefix=2.0.0
```

### Building ZIP distributions

```bat
msbuild Build.csproj /t:CleanAll;PrepareDistributionFiles;PackDistributions /p:VersionPrefix=2.0.0 /p:VersionSuffix=preview223
```

### Documentation generation

```bat
msbuild Build.csproj /t:BuildDocumentation
```
