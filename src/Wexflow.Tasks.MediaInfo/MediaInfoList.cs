using System;
using System.Runtime.InteropServices;

namespace Wexflow.Tasks.MediaInfo
{
    public class MediaInfoList
    {
        private readonly IntPtr Handle;

        //MediaInfo class
        public MediaInfoList()
        {
            Handle = MediaInfoList_New();
        }

        //Import of DLL functions. DO NOT USE until you know what you do (MediaInfo DLL do NOT use CoTaskMemAlloc to allocate memory)
        [DllImport("MediaInfo.dll")]
        private static extern IntPtr MediaInfoList_New();

        [DllImport("MediaInfo.dll")]
        private static extern void MediaInfoList_Delete(IntPtr Handle);

        [DllImport("MediaInfo.dll")]
        private static extern IntPtr MediaInfoList_Open(IntPtr Handle,
            [MarshalAs(UnmanagedType.LPWStr)] string FileName, IntPtr Options);

        [DllImport("MediaInfo.dll")]
        private static extern void MediaInfoList_Close(IntPtr Handle, IntPtr FilePos);

        [DllImport("MediaInfo.dll")]
        private static extern IntPtr MediaInfoList_Inform(IntPtr Handle, IntPtr FilePos, IntPtr Reserved);

        [DllImport("MediaInfo.dll")]
        private static extern IntPtr MediaInfoList_GetI(IntPtr Handle, IntPtr FilePos, IntPtr StreamKind,
            IntPtr StreamNumber, IntPtr Parameter, IntPtr KindOfInfo);

        [DllImport("MediaInfo.dll")]
        private static extern IntPtr MediaInfoList_Get(IntPtr Handle, IntPtr FilePos, IntPtr StreamKind,
            IntPtr StreamNumber, [MarshalAs(UnmanagedType.LPWStr)] string Parameter, IntPtr KindOfInfo,
            IntPtr KindOfSearch);

        [DllImport("MediaInfo.dll")]
        private static extern IntPtr MediaInfoList_Option(IntPtr Handle,
            [MarshalAs(UnmanagedType.LPWStr)] string Option, [MarshalAs(UnmanagedType.LPWStr)] string Value);

        [DllImport("MediaInfo.dll")]
        private static extern IntPtr MediaInfoList_State_Get(IntPtr Handle);

        [DllImport("MediaInfo.dll")]
        private static extern IntPtr MediaInfoList_Count_Get(IntPtr Handle, IntPtr FilePos, IntPtr StreamKind,
            IntPtr StreamNumber);

        ~MediaInfoList()
        {
            MediaInfoList_Delete(Handle);
        }

        public int Open(string FileName, InfoFileOptions Options)
        {
            return (int) MediaInfoList_Open(Handle, FileName, (IntPtr) Options);
        }

        public void Close(int FilePos)
        {
            MediaInfoList_Close(Handle, (IntPtr) FilePos);
        }

        public string Inform(int FilePos)
        {
            return Marshal.PtrToStringUni(MediaInfoList_Inform(Handle, (IntPtr) FilePos, (IntPtr) 0));
        }

        public string Get(int FilePos, StreamKind StreamKind, int StreamNumber, string Parameter, InfoKind KindOfInfo,
            InfoKind KindOfSearch)
        {
            return Marshal.PtrToStringUni(MediaInfoList_Get(Handle, (IntPtr) FilePos, (IntPtr) StreamKind,
                (IntPtr) StreamNumber, Parameter, (IntPtr) KindOfInfo, (IntPtr) KindOfSearch));
        }

        public string Get(int FilePos, StreamKind StreamKind, int StreamNumber, int Parameter, InfoKind KindOfInfo)
        {
            return Marshal.PtrToStringUni(MediaInfoList_GetI(Handle, (IntPtr) FilePos, (IntPtr) StreamKind,
                (IntPtr) StreamNumber, (IntPtr) Parameter, (IntPtr) KindOfInfo));
        }

        public string Option(string Option, string Value)
        {
            return Marshal.PtrToStringUni(MediaInfoList_Option(Handle, Option, Value));
        }

        public int State_Get()
        {
            return (int) MediaInfoList_State_Get(Handle);
        }

        public int Count_Get(int FilePos, StreamKind StreamKind, int StreamNumber)
        {
            return (int) MediaInfoList_Count_Get(Handle, (IntPtr) FilePos, (IntPtr) StreamKind, (IntPtr) StreamNumber);
        }

        //Default values, if you know how to set default values in C#, say me
        public void Open(string FileName)
        {
            Open(FileName, 0);
        }

        public void Close()
        {
            Close(-1);
        }

        public string Get(int FilePos, StreamKind StreamKind, int StreamNumber, string Parameter, InfoKind KindOfInfo)
        {
            return Get(FilePos, StreamKind, StreamNumber, Parameter, KindOfInfo, InfoKind.Name);
        }

        public string Get(int FilePos, StreamKind StreamKind, int StreamNumber, string Parameter)
        {
            return Get(FilePos, StreamKind, StreamNumber, Parameter, InfoKind.Text, InfoKind.Name);
        }

        public string Get(int FilePos, StreamKind StreamKind, int StreamNumber, int Parameter)
        {
            return Get(FilePos, StreamKind, StreamNumber, Parameter, InfoKind.Text);
        }

        public string Option(string Option_)
        {
            return Option(Option_, "");
        }

        public int Count_Get(int FilePos, StreamKind StreamKind)
        {
            return Count_Get(FilePos, StreamKind, -1);
        }
    }
}