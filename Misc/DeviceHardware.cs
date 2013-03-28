using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;

#if ANDROID
using Android.OS;
using Android.Util;
#elif IOS
using MonoTouch.Foundation;
using MonoTouch.UIKit;
#elif WINDOWS
using System.Management;
#endif

#if TOUCH
using Trace = System.Console;
#endif

namespace Phantom
{
	public class DeviceHardware
	{
		public const string HardwareProperty = "hw.machine";

		public enum DeviceOS {
			Android,
			BlackBerry,
			BSD,
			Flash,
			GameStick,
			Gaikai,
			iOS,
			Linux,
			MacOSX,
			OnLive,
			Ouya,
			PlayStation3,
			PlayStation4,
			PSMobile,
			Shield,
			SilverLight,
			SteamBox,
			Tizen,
			Wii,
			WiiU,
			Windows,
			Windows8,
			WindowsPhone,
			WindowsPhone8,
			WindowsRT,
			XBox360,
			XBoxNG,
			Unknown
		}
		public enum DeviceForm {
			Computer,
            Desktop,
            Laptop,
			Console,
			Phone,
			Pod,
			Tablet,
			Web,
			Streaming,
			Unknown
		}

#if IOS
		[DllImport(MonoTouch.Constants.SystemLibrary)]
		static extern int sysctlbyname([MarshalAs(UnmanagedType.LPStr)] string property, IntPtr output, IntPtr oldLen, IntPtr newp, uint newlen);
#elif WINDOWS
        public enum ChassisTypes
        {
            Other = 1,
            Unknown,
            Desktop,
            LowProfileDesktop,
            PizzaBox,
            MiniTower,
            Tower,
            Portable,
            Laptop,
            Notebook,
            Handheld,
            DockingStation,
            AllInOne,
            SubNotebook,
            SpaceSaving,
            LunchBox,
            MainSystemChassis,
            ExpansionChassis,
            SubChassis,
            BusExpansionChassis,
            PeripheralChassis,
            StorageChassis,
            RackMountChassis,
            SealedCasePC
        }
        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int GetDeviceCaps(IntPtr hDC, int nIndex);
        public enum DeviceCap
        {
          LOGPIXELSX = 88,
          LOGPIXELSY = 90
        }      
#endif

		private static DeviceOS deviceOS;
		private static string deviceOSVersion;
		private static DeviceForm deviceForm;
		private static string deviceManufacturer;
		private static string deviceIdentifier;
		private static string deviceModel;
		private static string deviceModelVersion;
		private static float devicePPcm;
		private static float devicePPI; // ye olde ways
		private static int deviceScreenWidth;
		private static int deviceScreenHeight;
		private static float deviceDisplayRealWidth;
		private static float deviceDisplayRealHeight;
		private static float deviceDisplayDiagonal;

		public static DeviceOS OS {
			get {
				if(!deviceInfoInitialized) RetrieveInfo();
				return deviceOS;
			}
			private set { deviceOS = value; }
		}
		public static string OSVersion {
			get {
				if(!deviceInfoInitialized) RetrieveInfo();
				return deviceOSVersion;
			}
			private set { deviceOSVersion = value; }
		}
		public static DeviceForm Form {
			get {
				if(!deviceInfoInitialized) RetrieveInfo();
				return deviceForm;
			}
			private set { deviceForm = value; }
		}
		public static string Manufacturer {
			get {
				if(!deviceInfoInitialized) RetrieveInfo();
				return deviceManufacturer;
			}
			private set { deviceManufacturer = value; }
		}
		public static string Identifier {
			get {
				if(!deviceInfoInitialized) RetrieveInfo();
				return deviceIdentifier;
			}
			private set { deviceIdentifier = value; }
		}
		public static string Model {
			get {
				if(!deviceInfoInitialized) RetrieveInfo();
				return deviceModel;
			}
			private set { deviceModel = value; }
		}
		public static string ModelVersion {
			get {
				if(!deviceInfoInitialized) RetrieveInfo();
				return deviceModelVersion;
			}
			private set { deviceModelVersion = value; }
		}
		public static float PPcm {
			get {
				if(!deviceInfoInitialized) RetrieveInfo();
				return devicePPcm;
			}
			private set { devicePPcm = value; }
		}
		public static float PPI {
			get {
				if(!deviceInfoInitialized) RetrieveInfo();
				return devicePPI;
			}
			private set { devicePPI = value; }
		}
		public static int ScreenWidth {
			get {
				if(!deviceInfoInitialized) RetrieveInfo();
				return deviceScreenWidth;
			}
			private set { deviceScreenWidth = value; }
		}
		public static int ScreenHeight {
			get {
				if(!deviceInfoInitialized) RetrieveInfo();
				return deviceScreenHeight;
			}
			private set { deviceScreenHeight = value; }
		}
		public static float DisplayRealWidth {
			get {
				if(!deviceInfoInitialized) RetrieveInfo();
				return deviceDisplayRealWidth;
			}
			private set { deviceDisplayRealWidth = value; }
		}
		public static float DisplayRealHeight {
			get {
				if(!deviceInfoInitialized) RetrieveInfo();
				return deviceDisplayRealHeight;
			}
			private set { deviceDisplayRealHeight = value; }
		}
		public static float DisplayDiagonal {
			get {
				if(!deviceInfoInitialized) RetrieveInfo();
				return deviceDisplayDiagonal;
			}
			private set { deviceDisplayDiagonal = value; }
		}

		private static bool deviceInfoInitialized = false;

		public static void RetrieveInfo()
		{
#if ANDROID
			DisplayMetrics metrics = new DisplayMetrics();
			Game.Activity.WindowManager.DefaultDisplay.GetMetrics(metrics);
			ScreenWidth = metrics.WidthPixels;
			ScreenHeight = metrics.HeightPixels;
			DisplayRealWidth = 2.54f * deviceScreenWidth / metrics.Xdpi;
			DisplayRealHeight = 2.54f * deviceScreenHeight / metrics.Ydpi;
			int shortSizeDp = (Math.Min(deviceScreenWidth, deviceScreenHeight) * (int)(DisplayMetricsDensity.Default)) / (int)metrics.DensityDpi;
			
			OS = DeviceOS.Android;
			OSVersion = Build.VERSION.Release;
			Manufacturer = Build.Manufacturer;
			Identifier = Build.Model;
			Model = Build.Device;
			Form = shortSizeDp < 720 ? DeviceForm.Phone : DeviceForm.Tablet; // Taken from Android SystemUI (status bar) layout policy for deciding when to use Tablet UI
			ModelVersion = Build.VERSION.Incremental;
			PPI = (metrics.Xdpi + metrics.Ydpi) / 2f;

#elif BLACKBERRY
			OS = DeviceOS.BlackBerry;
			OSVersion = "Unknown";
			Manufacturer = "BlackBerry";
			Identifier = "Unspecified";
			Model = "UnknownBlackBerryDevice";
			Form = DeviceForm.Unknown;
			ModelVersion = "UnknownBlackBerryVersion";
			PPI = 96f;

#elif BSD
			OS = DeviceOS.BSD;
			OSVersion = "Unknown";
			Manufacturer = "Unknown";
			Identifier = "Unspecified";
			Model = "UnknownBSDDevice";
			Form = DeviceForm.Computer;
			ModelVersion = "UnknownBSDVersion";
			PPI = 96f;

#elif FLASH
			OS = DeviceOS.Flash;
			OSVersion = "Unknown";
			Manufacturer = "Adobe";
			Identifier = "Unspecified";
			Model = "UnknownFlashPlayer";
			Form = DeviceForm.Web;
			ModelVersion = "UnknownFlashVersion";
			PPI = 96f;

#elif GAMESTICK
			OS = DeviceOS.GameStick;
			OSVersion = "Unknown";
			Manufacturer = "Playjam";
			Identifier = "Unspecified";
			Model = "UnknownGameStickDevice";
			Form = DeviceForm.Console;
			ModelVersion = "UnknownGameStickVersion";
			PPI = 96f;

#elif GAIKAI
			OS = DeviceOS.Gaikai;
			OSVersion = "Unknown";
			Manufacturer = "Sony";
			Identifier = "Unspecified";
			Model = "UnknownGaikaiDevice";
			Form = DeviceForm.Streaming;
			ModelVersion = "UnknownGaikaiVersion";
			PPI = 96f;

#elif IOS
			string hardwareStr = "Unspecified";
			IntPtr pLen = Marshal.AllocHGlobal(sizeof(int));
			sysctlbyname(DeviceHardware.HardwareProperty, IntPtr.Zero, pLen, IntPtr.Zero, 0);
			
			int length = Marshal.ReadInt32(pLen);
			
			if (length == 0)
			{
				Marshal.FreeHGlobal(pLen);
			}
			else {
				IntPtr pStr = Marshal.AllocHGlobal(length);
				sysctlbyname(DeviceHardware.HardwareProperty, pStr, pLen, IntPtr.Zero, 0);
				
				hardwareStr = Marshal.PtrToStringAnsi(pStr);
				
				Marshal.FreeHGlobal(pLen);
				Marshal.FreeHGlobal(pStr);
			}
			
			string hardwareStrLC = hardwareStr.ToLower();

			float scale = 1f;
			try
			{
				scale = UIScreen.MainScreen.Scale;
			}
			catch(Exception)
			{
				scale = 1f;
			}


			OS = DeviceOS.iOS;
			OSVersion = UIDevice.CurrentDevice.SystemVersion;
			Manufacturer = "Apple";
			Identifier = hardwareStr;

			if (hardwareStrLC.StartsWith("iphone")) {
				Model = "iPhone";
				Form = DeviceForm.Phone;
			}
			else if (hardwareStrLC == "ipad2,5" || hardwareStrLC == "ipad2,6" || hardwareStrLC == "ipad2,7") {
				Model = "iPad Mini";
				Form = DeviceForm.Tablet;
			}
			else if (hardwareStrLC.StartsWith("ipad")) {
				Model = "iPad";
				Form = DeviceForm.Tablet;
			}
			else if (hardwareStrLC.StartsWith("ipod")) {
				Model = "iPod Touch";
				Form = DeviceForm.Pod;
			}
			else if (hardwareStrLC.StartsWith("appletv")) {
				Model = "Apple TV";
				Form = DeviceForm.Console;
			}
			else if (hardwareStrLC == "i386" || hardwareStrLC == "x86_64") {
				if (UIDevice.CurrentDevice.Model.ToLower().Contains("iphone")) {
					Model = "iPhoneSimulator";
					Form = DeviceForm.Phone;
				}
				else {
					Model = "iPadSimulator";
				Form = DeviceForm.Tablet;
				}
			}
			else {
				Model = "UnknowniOSDevice";
				Form = DeviceForm.Unknown;
			}

			if (hardwareStrLC == "iphone1,1") ModelVersion = "2G";
			else if (hardwareStrLC == "iphone1,2") ModelVersion = "3G";
			else if (hardwareStrLC == "iphone2,1") ModelVersion = "3GS";
			else if (hardwareStrLC == "iphone3,1") ModelVersion = "4";
			else if (hardwareStrLC == "iphone3,2") ModelVersion = "4 Rev.A";
			else if (hardwareStrLC == "iphone3,3") ModelVersion = "4 CDMA";
			else if (hardwareStrLC == "iphone4,1") ModelVersion = "4S";
			else if (hardwareStrLC == "iphone5,1") ModelVersion = "5 GSM";
			else if (hardwareStrLC == "iphone5,2") ModelVersion = "5 CDMA+GSM";

			else if (hardwareStrLC == "ipad1,1") ModelVersion = "1 WiFi";
			else if (hardwareStrLC == "ipad1,2") ModelVersion = "1 3G";
			else if (hardwareStrLC == "ipad2,1") ModelVersion = "2 WiFi";
			else if (hardwareStrLC == "ipad2,2") ModelVersion = "2 GSM";
			else if (hardwareStrLC == "ipad2,3") ModelVersion = "2 CDMA";
			else if (hardwareStrLC == "ipad2,4") ModelVersion = "2 WiFi Rev.A";
			else if (hardwareStrLC == "ipad2,5") ModelVersion = "1 WiFi";
			else if (hardwareStrLC == "ipad2,6") ModelVersion = "1 GSM";
			else if (hardwareStrLC == "ipad2,7") ModelVersion = "1 CDMA+GSM";
			else if (hardwareStrLC == "ipad3,1") ModelVersion = "3 WiFi";
			else if (hardwareStrLC == "ipad3,2") ModelVersion = "3 CDMA";
			else if (hardwareStrLC == "ipad3,3") ModelVersion = "3 GSM";
			else if (hardwareStrLC == "ipad3,4") ModelVersion = "4 WiFi";
			else if (hardwareStrLC == "ipad3,5") ModelVersion = "4 GSM";
			else if (hardwareStrLC == "ipad3,6") ModelVersion = "4 CDMA+GSM";

			else if (hardwareStrLC == "ipod1,1") ModelVersion = "1G";
			else if (hardwareStrLC == "ipod2,1") ModelVersion = "2G";
			else if (hardwareStrLC == "ipod3,1") ModelVersion = "3G";
			else if (hardwareStrLC == "ipod4,1") ModelVersion = "4G";
			else if (hardwareStrLC == "ipod5,1") ModelVersion = "5G";

			else if (hardwareStrLC == "appletv2,1") ModelVersion = "2G";
			else if (hardwareStrLC == "appletv3,1") ModelVersion = "3G";
			else if (hardwareStrLC == "appletv3,2") ModelVersion = "3G Rev.A";

			else if (hardwareStrLC == "i386" || hardwareStr=="x86_64")
			{
				if (UIDevice.CurrentDevice.Model.ToLower().Contains("iphone"))
				{
					if (scale > 1.5f)
						ModelVersion = "iPhoneRetinaSimulator";
					else
						ModelVersion = "iPhoneSimulator";
				}
				else
				{
					if (scale > 1.5f)
						ModelVersion = "iPadRetinaSimulator";
					else
						ModelVersion = "iPadSimulator";
				}
			}
			else {
				ModelVersion = "UnknowniOSVersion";
			}

			if (deviceForm == DeviceForm.Phone || deviceForm == DeviceForm.Pod || deviceModel == "iPad Mini")
				PPI = 163f * scale;
			else if (deviceForm == DeviceForm.Tablet)
				PPI = 132f * scale;
			else if (deviceForm == DeviceForm.Console)
				PPI = 72f;
			else
				PPI = 160f * scale;

			ScreenWidth = (int)UIScreen.MainScreen.CurrentMode.Size.Width;
			ScreenHeight = (int)UIScreen.MainScreen.CurrentMode.Size.Height;

#elif LINUX
			OS = DeviceOS.Linux;
			OSVersion = "Unknown";
			Manufacturer = "Unknown";
			Identifier = "Unspecified";
			Model = "UnknownLinuxDevice";
			Form = DeviceForm.Computer;
			ModelVersion = "UnknownLinuxVersion";
			PPI = 96f;

#elif MACOS
			OS = DeviceOS.MacOSX;
			OSVersion = "Unknown";
			Manufacturer = "Apple";
			Identifier = "Unspecified";
			Model = "UnknownMacOSXDevice";
			Form = DeviceForm.Computer;
			ModelVersion = "UnknownMacOSXVersion";
			PPI = 96f;

#elif ONLIVE
			OS = DeviceOS.OnLive;
			OSVersion = "Unknown";
			Manufacturer = "OnLive";
			Identifier = "Unspecified";
			Model = "UnknownOnLiveDevice";
			Form = DeviceForm.Streaming;
			ModelVersion = "UnknownOnLiveVersion";
			PPI = 96f;

#elif OUYA
			OS = DeviceOS.Ouya;
			OSVersion = "Unknown";
			Manufacturer = "OUYA";
			Identifier = "Unspecified";
			Model = "UnknownOuyaDevice";
			Form = DeviceForm.Console;
			ModelVersion = "UnknownOuyaVersion";
			PPI = 96f;

#elif PS3
			OS = DeviceOS.PlayStation3;
			OSVersion = "Unknown";
			Manufacturer = "Sony";
			Identifier = "Unspecified";
			Model = "UnknownPlayStation3Device";
			Form = DeviceForm.Console;
			ModelVersion = "UnknownPlayStation3Version";
			PPI = 96f;

#elif PS4
			OS = DeviceOS.PlayStation4;
			OSVersion = "Unknown";
			Manufacturer = "Sony";
			Identifier = "Unspecified";
			Model = "UnknownPlayStation4Device";
			Form = DeviceForm.Console;
			ModelVersion = "UnknownPlayStation4Version";
			PPI = 96f;

#elif PSMOBILE
			OS = DeviceOS.PSMobile;
			OSVersion = "Unknown";
			Manufacturer = "Unknown";
			Identifier = "Unspecified";
			Model = "UnknownPSMobileDevice";
			Form = DeviceForm.Console;
			ModelVersion = "UnknownPSMobileVersion";
			PPI = 96f;

#elif SHIELD
			OS = DeviceOS.Shield;
			OSVersion = "Unknown";
			Manufacturer = "NVidia";
			Identifier = "Unspecified";
			Model = "UnknownShieldDevice";
			Form = DeviceForm.Console;
			ModelVersion = "UnknownShieldVersion";
			PPI = 96f;

#elif SILVERLIGHT
			OS = DeviceOS.SilverLight;
			OSVersion = "Unknown";
			Manufacturer = "Microsoft";
			Identifier = "Unspecified";
			Model = "UnknownSilverLightPlayer";
			Form = DeviceForm.Web;
			ModelVersion = "UnknownSilverLightVersion";
			PPI = 96f;

#elif STEAMBOX
			OS = DeviceOS.SteamBox;
			OSVersion = "Unknown";
			Manufacturer = "Valve";
			Identifier = "Unspecified";
			Model = "UnknownSteamBoxDevice";
			Form = DeviceForm.Console;
			ModelVersion = "UnknownSteamBoxVersion";
			PPI = 96f;

#elif TIZEN
			OS = DeviceOS.Tizen;
			OSVersion = "Unknown";
			Manufacturer = "Unknown";
			Identifier = "Unspecified";
			Model = "UnknownTizenDevice";
			Form = DeviceForm.Unknown;
			ModelVersion = "UnknownTizenVersion";
			PPI = 96f;

#elif WII
			OS = DeviceOS.Wii;
			OSVersion = "Unknown";
			Manufacturer = "Nintendo";
			Identifier = "Unspecified";
			Model = "UnknownWiiDevice";
			Form = DeviceForm.Console;
			ModelVersion = "UnknownWiiVersion";
			PPI = 96f;
#elif WIIU
			OS = DeviceOS.WiiU;
			OSVersion = "Unknown";
			Manufacturer = "Nintendo";
			Identifier = "Unspecified";
			Model = "UnknownWiiUDevice";
			Form = DeviceForm.Console;
			ModelVersion = "UnknownWiiUVersion";
			PPI = 96f;

#elif WINDOWS8
			OS = DeviceOS.Windows8;
			OSVersion = "Unknown";
			Manufacturer = "Unknown";
			Identifier = "Unspecified";
			Model = "UnknownWindows8Device";
			Form = DeviceForm.Computer;
			ModelVersion = "UnknownWindows8Version";
			PPI = 96f;

#elif WINDOWS_RT
			OS = DeviceOS.WindowsRT;
			OSVersion = "Unknown";
			Manufacturer = "Unknown";
			Identifier = "Unspecified";
			Model = "UnknownWindowsRTDevice";
			Form = DeviceForm.Tablet;
			ModelVersion = "UnknownWindowsRTVersion";
			PPI = 96f;

#elif WINDOWS
            string arch = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
//            int arch = ((String.IsNullOrEmpty(pa) || String.Compare(pa, 0, "x86", 0, 3, true) == 0) ? 32 : 64);

            OperatingSystem os = Environment.OSVersion;
            Version vs = os.Version;

            string operatingSystem = "";

            if (os.Platform == PlatformID.Win32Windows)
            {
                switch (vs.Minor)
                {
                    case 0:
                        operatingSystem = "95";
                        break;
                    case 10:
                        if (vs.Revision.ToString() == "2222A")
                            operatingSystem = "98SE";
                        else
                            operatingSystem = "98";
                        break;
                    case 90:
                        operatingSystem = "Me";
                        break;
                    default:
                        break;
                }
            }
            else if (os.Platform == PlatformID.Win32NT)
            {
                switch (vs.Major)
                {
                    case 3:
                        operatingSystem = "NT 3.51";
                        break;
                    case 4:
                        operatingSystem = "NT 4.0";
                        break;
                    case 5:
                        if (vs.Minor == 0)
                            operatingSystem = "2000";
                        else
                            operatingSystem = "XP";
                        break;
                    case 6:
                        if (vs.Minor == 0)
                            operatingSystem = "Vista";
                        else if (vs.Minor == 1)
                            operatingSystem = "7";
                        else if (vs.Minor == 2)
                            operatingSystem = "8";
                        break;
                    default:
                        operatingSystem = "Unknown Version";
                        break;
                }
            }

            ChassisTypes chassis = ChassisTypes.Unknown;
            ManagementClass mc;
            try
            {
                mc = new ManagementClass("Win32_SystemEnclosure");
                foreach (ManagementObject obj in mc.GetInstances())
                {
                    foreach (int i in (UInt16[])(obj["ChassisTypes"]))
                    {
                        if (i > 0 && i < 25)
                        {
                            chassis = (ChassisTypes)i;
                        }
                    }
                }
            }
            catch { }

			Manufacturer = "Unknown";
            Model = "UnknownModel";
            ModelVersion = "UnknownModelVersion";

            try
            {
                mc = new ManagementClass("Win32_ComputerSystem");
                foreach (ManagementObject mo in mc.GetInstances())
                {
                    object val = mo.GetPropertyValue("Manufacturer");
                    if (val != null) Manufacturer = val.ToString();
                    val = mo.GetPropertyValue("Model");
                    if (val != null)
                    {
                        Model = val.ToString();
                        int space = deviceModel.LastIndexOf(' ');
                        if (space > 0)
                        {
                            ModelVersion = deviceModel.Substring(space + 1);
                            Model = deviceModel.Substring(0, space);
                        }
                        else
                            ModelVersion = deviceModel;
                    }
                }
            }
            catch { }

			OS = DeviceOS.Windows;
            OSVersion = operatingSystem;
			Identifier = System.Environment.OSVersion.VersionString + " ("+arch+")";
            if (chassis == ChassisTypes.AllInOne || chassis == ChassisTypes.Desktop || chassis == ChassisTypes.LowProfileDesktop || chassis == ChassisTypes.LunchBox || chassis == ChassisTypes.MiniTower || chassis == ChassisTypes.PizzaBox || chassis == ChassisTypes.SealedCasePC || chassis == ChassisTypes.SpaceSaving || chassis == ChassisTypes.Tower)
                Form = DeviceForm.Desktop;
            else if (chassis == ChassisTypes.DockingStation || chassis == ChassisTypes.Laptop || chassis == ChassisTypes.Notebook || chassis == ChassisTypes.Portable || chassis == ChassisTypes.SubNotebook)
                Form = DeviceForm.Laptop;
            else if (chassis == ChassisTypes.Handheld)
                Form = DeviceForm.Tablet;
            else
                Form = DeviceForm.Computer;

            System.Drawing.Graphics g = System.Drawing.Graphics.FromHwnd(IntPtr.Zero);
            IntPtr desktop = g.GetHdc();

            ScreenWidth = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
            ScreenHeight = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;

            int Xdpi = GetDeviceCaps(desktop, (int)DeviceCap.LOGPIXELSX);
            int Ydpi = GetDeviceCaps(desktop, (int)DeviceCap.LOGPIXELSY); 

			PPI = ((float)(Xdpi + Ydpi)) / 2f;

#elif WINDOWS_PHONE_8
			OS = DeviceOS.WindowsPhone8;
			OSVersion = "Unknown";
			Manufacturer = "Unknown";
			Identifier = "Unspecified";
			Model = "UnknownWindowsPhone8Device";
			Form = DeviceForm.Phone;
			ModelVersion = "UnknownWindowsPhone8Version";
			PPI = 96f;

#elif WINDOWS_PHONE
			OS = DeviceOS.WindowsPhone;
			OSVersion = "Unknown";
			Manufacturer = "Unknown";
			Identifier = "Unspecified";
			Model = "UnknownWindowsPhoneDevice";
			Form = DeviceForm.Phone;
			ModelVersion = "UnknownWindowsPhoneVersion";
			PPI = 96f;

#elif XBOX
			OS = DeviceOS.XBox360;
			OSVersion = "Unknown";
			Manufacturer = "Microsoft";
			Identifier = "Unspecified";
			Model = "UnknownXBox360Device";
			Form = DeviceForm.Console;
			ModelVersion = "UnknownXBox360Version";
			PPI = 96f;

#elif XBOXNG
			OS = DeviceOS.XBoxNG;
			OSVersion = "Unknown";
			Manufacturer = "Microsoft";
			Identifier = "Unspecified";
			Model = "UnknownXBoxNGDevice";
			Form = DeviceForm.Console;
			ModelVersion = "UnknownXBoxNGVersion";
			PPI = 96f;

#else
			OS = DeviceOS.Unknown;
			OSVersion = "Unknown";
			Manufacturer = "Unknown";
			Identifier = "Unspecified";
			Model = "UnknownDevice";
			Form = DeviceForm.Unknown;
			ModelVersion = "UnknownVersion";
			PPI = 96f;

#endif
			PPcm = devicePPI / 2.54f;

			if(deviceDisplayRealWidth == 0f)
			{
				DisplayRealWidth = deviceScreenWidth / devicePPcm;
				DisplayRealHeight = deviceScreenHeight / devicePPcm;
			}
			DisplayDiagonal = (float)Math.Sqrt(deviceDisplayRealWidth * deviceDisplayRealWidth + deviceDisplayRealHeight * deviceDisplayRealHeight);

			deviceInfoInitialized = true;
		}
	}
}
