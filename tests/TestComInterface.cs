using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace VeqloTest
{
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("a08ce4d0-fa25-44ab-b57c-c7b1c323e0b9")]
    public interface IExplorerCommand
    {
        [PreserveSig]
        int GetTitle(IntPtr psiItemArray, [MarshalAs(UnmanagedType.LPWStr)] out string ppszName);
        [PreserveSig]
        int GetIcon(IntPtr psiItemArray, [MarshalAs(UnmanagedType.LPWStr)] out string ppszIcon);
        [PreserveSig]
        int GetToolTip(IntPtr psiItemArray, [MarshalAs(UnmanagedType.LPWStr)] out string ppszInfoTip);
        [PreserveSig]
        int GetCanonicalName(out Guid pguidCommandName);
        [PreserveSig]
        int GetState(IntPtr psiItemArray, bool fOkToBeSlow, out int pCmdState);
        [PreserveSig]
        int Invoke(IntPtr psiItemArray, IntPtr pbc);
        [PreserveSig]
        int GetFlags(out int pFlags);
        [PreserveSig]
        int EnumSubCommands(out IEnumExplorerCommand ppEnum);
    }

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("a88826f8-186f-4987-aade-ea0cef8fbfe8")]
    public interface IEnumExplorerCommand
    {
        [PreserveSig]
        int Next(uint celt, [MarshalAs(UnmanagedType.Interface)] out IExplorerCommand pUICommand, out uint pceltFetched);
        [PreserveSig]
        int Skip(uint celt);
        [PreserveSig]
        int Reset();
        [PreserveSig]
        int Clone(out IEnumExplorerCommand ppEnum);
    }

    class Program
    {
        static void Main()
        {
            try
            {
                Guid clsidParent = new Guid("65863AF6-B22D-4C52-BB0F-610D11268E04");
                Type tParent = Type.GetTypeFromCLSID(clsidParent);
                object objParent = Activator.CreateInstance(tParent);
                IExplorerCommand cmdParent = (IExplorerCommand)objParent;

                string titleParent;
                cmdParent.GetTitle(IntPtr.Zero, out titleParent);
                int flags;
                cmdParent.GetFlags(out flags);
                Console.WriteLine("Parent Title: " + titleParent + " (HasSubcommands: " + ((flags & 1) != 0) + ")");

                IEnumExplorerCommand pEnum;
                int hrEnum = cmdParent.EnumSubCommands(out pEnum);
                Console.WriteLine("EnumSubCommands HR: " + hrEnum);

                List<string> subTitles = new List<string>();
                if (pEnum != null)
                {
                    while (true)
                    {
                        IExplorerCommand subCmd;
                        uint fetched;
                        int hrNext = pEnum.Next(1, out subCmd, out fetched);
                        if (fetched == 0 || subCmd == null) break;

                        string subTitle;
                        subCmd.GetTitle(IntPtr.Zero, out subTitle);
                        subTitles.Add(subTitle);
                        Console.WriteLine("  -> Subcommand: " + subTitle);
                    }
                }

                if (subTitles.Contains("Convert") && subTitles.Contains("Compress") && subTitles.Contains("Resize") && subTitles.Contains("Inspect details"))
                {
                    Console.WriteLine("[PASS] All 4 subcommands (Convert, Compress, Resize, Inspect details) properly enumerated via COM IExplorerCommand!");
                }
                else
                {
                    throw new Exception("Missing one or more subcommands: " + string.Join(", ", subTitles.ToArray()));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex);
                Environment.Exit(1);
            }
        }
    }
}
