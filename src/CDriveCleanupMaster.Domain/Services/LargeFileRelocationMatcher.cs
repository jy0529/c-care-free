using CDriveCleanupMaster.Domain.Models;

namespace CDriveCleanupMaster.Domain.Services;

public static class LargeFileRelocationMatcher
{
    public static LargeFileItem WithRelocationHint(LargeFileItem item)
    {
        string? hint = GetHint(item.Path);
        if (hint is null)
        {
            return item;
        }

        return new LargeFileItem
        {
            Path = item.Path,
            Name = item.Name,
            Extension = item.Extension,
            Bytes = item.Bytes,
            ModifiedAt = item.ModifiedAt,
            RelocationHint = hint
        };
    }

    public static IReadOnlyList<LargeFileItem> ApplyHints(IEnumerable<LargeFileItem> items) =>
        items.Select(WithRelocationHint).ToArray();

    public static string? GetHint(string path)
    {
        if (path.Contains(@"\Docker\wsl\", StringComparison.OrdinalIgnoreCase)
            && path.EndsWith(".vhdx", StringComparison.OrdinalIgnoreCase))
        {
            return "Docker 数据盘：在 Docker Desktop → Settings → Resources 迁移到其他盘";
        }

        if (path.Contains(@"\Docker\", StringComparison.OrdinalIgnoreCase)
            && path.EndsWith(".vhdx", StringComparison.OrdinalIgnoreCase))
        {
            return "Docker 虚拟磁盘：建议在 Docker Desktop 设置中迁移数据目录";
        }

        if (path.Contains(@"\Packages\", StringComparison.OrdinalIgnoreCase)
            && (path.EndsWith("ext4.vhdx", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith("disk.vhdx", StringComparison.OrdinalIgnoreCase)
                || path.Contains(@"\LocalState\", StringComparison.OrdinalIgnoreCase)
                    && path.EndsWith(".vhdx", StringComparison.OrdinalIgnoreCase)))
        {
            return "WSL 发行版磁盘：可用 wsl --export / wsl --import 迁移到其他盘";
        }

        if (path.Contains(@"\Android\Sdk\", StringComparison.OrdinalIgnoreCase)
            || path.Contains(@"\.android\avd\", StringComparison.OrdinalIgnoreCase))
        {
            return "Android 模拟器/SDK：可在 Android Studio 中把 SDK 或 AVD 目录改到其他盘";
        }

        if (path.Contains(@"\VMware\", StringComparison.OrdinalIgnoreCase)
            && (path.EndsWith(".vmdk", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".vmem", StringComparison.OrdinalIgnoreCase)))
        {
            return "VMware 虚拟磁盘：在虚拟机设置中迁移磁盘文件位置";
        }

        if (path.Contains(@"VirtualBox\", StringComparison.OrdinalIgnoreCase)
            && (path.EndsWith(".vdi", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".vmdk", StringComparison.OrdinalIgnoreCase)))
        {
            return "VirtualBox 虚拟磁盘：在介质管理器中迁移到其他盘";
        }

        if (path.Contains(@"\Hyper-V\", StringComparison.OrdinalIgnoreCase)
            && path.EndsWith(".vhdx", StringComparison.OrdinalIgnoreCase))
        {
            return "Hyper-V 虚拟硬盘：在 Hyper-V 管理器中移动存储位置";
        }

        if (path.EndsWith("hiberfil.sys", StringComparison.OrdinalIgnoreCase))
        {
            return "休眠文件：管理员运行 powercfg -h off 可关闭休眠并释放空间";
        }

        if (path.EndsWith("pagefile.sys", StringComparison.OrdinalIgnoreCase))
        {
            return "页面文件：可在系统高级设置中将虚拟内存改到其他盘";
        }

        if (path.Contains(@"\SteamLibrary\", StringComparison.OrdinalIgnoreCase)
            || path.Contains(@"\steamapps\", StringComparison.OrdinalIgnoreCase))
        {
            return "Steam 游戏库：可在 Steam 设置中添加其他盘的库文件夹并迁移游戏";
        }

        return null;
    }
}
