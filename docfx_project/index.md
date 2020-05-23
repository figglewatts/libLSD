# LibLSD
A C# library for loading LSD: Dream Emulator data files.

## Installation
LibLSD is available as a [NuGet package](https://www.nuget.org/packages/libLSD/).

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

You can do this with any of the formats listed below.

## Formats
LibLSD currently supports the following formats:
- [**LBD**](api/libLSD.Formats.LBD.yml): Level data.
- [**MOM**](api/libLSD.Formats.MOM.yml): Interactive objects with animations.
- [**MML**](api/libLSD.Formats.MML.yml): Containers for multiple MOM files.
- [**MOS**](api/libLSD.Formats.MOS.yml): Container for TOD animation data.
- [**TIM**](api/libLSD.Formats.TIM.yml): Texture data.
- [**TIX**](api/libLSD.Formats.TIX.yml): Archives of multiple textures.
- [**TMD**](api/libLSD.Formats.TMD.yml): 3D model data.
- [**TOD**](api/libLSD.Formats.TOD.yml): 3D model animation data.