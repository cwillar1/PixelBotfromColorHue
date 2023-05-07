using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Diagnostics;
using WindowsInput;
using WindowsInput.Native;
using Pixelfromhue;
class Program
{

    [DllImport("user32.dll", SetLastError = true)]
    static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

    private const byte VK_CAPITAL = 0x14;
    private const byte VK_KEY_Z = 0x5A;
    private const UInt32 MOUSEEVENTF_LEFTDOWN = 0x0002;
    private const UInt32 MOUSEEVENTF_LEFTUP = 0x0004;
    public const byte KEYEVENTF_KEYUP = 0x02;
    public const byte VK_LMENU = 0xA4;

    
    static void Main(string[] args)
    {

        while (true)
        {
            if (GetAsyncKeyState(VK_CAPITAL) != 0)
            {
                //REMEMBER TO BE IN FULLSCREEN!

                savert();
            }
        }
    }



    public static void toggleOverlay()
    {
        keybd_event(VK_LMENU, 0, 0, 0);
        SendKeys.SendWait("{z}");
        keybd_event(VK_LMENU, 0, KEYEVENTF_KEYUP, 0);
    }
    public static HueStruct.CIEXYZ RGBtoXYZ(int red, int green, int blue)
    {
        // normalize red, green, blue values
        double rLinear = (double)red / 255.0;
        double gLinear = (double)green / 255.0;
        double bLinear = (double)blue / 255.0;

        // convert to a sRGB form
        double r = (rLinear > 0.04045) ? Math.Pow((rLinear + 0.055) / (
            1 + 0.055), 2.2) : (rLinear / 12.92);
        double g = (gLinear > 0.04045) ? Math.Pow((gLinear + 0.055) / (
            1 + 0.055), 2.2) : (gLinear / 12.92);
        double b = (bLinear > 0.04045) ? Math.Pow((bLinear + 0.055) / (
            1 + 0.055), 2.2) : (bLinear / 12.92);

        // converts
        return new HueStruct.CIEXYZ(
            (r * 0.4124 + g * 0.3576 + b * 0.1805),
            (r * 0.2126 + g * 0.7152 + b * 0.0722),
            (r * 0.0193 + g * 0.1192 + b * 0.9505)
            );
    }

    /// <summary>
    /// XYZ to L*a*b* transformation function.
    /// </summary>
    private static double Fxyz(double t)
    {
        return ((t > 0.008856) ? Math.Pow(t, (1.0 / 3.0)) : (7.787 * t + 16.0 / 116.0));
    }

    /// <summary>
    /// Converts CIEXYZ to CIELab.
    /// </summary>
    public static HueStruct.CIELab XYZtoLab(HueStruct.CIEXYZ xyzL)
    {
        double y = xyzL.y;
        double x = xyzL.x;
        double z = xyzL.z;

        HueStruct.CIELab lab = HueStruct.CIELab.Empty;

        lab.L = 116.0 * Fxyz(y / HueStruct.CIEXYZ.D65.Y) - 16;
        lab.A = 500.0 * (Fxyz(x / HueStruct.CIEXYZ.D65.X) - Fxyz(y / HueStruct.CIEXYZ.D65.Y));
        lab.B = 200.0 * (Fxyz(y / HueStruct.CIEXYZ.D65.Y) - Fxyz(z / HueStruct.CIEXYZ.D65.Z));

        return lab;
    }

    public static double deltaECalc(HueStruct.CIELab lab1, HueStruct.CIELab lab2)
    {
        //using CEI76 
        double totalL = lab2.L - lab1.L;
        double totala = lab2.a - lab1.a;
        double totalb = lab2.b - lab1.b;

        totalL = totalL * totalL;
        totala = totala * totala;
        totalb = totalb * totalb;

        double totalDinkert = totalL + totala + totalb;
        totalDinkert = Math.Sqrt(totalDinkert);

        return totalDinkert;
    }

    public static void LinearSmoothMove(int x, int y, int steps)
    {

        InputSimulator inputSim = new InputSimulator();
        Cursor.Position = new Point(0, 0);
        Point start = Cursor.Position;
        PointF iterPoint = start;

        inputSim.Mouse.MoveMouseTo(x, y + 5);
        System.Threading.Thread.Sleep(5);
        inputSim.Mouse.LeftButtonDown();
        inputSim.Mouse.LeftButtonUp();

    }

    public static void ParseScreen(Bitmap memoryImage, HueStruct.CIELab labL)
    {

        for (int y = 0; y < 540; y+=10)
        {
            for (int x = 0; x < 960; x+=3)
            {
                // Get the current pixels color
                Color currentPixelColor = memoryImage.GetPixel(x, y);

                //Color currentPixelColor = Color.FromArgb(214, 118, 72);
                HueStruct.CIEXYZ xyzK = RGBtoXYZ(currentPixelColor.R, currentPixelColor.G, currentPixelColor.B);
                HueStruct.CIELab labK = XYZtoLab(xyzK);

                //calculate delta E from both labs
                double deltaE = deltaECalc(labL, labK);

                // Finally compare the pixels hex color and the desired hex color (if they match we found a pixel)
                if (deltaE < 25)
                {
                    LinearSmoothMove(x+480, y+270, 2);
                    return;
                }
                if (y == 539)
                {
                    Console.WriteLine("No Color found");
                    return;
                }

            }
        }

    }
    [DllImport("user32.dll")]
    public static extern short GetAsyncKeyState(int vKey);

    [DllImport("user32.dll")]
    private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, uint dwExtraInf);

    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    static extern bool ScreenToClient(IntPtr hWnd, ref Point lpPoint);

    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr FindWindow(string lpClassName, string lpWindowName);


    public static void savert()
    {

        Bitmap memoryImage;
        memoryImage = new Bitmap(960, 540); //res
        Size s = new Size(memoryImage.Width, memoryImage.Height);

        Graphics memoryGraphics = Graphics.FromImage(memoryImage);
        memoryGraphics.CopyFromScreen(480, 270, 0, 0, s);//480, 270, 1440, 810, s);
        Color desiredColor = new Color();

        //MAGENTA COLOR 255,0,255
        desiredColor = Color.FromArgb(255, 0, 255);
        HueStruct.CIEXYZ xyzL = RGBtoXYZ(255, 0, 255);
        HueStruct.CIELab labL = XYZtoLab(xyzL);

        ParseScreen(memoryImage, labL);

    }




}