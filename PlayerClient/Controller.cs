using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Untility;


namespace PlayerClient
{
    /// <summary>
    ///声音控制api 
    /// </summary>
    public static class Controller
    {
        [ComImport]
        [Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
        internal class MMDeviceEnumerator
        {
        }

        internal enum EDataFlow
        {
            eRender,
            eCapture,
            eAll,
            EDataFlow_enum_count
        }

        internal enum ERole
        {
            eConsole,
            eMultimedia,
            eCommunications,
            ERole_enum_count
        }

        [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IMMDeviceEnumerator
        {
            int NotImpl1();

            [PreserveSig]
            int GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, out IMMDevice ppDevice);

            // the rest is not implemented
        }

        [Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IMMDevice
        {
            [PreserveSig]
            int Activate(ref Guid iid, int dwClsCtx, IntPtr pActivationParams, [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);

            // the rest is not implemented
        }

        [Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IAudioSessionManager2
        {
            int NotImpl1();
            int NotImpl2();

            [PreserveSig]
            int GetSessionEnumerator(out IAudioSessionEnumerator SessionEnum);

            // the rest is not implemented
        }

        [Guid("E2F5BB11-0570-40CA-ACDD-3AA01277DEE8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IAudioSessionEnumerator
        {
            [PreserveSig]
            int GetCount(out int SessionCount);

            [PreserveSig]
            int GetSession(int SessionCount, out IAudioSessionControl Session);
        }

        [Guid("F4B1A599-7266-4319-A8CA-E70ACB11E8CD"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IAudioSessionControl
        {
            int NotImpl1();

            [PreserveSig]
            int GetDisplayName([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);

            // the rest is not implemented
        }

        [Guid("87CE5498-68D6-44E5-9215-6DA47EF883D8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface ISimpleAudioVolume
        {
            [PreserveSig]
            int SetMasterVolume(float fLevel, ref Guid EventContext);

            [PreserveSig]
            int GetMasterVolume(out float pfLevel);

            [PreserveSig]
            int SetMute(bool bMute, ref Guid EventContext);

            [PreserveSig]
            int GetMute(out bool pbMute);
        }

        /// <summary>
        /// 判断是否存在进程
        /// </summary>
        /// <param name="processName"></param>
        /// <returns></returns>
        public static bool IsExistProcessName(string processName)
        {
            try
            {
                //标识
                bool flag = false;
                // get the speakers (1st render + multimedia) device
                IMMDeviceEnumerator deviceEnumerator = (IMMDeviceEnumerator)(new MMDeviceEnumerator());
                IMMDevice speakers=null;
                if (deviceEnumerator!=null)
                {
                    deviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out speakers);                    
                }
               
                // activate the session manager. we need the enumerator
                Guid IID_IAudioSessionManager2 = typeof(IAudioSessionManager2).GUID;
                object o=null;

                if (speakers != null)
                {
                    speakers.Activate(ref IID_IAudioSessionManager2, 0, IntPtr.Zero, out o);
                }
               
                IAudioSessionManager2 mgr = (IAudioSessionManager2)o;
                // enumerate sessions for on this device
                IAudioSessionEnumerator sessionEnumerator=null;
                if (mgr!=null)
                {
                    mgr.GetSessionEnumerator(out sessionEnumerator);      
                }
                  
                int count=0;

                if (sessionEnumerator!=null)
                {
                    sessionEnumerator.GetCount(out count);
                }
               

                for (int i = 0; i < count; i++)
                {
                    IAudioSessionControl ctl;
                    IAudioSessionControl2 ctl2;

                    sessionEnumerator.GetSession(i, out ctl);
                    ctl2 = ctl as IAudioSessionControl2;

                    uint pid = 0;

                    string sout1 = "";
                    string sout2 = "";
                    int a = 0;


                    if (ctl2 != null)
                    {
                        ctl2.GetSessionIdentifier(out sout1);
                        ctl2.GetProcessId(out pid);
                        ctl2.GetSessionInstanceIdentifier(out sout2);
                    }


                    if (sout2.ToUpper().Contains(processName.ToUpper()))
                    {
                        ISimpleAudioVolume volumeControl = null;
                        volumeControl = ctl as ISimpleAudioVolume;
                        Guid guid = Guid.Empty;

                        AudioSessionState state;
                        ctl2.GetState(out state);
                        if (state.ToString() == "AudioSessionStateActive")
                        {
                            //volumeControl.GetMasterVolume(out Ad_Volume_level);
                            flag = true;

                        }
                    }

                    if (ctl != null)
                        Marshal.ReleaseComObject(ctl);

                    if (ctl2 != null)
                        Marshal.ReleaseComObject(ctl2);
                }

                if (sessionEnumerator != null)
                {
                    Marshal.ReleaseComObject(sessionEnumerator);
                }

                if (mgr != null)
                {
                    Marshal.ReleaseComObject(mgr);
                }

                if (speakers != null)
                {
                    Marshal.ReleaseComObject(speakers);
                }

                if (deviceEnumerator != null)
                {
                    Marshal.ReleaseComObject(deviceEnumerator);
                }
                return flag;
            }
            catch (Exception ex)
            {
                WriteLog.WriteErrorLogToFile(string.Format("日志定位Controller.cs-IsExistProcessName(string processName)判断是否存在进程错误-错误信息：{0} {1}", ex.Message.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")), true);
                throw ex;
            }
        }

        /// <summary>
        /// 设置音量
        /// </summary>
        /// <param name="processName"></param>
        /// <param name="volumeLevel"></param>
        public static void SetVolume(string processName, float volumeLevel)
        {
            try
            {
                // get the speakers (1st render + multimedia) device
                IMMDeviceEnumerator deviceEnumerator = (IMMDeviceEnumerator)(new MMDeviceEnumerator());
                IMMDevice speakers;
                deviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out speakers);

                // activate the session manager. we need the enumerator
                Guid IID_IAudioSessionManager2 = typeof(IAudioSessionManager2).GUID;
                object o;
                speakers.Activate(ref IID_IAudioSessionManager2, 0, IntPtr.Zero, out o);
                IAudioSessionManager2 mgr = (IAudioSessionManager2)o;

                // enumerate sessions for on this device
                IAudioSessionEnumerator sessionEnumerator;
                mgr.GetSessionEnumerator(out sessionEnumerator);
                int count;
                sessionEnumerator.GetCount(out count);

                for (int i = 0; i < count; i++)
                {
                    IAudioSessionControl ctl;
                    IAudioSessionControl2 ctl2;

                    sessionEnumerator.GetSession(i, out ctl);

                    ctl2 = ctl as IAudioSessionControl2;

                    string dn;

                    uint pid = 0;

                    string sout1 = "";
                    string sout2 = "";


                    if (ctl2 != null)
                    {
                        ctl2.GetSessionIdentifier(out sout1);
                        ctl2.GetProcessId(out pid);
                        ctl2.GetSessionInstanceIdentifier(out sout2);
                    }

                    if (sout2.ToUpper().Contains(processName.ToUpper()))
                    {
                        ISimpleAudioVolume volumeControl = null;
                        volumeControl = ctl as ISimpleAudioVolume;
                        Guid guid = Guid.Empty;
                        volumeControl.SetMasterVolume((float)volumeLevel, ref guid);
                    }

                    if (ctl != null)
                        Marshal.ReleaseComObject(ctl);

                    if (ctl2 != null)
                        Marshal.ReleaseComObject(ctl2);
                }

                if (sessionEnumerator != null)
                {
                    Marshal.ReleaseComObject(sessionEnumerator);
                }

                if (mgr != null)
                {
                    Marshal.ReleaseComObject(mgr);
                }

                if (speakers != null)
                {
                    Marshal.ReleaseComObject(speakers);
                }

                if (deviceEnumerator != null)
                {
                    Marshal.ReleaseComObject(deviceEnumerator);
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteErrorLogToFile(string.Format("日志定位Controller.cs-SetVolume(string processName, float volumeLevel)判断是否存在进程错误-错误信息：{0} {1}", ex.Message.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")), true);
                throw ex;
            }
        }

        /// <summary>
        /// 设置音量
        /// </summary>
        /// <param name="type"></param>
        /// <param name="processName"></param>
        /// <param name="volumeLevel"></param>
        public static void SetVolume(string type, string processName, float volumeLevel, ref float adVolumeLevel)
        {
            try
            {


                // get the speakers (1st render + multimedia) device
                IMMDeviceEnumerator deviceEnumerator = (IMMDeviceEnumerator)(new MMDeviceEnumerator());
                IMMDevice speakers;
                deviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out speakers);

                // activate the session manager. we need the enumerator
                Guid IID_IAudioSessionManager2 = typeof(IAudioSessionManager2).GUID;
                object o;
                speakers.Activate(ref IID_IAudioSessionManager2, 0, IntPtr.Zero, out o);
                IAudioSessionManager2 mgr = (IAudioSessionManager2)o;

                // enumerate sessions for on this device
                IAudioSessionEnumerator sessionEnumerator;
                mgr.GetSessionEnumerator(out sessionEnumerator);
                int count;
                sessionEnumerator.GetCount(out count);

                for (int i = 0; i < count; i++)
                {
                    IAudioSessionControl ctl;
                    IAudioSessionControl2 ctl2;

                    sessionEnumerator.GetSession(i, out ctl);

                    ctl2 = ctl as IAudioSessionControl2;

                    uint pid = 0;

                    string sout1 = "";
                    string sout2 = "";


                    if (ctl2 != null)
                    {
                        ctl2.GetSessionIdentifier(out sout1);
                        ctl2.GetProcessId(out pid);
                        ctl2.GetSessionInstanceIdentifier(out sout2);
                    }

                    if (type == "ad")
                    {
                        if (sout2.ToUpper().Contains(processName.ToUpper()))
                        {
                            ISimpleAudioVolume volumeControl = null;
                            volumeControl = ctl as ISimpleAudioVolume;
                            Guid guid = Guid.Empty;
                            volumeControl.GetMasterVolume(out adVolumeLevel);
                            volumeControl.SetMasterVolume((float)volumeLevel, ref guid);
                        }
                    }
                    else if (type == "voice")
                    {
                        if (sout2.ToUpper().Contains(processName.ToUpper()))
                        {
                            ISimpleAudioVolume volumeControl = null;
                            volumeControl = ctl as ISimpleAudioVolume;
                            Guid guid = Guid.Empty;
                            volumeControl.SetMasterVolume((float)volumeLevel, ref guid);
                        }
                    }


                    if (ctl != null)
                        Marshal.ReleaseComObject(ctl);

                    if (ctl2 != null)
                        Marshal.ReleaseComObject(ctl2);
                }

                if (sessionEnumerator != null)
                {
                    Marshal.ReleaseComObject(sessionEnumerator);
                }

                if (mgr != null)
                {
                    Marshal.ReleaseComObject(mgr);
                }

                if (speakers != null)
                {
                    Marshal.ReleaseComObject(speakers);
                }

                if (deviceEnumerator != null)
                {
                    Marshal.ReleaseComObject(deviceEnumerator);
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteErrorLogToFile(string.Format("日志定位Controller.cs-SetVolume(string type, string processName, float volumeLevel, ref float adVolumeLevel)判断是否存在进程错误-错误信息：{0} {1}", ex.Message.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")), true);
                throw ex;
            }
        }

        /// <summary>
        /// 获取正在使用音频的进程
        /// </summary>
        /// <param name="processName"></param>
        /// <param name="volumeLevel"></param>
        /// <returns></returns>
        public static IEnumerable<string> EnumerateApplications(string processName, float volumeLevel)
        {
            // get the speakers (1st render + multimedia) device
            IMMDeviceEnumerator deviceEnumerator = (IMMDeviceEnumerator)(new MMDeviceEnumerator());
            IMMDevice speakers;
            deviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out speakers);

            // activate the session manager. we need the enumerator
            Guid IID_IAudioSessionManager2 = typeof(IAudioSessionManager2).GUID;
            object o;
            speakers.Activate(ref IID_IAudioSessionManager2, 0, IntPtr.Zero, out o);
            IAudioSessionManager2 mgr = (IAudioSessionManager2)o;

            // enumerate sessions for on this device
            IAudioSessionEnumerator sessionEnumerator;
            mgr.GetSessionEnumerator(out sessionEnumerator);
            int count;
            sessionEnumerator.GetCount(out count);

            for (int i = 0; i < count; i++)
            {
                IAudioSessionControl ctl;
                IAudioSessionControl2 ctl2;

                sessionEnumerator.GetSession(i, out ctl);

                ctl2 = ctl as IAudioSessionControl2;

                string dn;

                uint pid = 0;

                string sout1 = "";
                string sout2 = "";


                if (ctl2 != null)
                {
                    ctl2.GetSessionIdentifier(out sout1);
                    ctl2.GetProcessId(out pid);
                    ctl2.GetSessionInstanceIdentifier(out sout2);
                }

                if (sout2.Contains(processName))
                {
                    ISimpleAudioVolume volumeControl = null;
                    volumeControl = ctl as ISimpleAudioVolume;
                    Guid guid = Guid.Empty;
                    volumeControl.SetMasterVolume((float)volumeLevel, ref guid);
                }

                //ctl.GetDisplayName(out dn);
                //  ctl2.GetProcessId(out pid);

                //yield return pid.ToString();
                yield return pid.ToString() + ": " + sout1 + " :: " + sout2;

                if (ctl != null)
                    Marshal.ReleaseComObject(ctl);

                if (ctl2 != null)
                    Marshal.ReleaseComObject(ctl2);
            }

            if (sessionEnumerator != null)
            {
                Marshal.ReleaseComObject(sessionEnumerator);
            }

            if (mgr != null)
            {
                Marshal.ReleaseComObject(mgr);
            }

            if (speakers != null)
            {
                Marshal.ReleaseComObject(speakers);
            }

            if (deviceEnumerator != null)
            {
                Marshal.ReleaseComObject(deviceEnumerator);
            }
        }

        public enum AudioSessionState
        {
            AudioSessionStateInactive = 0,
            AudioSessionStateActive = 1,
            AudioSessionStateExpired = 2
        }

        public enum AudioSessionDisconnectReason
        {
            DisconnectReasonDeviceRemoval = 0,
            DisconnectReasonServerShutdown = (DisconnectReasonDeviceRemoval + 1),
            DisconnectReasonFormatChanged = (DisconnectReasonServerShutdown + 1),
            DisconnectReasonSessionLogoff = (DisconnectReasonFormatChanged + 1),
            DisconnectReasonSessionDisconnected = (DisconnectReasonSessionLogoff + 1),
            DisconnectReasonExclusiveModeOverride = (DisconnectReasonSessionDisconnected + 1)
        }
        [Guid("24918ACC-64B3-37C1-8CA9-74A66E9957A8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IAudioSessionEvents
        {
            [PreserveSig]
            int OnDisplayNameChanged([MarshalAs(UnmanagedType.LPWStr)] string NewDisplayName, Guid EventContext);
            [PreserveSig]
            int OnIconPathChanged([MarshalAs(UnmanagedType.LPWStr)] string NewIconPath, Guid EventContext);
            [PreserveSig]
            int OnSimpleVolumeChanged(float NewVolume, bool newMute, Guid EventContext);
            [PreserveSig]
            int OnChannelVolumeChanged(UInt32 ChannelCount, IntPtr NewChannelVolumeArray, UInt32 ChangedChannel, Guid EventContext);
            [PreserveSig]
            int OnGroupingParamChanged(Guid NewGroupingParam, Guid EventContext);
            [PreserveSig]
            int OnStateChanged(AudioSessionState NewState);
            [PreserveSig]
            int OnSessionDisconnected(AudioSessionDisconnectReason DisconnectReason);
        }

        [Guid("BFB7FF88-7239-4FC9-8FA2-07C950BE9C6D"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IAudioSessionControl2
        {
            [PreserveSig]
            int GetState(out AudioSessionState state);
            [PreserveSig]
            int GetDisplayName([Out(), MarshalAs(UnmanagedType.LPWStr)] out string name);
            [PreserveSig]
            int SetDisplayName([MarshalAs(UnmanagedType.LPWStr)] string value, Guid EventContext);
            [PreserveSig]
            int GetIconPath([Out(), MarshalAs(UnmanagedType.LPWStr)] out string Path);
            [PreserveSig]
            int SetIconPath([MarshalAs(UnmanagedType.LPWStr)] string Value, Guid EventContext);
            [PreserveSig]
            int GetGroupingParam(out Guid GroupingParam);
            [PreserveSig]
            int SetGroupingParam(Guid Override, Guid Eventcontext);
            [PreserveSig]
            int RegisterAudioSessionNotification(IAudioSessionEvents NewNotifications);
            [PreserveSig]
            int UnregisterAudioSessionNotification(IAudioSessionEvents NewNotifications);
            [PreserveSig]
            int GetSessionIdentifier([Out(), MarshalAs(UnmanagedType.LPWStr)] out string retVal);
            [PreserveSig]
            int GetSessionInstanceIdentifier([Out(), MarshalAs(UnmanagedType.LPWStr)] out string retVal);
            [PreserveSig]
            int GetProcessId(out UInt32 retvVal);
            [PreserveSig]
            int IsSystemSoundsSession();
            [PreserveSig]
            int SetDuckingPreference(bool optOut);
        }
    }
}
