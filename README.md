# Turntable Calibrator

A quick and dirty program to generate DirectInput calibration registry entries for Xbox 360 turntables.

## Why?

The Xbox 360 turntable has an insanely small range for the values it outputs for turntable spin speed, which causes difficulties with mapping controls in emulators and such. To fix this, a registry entry file is created for the turntable that applies calibrations to fix the small range, and DirectInput/MMJoystick is used as the input backend in the emulator instead of XInput.

However, since third-party Xbox 360 receivers change the product ID that turntables appear under, those receivers don't have the same product IDs, it's impossible to just provide a single set of registry entries to apply calibrations. This tool addresses that, and generates a calibration file for any connected Xbox 360 turntables.

## Usage

1. Download the application from the [Releases page](../../releases). No dependencies are required, they're bundled into the application.
2. Connect at least one turntable to your Xbox 360 receiver. Since all connected turntables will show up with the same VID/PID, they will all receive the calibration.
3. Run the application. It will generate two registry files: one to apply the calibrations, and one to remove them.
4. Double-click the `360table_calibration_VID_xxxx&PID_xxxx.reg` file to apply the calibrations.
5. Set up your controls in the emulator or game.
6. To remove the calibrations, double-click the `360table_calibration_reset_VID_xxxx&PID_xxxx.reg` file.

## A Note/Warning

Since wireless Xbox 360 controllers don't report a product ID, these calibrations will apply to all controllers that report the same vendor ID as the turntable. More specifically, since the turntable uses the same vendor ID as the Guitar Hero guitars and drumkits, these calibrations will apply to those controllers as well.

This doesn't particularly matter though, since on guitars and drumkits nothing vital is placed on the axes that get calibrated, and these calibrations only apply when an app is using DirectInput anyways. Regardless, the calibration reset file is provided for this reason.

## License

This application is licensed under the MIT license. See the [LICENSE](LICENSE) file for more details.
