using System.Diagnostics;
using System.Reflection;

using WoWSyncNotes.Common.Singletons;

namespace WoWSyncNotes.Common.AssemblyInfo;

public class AppInfo : Singleton<AppInfo>
{
    // Properties

    public Assembly Assembly { get; set; } = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

    public string? FileName
    {
        get
        {
            string? filePathAndName = Process.GetCurrentProcess().MainModule?.FileName;
            return string.IsNullOrEmpty(filePathAndName) ? null : Path.GetFileName(filePathAndName);
        }
    }
    public string? PathName
    {
        get
        {
            Uri location = new(AppContext.BaseDirectory);
            FileInfo fi = new(location.AbsolutePath);
            return fi.Directory?.FullName;
        }
    }
    public string? Company
    {
        get
        {
            AssemblyCompanyAttribute? attr = Assembly.GetCustomAttribute<AssemblyCompanyAttribute>();
            return attr != null && attr.Company != null ? attr.Company : null;
        }
    }
    public string? Copyright
    {
        get
        {
            AssemblyCopyrightAttribute? attr = Assembly.GetCustomAttribute<AssemblyCopyrightAttribute>();
            return attr != null && attr.Copyright != null ? attr.Copyright : null;
        }
    }
    public string? Description
    {
        get
        {
            AssemblyDescriptionAttribute? attr = Assembly.GetCustomAttribute<AssemblyDescriptionAttribute>();
            return attr != null && attr.Description != null ? attr.Description : null;
        }
    }
    public string? Product
    {
        get
        {
            AssemblyProductAttribute? attr = Assembly.GetCustomAttribute<AssemblyProductAttribute>();
            return attr != null && attr.Product != null ? attr.Product : null;
        }
    }
    public string? Title
    {
        get
        {
            AssemblyTitleAttribute? attr = Assembly.GetCustomAttribute<AssemblyTitleAttribute>();
            return attr != null && attr.Title != null ? attr.Title : null;
        }
    }
    public string? FileVersion
    {
        get
        {
            AssemblyFileVersionAttribute? attr = Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
            return attr != null && attr.Version != null ? attr.Version : null;
        }
    }
    public Version? Version => Assembly.GetName().Version ?? null;
    public int? VersionMajor => Version?.Major;
    public int? VersionMinor => Version?.Minor;
    public int? VersionBuild => Version?.Build;
    public int? VersionRevision => Version?.Revision;
    public string? VersionMajorMinor =>
        $"{(VersionMajor == null ? "(null)" : VersionMajor)}.{(VersionMinor == null ? "(null)" : VersionMinor)}";
    public string? VersionMajorMinorBuild =>
        $"{(VersionMajor == null ? "(null)" : VersionMajor)}.{(VersionMinor == null ? "(null)" : VersionMinor)}.{(VersionBuild == null ? "(null)" : VersionBuild)}";

    // Methods

    public List<string> ToList()
    {
        List<string> list = [
            $"{"FileName",-22}: {FileName}",
            $"{"PathName",-22}: {PathName}",
            $"{"Company",-22}: {Company}",
            $"{"Copyright",-22}: {Copyright}",
            $"{"Description",-22}: {Description}",
            $"{"FileVersion",-22}: {FileVersion}",
            $"{"Product",-22}: {Product}",
            $"{"Title",-22}: {Title}",
            $"{"Version",-22}: {Version}",
            $"{"VersionMajor",-22}: {VersionMajor}",
            $"{"VersionMinor",-22}: {VersionMinor}",
            $"{"VersionBuild",-22}: {VersionBuild}",
            $"{"VersionRevision",-22}: {VersionRevision}",
            $"{"VersionMajorMinor",-22}: {VersionMajorMinor}",
            $"{"VersionMajorMinorBuild",-22}: {VersionMajorMinorBuild}"
        ];

        return list;
    }
}
