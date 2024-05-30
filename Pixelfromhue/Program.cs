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



    public static void toggleOverlay() //overwatch overlay
    {
        keybd_event(VK_LMENU, 0, 0, 0);
        SendKeys.SendWait("{z}");
        keybd_event(VK_LMENU, 0, KEYEVENTF_KEYUP, 0);
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

        for (int y = 0; y < 540; y+=5)
        {
            for (int x = 0; x < 960; x+=3)
            {
                // Get the current pixels color
                Color currentPixelColor = memoryImage.GetPixel(x, y);

                //Color currentPixelColor = Color.FromArgb(214, 118, 72);
                HueStruct.CIEXYZ xyzK = HueStruct.RGBtoXYZ(currentPixelColor.R, currentPixelColor.G, currentPixelColor.B);
                HueStruct.CIELab labK = HueStruct.XYZtoLab(xyzK);

                //calculate delta E from both labs
                double deltaE = HueStruct.deltaECalc(labL, labK);
                Console.WriteLine(deltaE);

                // Finally compare the pixels hex color and the desired hex color (if they match we found a pixel)
                if (deltaE < 40)
                {
                    Console.WriteLine("Color found");
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
        HueStruct.CIEXYZ xyzL = HueStruct.RGBtoXYZ(255, 0, 255);
        HueStruct.CIELab labL = HueStruct.XYZtoLab(xyzL);

        memoryImage.Save(@"C:\Users\Gobbl\source\repos\Pixelfromhue\test.jpg");

        ParseScreen(memoryImage, labL);

    }




}