using System;
using System.Globalization;
using System.IO;
using Nefarius.Utilities.DeviceManagement.PnP;

namespace TurntableCalibrator
{
    class Program
    {
        const string vidString = "VID_";
        const string pidString = "PID_";
        const string subtypeString = "SUB_";

        static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            for (int instance = 0;
                Devcon.FindByInterfaceGuid(DeviceInterfaceIds.HidDevice, out string path, out string instanceId, instance);
                instance++
            )
            {
                // Xbox 360 controllers have this in their device path
                if (path.Contains("IG_"))
                {
                    Console.WriteLine($"Found Xbox 360 HID device: {path}");

                    // Retrieve VID/PID and subtype
                    var device = PnPDevice.GetDeviceByInstanceId(instanceId);
                    var ids = device.GetProperty<string[]>(DevicePropertyKey.Device_HardwareIds);
                    ushort vendorId = 0;
                    ushort productId = 0;
                    byte subType = 0;
                    foreach (string id in ids)
                    {
                        int index = id.IndexOf(vidString);
                        if (index > -1)
                        {
                            var vid = id.Substring(index + vidString.Length, 4);
                            vendorId = ushort.Parse(vid, NumberStyles.HexNumber);
                        }

                        index = id.IndexOf(pidString);
                        if (index > -1)
                        {
                            var pid = id.Substring(index + pidString.Length, 4);
                            productId = ushort.Parse(pid, NumberStyles.HexNumber);
                        }

                        index = id.IndexOf(subtypeString);
                        if (index > -1)
                        {
                            var sub = id.Substring(index + subtypeString.Length, 2);
                            subType = byte.Parse(sub, NumberStyles.HexNumber);
                        }
                    }

                    Console.WriteLine($"Vendor ID: 0x{vendorId:X4}, product ID: 0x{productId:X4}, subtype: 0x{subType:X2}");

                    // Only calibrate turntables
                    if (subType != 0x17)
                    {
                        Console.WriteLine($"Device is not a turntable! Skipping.");
                        continue;
                    }

                    Console.WriteLine($"Device is a turntable, writing calibration file...");

                    var idString = $"VID_{vendorId:X4}&PID_{productId:X4}";
                    var fileName = $"360table_calibration_{idString}.reg";
                    using (var file = File.CreateText(fileName))
                    {
                        file.WriteLine("Windows Registry Editor Version 5.00");
                        file.WriteLine();
                        file.WriteLine($@"[HKEY_CURRENT_USER\System\CurrentControlSet\Control\MediaProperties\PrivateProperties\DirectInput\{idString}\Calibration\0\Type\Axes]");
                        file.WriteLine();
                        file.WriteLine($@"[HKEY_CURRENT_USER\System\CurrentControlSet\Control\MediaProperties\PrivateProperties\DirectInput\{idString}\Calibration\0\Type\Axes\0]");
                        file.WriteLine("\"Calibration\"=hex:80,7f,00,00,ff,7f,00,00,7f,80,00,00");
                        file.WriteLine();
                        file.WriteLine($@"[HKEY_CURRENT_USER\System\CurrentControlSet\Control\MediaProperties\PrivateProperties\DirectInput\{idString}\Calibration\0\Type\Axes\1]");
                        file.WriteLine("\"Calibration\"=hex:80,7f,00,00,ff,7f,00,00,7f,80,00,00");
                        file.WriteLine();
                        file.WriteLine($@"[HKEY_CURRENT_USER\System\CurrentControlSet\Control\MediaProperties\PrivateProperties\DirectInput\{idString}\Calibration\0\Type\Axes\2]");
                        file.WriteLine("\"Calibration\"=-");
                        file.WriteLine();
                        file.WriteLine($@"[HKEY_CURRENT_USER\System\CurrentControlSet\Control\MediaProperties\PrivateProperties\DirectInput\{idString}\Calibration\0\Type\Axes\3]");
                        file.WriteLine("\"Calibration\"=-");
                        file.WriteLine();
                        file.WriteLine($@"[HKEY_CURRENT_USER\System\CurrentControlSet\Control\MediaProperties\PrivateProperties\DirectInput\{idString}\Calibration\0\Type\Axes\4]");
                        file.WriteLine("\"Calibration\"=-");
                        file.Flush();
                        Console.WriteLine($"Wrote calibration data to {fileName}");
                    }

                    fileName = $"360table_calibration_reset_{idString}.reg";
                    using (var file = File.CreateText(fileName))
                    {
                        file.WriteLine("Windows Registry Editor Version 5.00");
                        file.WriteLine();
                        file.WriteLine($@"[HKEY_CURRENT_USER\System\CurrentControlSet\Control\MediaProperties\PrivateProperties\DirectInput\{idString}\Calibration\0\Type\Axes]");
                        file.WriteLine();
                        file.WriteLine($@"[HKEY_CURRENT_USER\System\CurrentControlSet\Control\MediaProperties\PrivateProperties\DirectInput\{idString}\Calibration\0\Type\Axes\0]");
                        file.WriteLine("\"Calibration\"=-");
                        file.WriteLine();
                        file.WriteLine($@"[HKEY_CURRENT_USER\System\CurrentControlSet\Control\MediaProperties\PrivateProperties\DirectInput\{idString}\Calibration\0\Type\Axes\1]");
                        file.WriteLine("\"Calibration\"=-");
                        file.WriteLine();
                        file.WriteLine($@"[HKEY_CURRENT_USER\System\CurrentControlSet\Control\MediaProperties\PrivateProperties\DirectInput\{idString}\Calibration\0\Type\Axes\2]");
                        file.WriteLine("\"Calibration\"=-");
                        file.WriteLine();
                        file.WriteLine($@"[HKEY_CURRENT_USER\System\CurrentControlSet\Control\MediaProperties\PrivateProperties\DirectInput\{idString}\Calibration\0\Type\Axes\3]");
                        file.WriteLine("\"Calibration\"=-");
                        file.WriteLine();
                        file.WriteLine($@"[HKEY_CURRENT_USER\System\CurrentControlSet\Control\MediaProperties\PrivateProperties\DirectInput\{idString}\Calibration\0\Type\Axes\4]");
                        file.WriteLine("\"Calibration\"=-");
                        file.Flush();
                        Console.WriteLine($"Wrote calibration reset data to {fileName}");
                    }
                }
            }

            Console.WriteLine("Press Enter to exit...");
            while (Console.ReadKey(intercept: true).Key != ConsoleKey.Enter) { }
        }

        static void OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            Exception ex = args.ExceptionObject as Exception;
            Console.WriteLine("An unhandled exception has occured:");
            Console.WriteLine(ex);
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(intercept: true);
        }
    }
}
