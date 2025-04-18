﻿using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using System.Diagnostics;

namespace BSCGAOCBM2022.GroupA;

[MemoryDiagnoser]
[CategoriesColumn]
[Config(typeof(CustomConfig))]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class Day7Benchmarks
{
    private Input _input = null!;

    [GlobalSetup]
    public void Setup() => _input = Helpers.GetInput(7);

    #region Auros

    [Benchmark(Baseline = Helpers.AurosIsBaseline)]
    [BenchmarkCategory(Helpers.Part1)]
    public int Auros_Part1()
    {
        Auros_FSI? root = null;
        Auros_FSI? current = null;
        List<Auros_FSI> allDirectories = new();

        foreach (var line in _input.Lines)
            ReadInstruction(line);

        void ReadInstruction(string line)
        {
            if (line[0] == '$')
            {
                if (line[2..4] != "cd")
                    return;

                // cd
                var arg = line[5..];
                if (arg == "/")
                {
                    // Create root dir
                    root ??= new Auros_FSI(arg, true, new List<Auros_FSI>());
                    root.Parent = root;
                    current = root;

                    allDirectories.Add(root);
                }
                else if (arg == ".." && current is not null)
                {
                    // Move back
                    current = current.Parent;
                }
                else if (current is not null && current.IsDirectory && current.Children is not null)
                {
                    // Move into directory of {arg}
                    current = current.Children.First(c => c.Name == arg);
                }
            }
            else
            {
                if (current is null || !current.IsDirectory || current.Children is null)
                    return;

                if (line[0..3] == "dir")
                {
                    var dirName = line[4..];
                    Auros_FSI fsi = new(dirName, true, new List<Auros_FSI>());
                    current.Children.Add(fsi);
                    fsi.Parent = current;

                    allDirectories.Add(fsi);
                }
                else
                {
                    var file = line.Split(' ');
                    int fileSize = int.Parse(file[0]);
                    string fileName = file[1];

                    Auros_FSI fsi = new(fileName, false, null);
                    current.Children.Add(fsi);
                    fsi.Parent = current;
                    fsi.Size = fileSize;

                    Auros_FSI parent = current;
                    parent.Size += fileSize;
                    while (parent != root)
                    {
                        parent = parent.Parent;
                        parent.Size += fileSize;
                    }
                }
            }
        }

        var sumOfSmallerDirectories = allDirectories.Where(d => d.IsDirectory && d.Size <= 100_000).Sum(d => d.Size);
        return sumOfSmallerDirectories;
    }

    [Benchmark(Baseline = Helpers.AurosIsBaseline)]
    [BenchmarkCategory(Helpers.Part2)]
    public int? Auros_Part2()
    {
        Auros_FSI? root = null;
        Auros_FSI? current = null;
        List<Auros_FSI> allDirectories = new();

        foreach (var line in _input.Lines)
            ReadInstruction(line);

        void ReadInstruction(string line)
        {
            if (line[0] == '$')
            {
                if (line[2..4] != "cd")
                    return;

                // cd
                var arg = line[5..];
                if (arg == "/")
                {
                    // Create root dir
                    root ??= new Auros_FSI(arg, true, new List<Auros_FSI>());
                    root.Parent = root;
                    current = root;

                    allDirectories.Add(root);
                }
                else if (arg == ".." && current is not null)
                {
                    // Move back
                    current = current.Parent;
                }
                else if (current is not null && current.IsDirectory && current.Children is not null)
                {
                    // Move into directory of {arg}
                    current = current.Children.First(c => c.Name == arg);
                }
            }
            else
            {
                if (current is null || !current.IsDirectory || current.Children is null)
                    return;

                if (line[0..3] == "dir")
                {
                    var dirName = line[4..];
                    Auros_FSI fsi = new(dirName, true, new List<Auros_FSI>());
                    current.Children.Add(fsi);
                    fsi.Parent = current;

                    allDirectories.Add(fsi);
                }
                else
                {
                    var file = line.Split(' ');
                    int fileSize = int.Parse(file[0]);
                    string fileName = file[1];

                    Auros_FSI fsi = new(fileName, false, null);
                    current.Children.Add(fsi);
                    fsi.Parent = current;
                    fsi.Size = fileSize;

                    Auros_FSI parent = current;
                    parent.Size += fileSize;
                    while (parent != root)
                    {
                        parent = parent.Parent;
                        parent.Size += fileSize;
                    }
                }
            }
        }

        const int fileSystemDiskSpace = 70_000_000;
        const int requiredDiskSpaceForUpdate = 30_000_000;
        int diskSpaceToClear = requiredDiskSpaceForUpdate - (fileSystemDiskSpace - root!.Size);

        var directoryToDelete = allDirectories.Where(d => d.IsDirectory && d.Size >= diskSpaceToClear).OrderBy(d => d.Size).FirstOrDefault();
        return directoryToDelete?.Size;
    }

    record class Auros_FSI(string Name, bool IsDirectory, List<Auros_FSI>? Children)
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public Auros_FSI Parent { get; set; } = null!;

        public int Size { get; set; }
    }

    #endregion

    #region Caeden

    [Benchmark(Baseline = Helpers.CaedenIsBaseline)]
    [BenchmarkCategory(Helpers.Part1)]
    public long Caeden_Part1()
    {
        Caeden_DirInfo root = new("/", null);
        Caeden_DirInfo currentDir = root;

        string? currentCommand = string.Empty;
        for (int i = 0; i <_input.Lines.Length; i++)
        {
            currentCommand = _input.Lines[i];

            // We're about to do some serious bullshit
            switch (currentCommand[0])
            {
                case '$':
                    // We only need to do command processing for cd
                    if (currentCommand[2] == 'c')
                    {
                        currentDir = currentCommand[2] switch // Jump to root, dir parent, or dir child
                        {
                            '/' => root,
                            _ => currentDir.ChildDirs.Find(dir => dir.Name == currentCommand[5..]) ?? currentDir.Parent ?? root,
                        };
                    }
                    break;
                case 'd':
                    // This is always a directory within an 'ls' command. Let's pre-create and load this dir
                    currentDir.ChildDirs.Add(new(currentCommand[4..], currentDir));
                    break;
                default:
                    // With our given input, this will always be a file within an 'ls' command
                    var commandComponents = currentCommand.Split(' ');
                    currentDir.Files.Add(new(commandComponents[1], long.Parse(commandComponents[0])));
                    break;
            }
        }

        var sumOfSmallDirs = 0L;
        void IterateAndAddSmallDirSizes(Caeden_DirInfo dir)
        {
            var size = dir.Size;
            if (size <= 100_000) sumOfSmallDirs += size;
            dir.ChildDirs.ForEach(IterateAndAddSmallDirSizes);
        }
        IterateAndAddSmallDirSizes(root);

        return sumOfSmallDirs;
    }

    [Benchmark(Baseline = Helpers.CaedenIsBaseline)]
    [BenchmarkCategory(Helpers.Part2)]
    public long Caeden_Part2()
    {
        Caeden_DirInfo root = new("/", null);
        Caeden_DirInfo currentDir = root;

        string? currentCommand = string.Empty;
        for (int i = 0; i < _input.Lines.Length; i++)
        {
            currentCommand = _input.Lines[i];

            // We're about to do some serious bullshit
            switch (currentCommand[0])
            {
                case '$':
                    // We only need to do command processing for cd
                    if (currentCommand[2] == 'c')
                    {
                        currentDir = currentCommand[2] switch // Jump to root, dir parent, or dir child
                        {
                            '/' => root,
                            _ => currentDir.ChildDirs.Find(dir => dir.Name == currentCommand[5..]) ?? currentDir.Parent ?? root,
                        };
                    }
                    break;
                case 'd':
                    // This is always a directory within an 'ls' command. Let's pre-create and load this dir
                    currentDir.ChildDirs.Add(new(currentCommand[4..], currentDir));
                    break;
                default:
                    // With our given input, this will always be a file within an 'ls' command
                    var commandComponents = currentCommand.Split(' ');
                    currentDir.Files.Add(new(commandComponents[1], long.Parse(commandComponents[0])));
                    break;
            }
        }

        var usedSpace = 70_000_000L - root.Size;
        var cleanupTarget = 30_000_000L;
        var deletionSize = 70_000_000L;
        void IterateAndCheckDirForCleanup(Caeden_DirInfo dir)
        {
            var size = dir.Size;
            if (usedSpace + size >= cleanupTarget && size < deletionSize) deletionSize = size;
            dir.ChildDirs.ForEach(IterateAndCheckDirForCleanup);
        }
        IterateAndCheckDirForCleanup(root);
        return deletionSize;
    }

    record Caeden_DirInfo(string Name, Caeden_DirInfo? Parent)
    {
        public long Size => ChildDirs.Sum(dir => dir.Size) + Files.Sum(file => file.Size);

        internal List<Caeden_DirInfo> ChildDirs = new();
        internal List<Caeden_FileInfo> Files = new();
    }

    record Caeden_FileInfo(string Name, long Size);

    #endregion

    #region Eris

    [Benchmark(Baseline = Helpers.ErisIsBaseline)]
    [BenchmarkCategory(Helpers.Part1)]
    public long Eris_Part1()
    {
        var root = Eris_BuildFileTree();

        long sum = 0;

        void RecursiveFilteredSum(Eris_FolderObject folder, long folderSizeThreshold)
        {
            if (folder.Size < folderSizeThreshold)
            {
                sum += folder.Size;
            }

            foreach (var folderInternal in folder.ChildObjects.OfType<Eris_FolderObject>())
            {
                RecursiveFilteredSum(folderInternal, folderSizeThreshold);
            }
        }

        RecursiveFilteredSum(root, 100000);

        return sum;
    }

    [Benchmark(Baseline = Helpers.ErisIsBaseline)]
    [BenchmarkCategory(Helpers.Part2)]
    public long Eris_Part2()
    {
        var root = Eris_BuildFileTree();
        const long diskSize = 70000000;
        var freeDiskSpace = diskSize - root.Size;

        const long requiredFreeSpace = 30000000;
        var amountToFreeUp = requiredFreeSpace - freeDiskSpace;

        IEnumerable<Eris_FolderObject> RecursiveIterator(Eris_FolderObject folder)
        {
            foreach (var folderInternal in folder.ChildObjects.OfType<Eris_FolderObject>())
            {
                yield return folderInternal;
                foreach (var folderInternalInternal in RecursiveIterator(folderInternal))
                {
                    yield return folderInternalInternal;
                }
            }
        }

        return RecursiveIterator(root).Where(x => x.Size > amountToFreeUp).MinBy(x => x.Size)!.Size;
    }

    private Eris_FolderObject Eris_BuildFileTree()
    {
        var commandLineOutput = _input.Lines;
        var index = 0;
        Eris_FolderObject? root = null;
        Eris_FolderObject? current = null;
        do
        {
            var line = commandLineOutput[index];

            if (Eris_IsCommand(ref line))
            {
                var commandInput = line[2..];
                var commandParts = commandInput.Split(' ');
                switch (commandParts[0])
                {
                    case "cd":
                        root = Eris_HandleCdCommand(commandParts, ref current, ref root, ref index);
                        break;
                    case "ls":
                        index = Eris_HandleLsCommand(commandLineOutput, current, ref index);

                        break;
                }
            }
        } while (index < commandLineOutput.Length);

        return root!;
    }

    private static Eris_FolderObject? Eris_HandleCdCommand(string[] commandParts, ref Eris_FolderObject? current, ref Eris_FolderObject? root, ref int index)
    {
        switch (commandParts[1])
        {
            case "/":
                current = root ??= new Eris_FolderObject { Name = commandParts[1] };
                break;
            case "..":
                current = current!.Parent;
                break;
            default:
                var localCurrent = current!.ChildObjects.OfType<Eris_FolderObject>().FirstOrDefault(x => x.Name == commandParts[1]);
                if (localCurrent == null)
                {
                    localCurrent = new Eris_FolderObject { Name = commandParts[1], Parent = current };
                    current.ChildObjects.Add(localCurrent);
                }

                current = localCurrent;
                break;
        }

        index++;
        return root;
    }

    private static int Eris_HandleLsCommand(string[] commandLineOutput, Eris_FolderObject? current, ref int index)
    {
        current!.ChildObjects.Clear();
        index++;

        do
        {
            var line = commandLineOutput[index];
            if (Eris_IsCommand(ref line))
            {
                break;
            }

            var fileSystemObjectParts = line.Split(' ');
            current.ChildObjects.Add(fileSystemObjectParts[0] == "dir"
                ? new Eris_FolderObject { Name = fileSystemObjectParts[1], Parent = current }
                : new Eris_FileObject
                {
                    Name = fileSystemObjectParts[1],
                    Size = int.Parse(fileSystemObjectParts[0])
                });
            index++;
        } while (index < commandLineOutput.Length);

        return index;
    }

    private static bool Eris_IsCommand(ref string line) => line[0] == '$';

    private class Eris_FileObject : IEris_IFileSystemObject
    {
        public string Name { get; init; } = null!;
        public long Size { get; init; }
    }

    private class Eris_FolderObject : IEris_IFileSystemObject
    {
        public string Name { get; init; } = null!;
        public long Size => ChildObjects.Sum(x => x.Size);
        public Eris_FolderObject Parent { get; init; } = null!;
        public List<IEris_IFileSystemObject> ChildObjects { get; } = new();
    }

    private interface IEris_IFileSystemObject
    {
        string Name { get; init; }
        long Size { get; }
    }

    #endregion

    #region Goobie
    /*
    [Benchmark(Baseline = Helpers.GoobieIsBaseline)]
    [BenchmarkCategory(Helpers.Part1)]
    public int Goobie_Part1()
    {

    }

    [Benchmark(Baseline = Helpers.GoobieIsBaseline)]
    [BenchmarkCategory(Helpers.Part2)]
    public int Goobie_Part2()
    {

    }
    */
    #endregion
}