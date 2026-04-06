using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using FS = Microsoft.VisualBasic.FileIO.FileSystem;

namespace Dassie.Core.Actions;

internal static class FileSystemHelpers
{
    public static void CopyOrMove(IEnumerable<IFileSystemInfo> fileSystemEntries, string targetDir, bool copy, bool preserveStructure, bool overwrite)
    {
        if (!preserveStructure)
        {
            foreach (IFileSystemInfo fsInfo in fileSystemEntries)
            {
                string source = fsInfo.FullName;
                string target = Path.Combine(targetDir, Path.GetFileName(fsInfo.FullName));

                if (copy)
                {
                    if (File.Exists(source))
                    {
                        EmitBuildLogMessageFormatted(nameof(StringHelper.FileSystemHelpers_CopyingFile), [source, target], 2);

                        try
                        {
                            FS.CopyFile(source, target, overwrite);

                        }
                        catch (Exception ex)
                        {
                            EmitWarningMessageFormatted(
                                0, 0, 0,
                                DS0285_FileSystemOperationFailed,
                                nameof(StringHelper.FileSystemHelpers_FailedToCopyFile), [source, ex.Message],
                                ProjectConfigurationFileName);
                        }
                    }
                    else if (Directory.Exists(source))
                    {
                        EmitBuildLogMessageFormatted(nameof(StringHelper.FileSystemHelpers_CopyingDirectory), [source, target], 2);

                        try
                        {
                            FS.CopyDirectory(source, target, overwrite);
                        }
                        catch (Exception ex)
                        {
                            EmitWarningMessageFormatted(
                                0, 0, 0,
                                DS0285_FileSystemOperationFailed,
                                nameof(StringHelper.FileSystemHelpers_FailedToCopyDirectory), [source, ex.Message],
                                ProjectConfigurationFileName);
                        }
                    }

                    continue;
                }

                if (File.Exists(source))
                {
                    EmitBuildLogMessageFormatted(nameof(StringHelper.FileSystemHelpers_MovingFile), [source, target], 2);

                    try
                    {
                        FS.MoveFile(source, target, overwrite);
                    }
                    catch (Exception ex)
                    {
                        EmitWarningMessageFormatted(
                            0, 0, 0,
                            DS0285_FileSystemOperationFailed,
                            nameof(StringHelper.FileSystemHelpers_FailedToMoveFile), [source, ex.Message],
                            ProjectConfigurationFileName);
                    }
                }
                else if (Directory.Exists(source))
                {
                    EmitBuildLogMessageFormatted(nameof(StringHelper.FileSystemHelpers_MovingDirectory), [source, target], 2);

                    try
                    {
                        FS.MoveDirectory(source, target, overwrite);
                    }
                    catch (Exception ex)
                    {
                        EmitWarningMessageFormatted(
                            0, 0, 0,
                            DS0285_FileSystemOperationFailed,
                            nameof(StringHelper.FileSystemHelpers_FailedToMoveDirectory), [source, ex.Message],
                            ProjectConfigurationFileName);
                    }
                }
            }

            return;
        }

        throw new NotImplementedException();
    }

    public static void Delete(IEnumerable<IFileSystemInfo> fileSystemEntries)
    {
        foreach (IFileSystemInfo info in fileSystemEntries)
        {
            string path = info.FullName;

            if (File.Exists(path))
            {
                EmitBuildLogMessageFormatted(nameof(StringHelper.FileSystemHelpers_DeletingFile), [path], 2);

                try
                {
                    FS.DeleteFile(path);
                }
                catch (Exception ex)
                {
                    EmitWarningMessageFormatted(
                        0, 0, 0,
                        DS0285_FileSystemOperationFailed,
                        nameof(StringHelper.FileSystemHelpers_FailedToDeleteFile), [path, ex.Message],
                        ProjectConfigurationFileName);
                }
            }
            else if (Directory.Exists(path))
            {
                EmitBuildLogMessageFormatted(nameof(StringHelper.FileSystemHelpers_DeletingDirectory), [path], 2);

                try
                {
                    FS.DeleteDirectory(path, DeleteDirectoryOption.DeleteAllContents);
                }
                catch (Exception ex)
                {
                    EmitWarningMessageFormatted(
                        0, 0, 0,
                        DS0285_FileSystemOperationFailed,
                        nameof(StringHelper.FileSystemHelpers_FailedToDeleteDirectory), [path, ex.Message],
                        ProjectConfigurationFileName);
                }
            }
        }
    }
}