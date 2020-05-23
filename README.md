# LibLSD

![Nuget Version](https://img.shields.io/nuget/v/libLSD)
![License](https://img.shields.io/github/license/figglewatts/libLSD)
![Build status](https://img.shields.io/github/workflow/status/figglewatts/libLSD/CD)

A C# library for loading LSD: Dream Emulator data files.

## Installation
LibLSD is available as a [NuGet package](https://www.nuget.org/packages/libLSD/).

## Documentation
The documentation can be found at: https://www.figglewatts.co.uk/libLSD/

## General usage
Basically this library contains structs for most of the data file types that can be found in LSD: Dream Emulator.

All of these structs have a constructor that takes a `BinaryReader`. If you open a file and pass it to a `BinaryReader`,
you can then use that `BinaryReader` to construct the object.

For example, here's how you open an LBD file:
```C#
LBD lbd;
using (BinaryReader br = new BinaryReader(File.Open("path/to/file.LBD", FileMode.Open)))
{
    lbd = new LBD(br);
}

// now you can access the LBD data here...
```
