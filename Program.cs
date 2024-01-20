using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Nefarius.Utilities.DeviceManagement.PnP;

const string vidString = "VID_";
const string pidString = "PID_";
const string subtypeString = "SUB_";

const byte unknownSubtype = 0x00;
const byte turntableSubtype = 0x17;
const ushort wiredTurntableVid = 0x1430;
const ushort wiredTurntablePid = 0x1715;

AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

var devices = new Dictionary<string, int>();

for (int instance = 0;
    Devcon.FindByInterfaceGuid(DeviceInterfaceIds.HidDevice, out string path, out string instanceId, instance);
    instance++
)
{
    // Xbox 360 controllers have this in their device path
    if (!path.Contains("IG_"))
        continue;

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

    Console.WriteLine($"Vendor ID: 0x{vendorId:X4}");
    Console.WriteLine($"Product ID: 0x{productId:X4}");
    if (subType != unknownSubtype)
    {
        Console.WriteLine($"Subtype: 0x{subType:X2}");
        // Only calibrate turntables
        if (subType != turntableSubtype)
        {
            Console.WriteLine($"Device is not a turntable! Skipping.");
            Console.WriteLine();
            continue;
        }
        else
        {
            Console.WriteLine($"Device is a turntable.");
        }
    }
    else
    {
        Console.WriteLine("Couldn't determine subtype! Falling back to vendor/product IDs.");
        if (vendorId == wiredTurntableVid && productId == wiredTurntablePid)
        {
            Console.WriteLine("Matched with wired turntable vendor/product IDs.");
            Console.WriteLine("Device is a wired turntable.");
        }
        else
        {
            Console.WriteLine("Vendor/product IDs are unrecognized! Skipping device.");
            Console.WriteLine();
            continue;
        }
    }

    var idString = $"VID_{vendorId:X4}&PID_{productId:X4}";
    if (!devices.TryGetValue(idString, out int count))
        count = 0;
    devices[idString] = ++count;

    Console.WriteLine();
}

if (devices.Count < 1)
{
    Console.WriteLine("No Xbox 360 devices found!");
}
else
{
    Console.WriteLine($"Writing calibration files...");

    const string applyFilename = "360table_calibration_apply.reg";
    using (var file = File.CreateText(applyFilename))
    {
        file.WriteLine("Windows Registry Editor Version 5.00");

        foreach (var (idString, count) in devices)
        {
            for (int i = 0; i < count; i++)
            {
                file.WriteLine($"""

                    [HKEY_CURRENT_USER\System\CurrentControlSet\Control\MediaProperties\PrivateProperties\DirectInput\{idString}\Calibration\{i}\Type\Axes]

                    [HKEY_CURRENT_USER\System\CurrentControlSet\Control\MediaProperties\PrivateProperties\DirectInput\{idString}\Calibration\{i}\Type\Axes\0]
                    "Calibration"=hex:80,7f,00,00,ff,7f,00,00,7f,80,00,00

                    [HKEY_CURRENT_USER\System\CurrentControlSet\Control\MediaProperties\PrivateProperties\DirectInput\{idString}\Calibration\{i}\Type\Axes\1]
                    "Calibration"=hex:80,7f,00,00,ff,7f,00,00,7f,80,00,00

                    [HKEY_CURRENT_USER\System\CurrentControlSet\Control\MediaProperties\PrivateProperties\DirectInput\{idString}\Calibration\{i}\Type\Axes\2]
                    "Calibration"=-

                    [HKEY_CURRENT_USER\System\CurrentControlSet\Control\MediaProperties\PrivateProperties\DirectInput\{idString}\Calibration\{i}\Type\Axes\3]
                    "Calibration"=-

                    [HKEY_CURRENT_USER\System\CurrentControlSet\Control\MediaProperties\PrivateProperties\DirectInput\{idString}\Calibration\{i}\Type\Axes\4]
                    "Calibration"=-
                    """
                );
            }
        }

        file.Flush();
    }

    const string resetFilename = "360table_calibration_reset.reg";
    using (var file = File.CreateText(resetFilename))
    {
        file.WriteLine("Windows Registry Editor Version 5.00");

        foreach (var (idString, count) in devices)
        {
            for (int i = 0; i < count; i++)
            {
                file.WriteLine($"""

                    [HKEY_CURRENT_USER\System\CurrentControlSet\Control\MediaProperties\PrivateProperties\DirectInput\{idString}\Calibration\{i}\Type\Axes]

                    [HKEY_CURRENT_USER\System\CurrentControlSet\Control\MediaProperties\PrivateProperties\DirectInput\{idString}\Calibration\{i}\Type\Axes\0]
                    "Calibration"=-

                    [HKEY_CURRENT_USER\System\CurrentControlSet\Control\MediaProperties\PrivateProperties\DirectInput\{idString}\Calibration\{i}\Type\Axes\1]
                    "Calibration"=-

                    [HKEY_CURRENT_USER\System\CurrentControlSet\Control\MediaProperties\PrivateProperties\DirectInput\{idString}\Calibration\{i}\Type\Axes\2]
                    "Calibration"=-

                    [HKEY_CURRENT_USER\System\CurrentControlSet\Control\MediaProperties\PrivateProperties\DirectInput\{idString}\Calibration\{i}\Type\Axes\3]
                    "Calibration"=-

                    [HKEY_CURRENT_USER\System\CurrentControlSet\Control\MediaProperties\PrivateProperties\DirectInput\{idString}\Calibration\{i}\Type\Axes\4]
                    "Calibration"=-
                    """
                );
            }
        }

        file.Flush();
    }

    Console.WriteLine($"Wrote calibration files.");
    Console.WriteLine($"Double-click {applyFilename} to apply the custom calibration values.");
    Console.WriteLine($"Double-click {resetFilename} to reset the calibration to defaults.");
}

Console.WriteLine("Press Enter to exit...");
while (Console.ReadKey(intercept: true).Key != ConsoleKey.Enter) { }

static void OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
{
    Exception ex = args.ExceptionObject as Exception;
    Console.WriteLine("An unhandled exception has occured:");
    Console.WriteLine(ex);
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey(intercept: true);
}
