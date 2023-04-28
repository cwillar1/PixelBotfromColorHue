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

    public struct CIEXYZ
    {
        /// <summary>
        /// Gets an empty CIEXYZ structure.
        /// </summary>
        public static readonly CIEXYZ Empty = new CIEXYZ();
        /// <summary>
        /// Gets the CIE D65 (white) structure.
        /// </summary>
        public static readonly CIEXYZ D65 = new CIEXYZ(0.9505, 1.0, 1.0890);


        public double x;
        public double y;
        public double z;

        public static bool operator ==(CIEXYZ item1, CIEXYZ item2)
        {
            return (
                item1.X == item2.X
                && item1.Y == item2.Y
                && item1.Z == item2.Z
                );
        }

        public static bool operator !=(CIEXYZ item1, CIEXYZ item2)
        {
            return (
                item1.X != item2.X
                || item1.Y != item2.Y
                || item1.Z != item2.Z
                );
        }

        /// <summary>
        /// Gets or sets X component.
        /// </summary>
        public double X
        {
            get
            {
                return this.x;
            }
            set
            {
                this.x = (value > 0.9505) ? 0.9505 : ((value < 0) ? 0 : value);
            }
        }

        /// <summary>
        /// Gets or sets Y component.
        /// </summary>
        public double Y
        {
            get
            {
                return this.y;
            }
            set
            {
                this.y = (value > 1.0) ? 1.0 : ((value < 0) ? 0 : value);
            }
        }

        /// <summary>
        /// Gets or sets Z component.
        /// </summary>
        public double Z
        {
            get
            {
                return this.z;
            }
            set
            {
                this.z = (value > 1.089) ? 1.089 : ((value < 0) ? 0 : value);
            }
        }

        public CIEXYZ(double x, double y, double z)
        {
            this.x = (x > 0.9505) ? 0.9505 : ((x < 0) ? 0 : x);
            this.y = (y > 1.0) ? 1.0 : ((y < 0) ? 0 : y);
            this.z = (z > 1.089) ? 1.089 : ((z < 0) ? 0 : z);
        }

        public override bool Equals(Object obj)
        {
            if (obj == null || GetType() != obj.GetType()) return false;

            return (this == (CIEXYZ)obj);
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
        }

    }


    public struct CIELab
    {
        /// <summary>
        /// Gets an empty CIELab structure.
        /// </summary>
        public static readonly CIELab Empty = new CIELab();

        public double l;
        public double a;
        public double b;


        public static bool operator ==(CIELab item1, CIELab item2)
        {
            return (
                item1.L == item2.L
                && item1.A == item2.A
                && item1.B == item2.B
                );
        }

        public static bool operator !=(CIELab item1, CIELab item2)
        {
            return (
                item1.L != item2.L
                || item1.A != item2.A
                || item1.B != item2.B
                );
        }


        /// <summary>
        /// Gets or sets L component.
        /// </summary>
        public double L
        {
            get
            {
                return this.l;
            }
            set
            {
                this.l = value;
            }
        }

        /// <summary>
        /// Gets or sets a component.
        /// </summary>
        public double A
        {
            get
            {
                return this.a;
            }
            set
            {
                this.a = value;
            }
        }

        /// <summary>
        /// Gets or sets a component.
        /// </summary>
        public double B
        {
            get
            {
                return this.b;
            }
            set
            {
                this.b = value;
            }
        }

        public CIELab(double l, double a, double b)
        {
            this.l = l;
            this.a = a;
            this.b = b;
        }

        public override bool Equals(Object obj)
        {
            if (obj == null || GetType() != obj.GetType()) return false;

            return (this == (CIELab)obj);
        }

        public override int GetHashCode()
        {
            return L.GetHashCode() ^ a.GetHashCode() ^ b.GetHashCode();
        }

    }

    static void Main(string[] args)
    {

        while (true)
        {
            if (GetAsyncKeyState(VK_CAPITAL) != 0)
            {
                //REMEMBER TO BE IN FULLSCREEN!
                //remove overlay
                
                //toggleOverlay();
                //System.Threading.Thread.Sleep(8);
                //process

                savert();

                //toggleOverlay();
            }
        }
    }



    public static void toggleOverlay()
    {
        keybd_event(VK_LMENU, 0, 0, 0);
        SendKeys.SendWait("{z}");
        keybd_event(VK_LMENU, 0, KEYEVENTF_KEYUP, 0);
    }
    public static CIEXYZ RGBtoXYZ(int red, int green, int blue)
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
        return new CIEXYZ(
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
    public static CIELab XYZtoLab(CIEXYZ xyzL)
    {
        double y = xyzL.y;
        double x = xyzL.x;
        double z = xyzL.z;

        CIELab lab = CIELab.Empty;

        lab.L = 116.0 * Fxyz(y / CIEXYZ.D65.Y) - 16;
        lab.A = 500.0 * (Fxyz(x / CIEXYZ.D65.X) - Fxyz(y / CIEXYZ.D65.Y));
        lab.B = 200.0 * (Fxyz(y / CIEXYZ.D65.Y) - Fxyz(z / CIEXYZ.D65.Z));

        return lab;
    }

    public static double deltaECalc(CIELab lab1, CIELab lab2)
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

    public static void ParseScreen(Bitmap memoryImage, CIELab labL)
    {

        for (int y = 0; y < 540; y+=10)
        {
            for (int x = 0; x < 960; x+=3)
            {
                // Get the current pixels color
                Color currentPixelColor = memoryImage.GetPixel(x, y);

                //Color currentPixelColor = Color.FromArgb(214, 118, 72);
                CIEXYZ xyzK = RGBtoXYZ(currentPixelColor.R, currentPixelColor.G, currentPixelColor.B);
                CIELab labK = XYZtoLab(xyzK);

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
        CIEXYZ xyzL = RGBtoXYZ(255, 0, 255);
        CIELab labL = XYZtoLab(xyzL);

        ParseScreen(memoryImage, labL);

    }




}