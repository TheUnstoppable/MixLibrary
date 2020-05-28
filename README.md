# MixLibrary
MixLibrary is a basic API to read C&C: Renegade MIX files. Operationality in other games are not tested and it is only intended to work with C&C: Renegade packages.

Example Usages: 

- To create an instance of `MixPackageClass` and download from a remote host:
```csharp
Uri url = new Uri("http://path-to/your.mix");
MixPackageClass MIX = MixClass.Load(url);
```

- To load a MIX package from a directory:
```csharp
string Location = @"C:\Path\To.Mix";
MixPackageClass MIX = MixClass.Load(Location);
```
Game uses a name-based integrity checking system to check if file names and indexes are properly coded into the package. And this API does the same check too. All `MixClass.Load` functions has a second parameter named `IgnoreCRCMismatches`. You can set it to `true` to ignore the integrity check, even though file is incorrect (not recommended to ignore).

You can modify MIX package and it's files and/or delete files from it and then save into byte array, in a ``Stream``, or to a file.
There is an example of saving the MIX package into a file:
```csharp
MixClass.Save(MIX, @"C:\Path\To\New.Mix");
```
