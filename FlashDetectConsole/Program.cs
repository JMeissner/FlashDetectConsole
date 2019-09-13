using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace FlashDetectConsole
{
    class Program
    {
        #region Public Fields

        /* CONFIG */
        public const int MEASURE_FREQUENCY_MS = 1000; //Frequency of measurements
        public const int MEASURE_HEIGHT_PERC = 10; //Currently not used
        public const double MEASURE_MARGIN = 0.05; //Margin of error (%)
        public const int MEASURE_OFFSET = 150; //Measure offset from screen corners in pixel
        public const int START_MEASURE_PERC = 40; //Currently not used

        /* WINDOW PROPERTIES */
        public static System.Timers.Timer Clock;
        public static int MSR_Height;
        public static int MSR_Start;
        public static int MSR_Stop;
        public static int Win_Height;
        public static int Win_Width;

        #endregion Public Fields

        #region Public Methods

        /// <summary>
        /// Measures difference to white in % at 4 points
        /// </summary>
        public static void DetectColor()
        {
            //Capture screen
            Bitmap bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics gr = Graphics.FromImage(bmp);
            gr.CopyFromScreen(0, 0, 0, 0, bmp.Size);

            //measure points with offset
            List<Color> measurePoints = new List<Color>();
            measurePoints.Add(bmp.GetPixel(MEASURE_OFFSET, MEASURE_OFFSET));
            measurePoints.Add(bmp.GetPixel(MEASURE_OFFSET, bmp.Height - MEASURE_OFFSET));
            measurePoints.Add(bmp.GetPixel(bmp.Width - MEASURE_OFFSET, MEASURE_OFFSET));
            measurePoints.Add(bmp.GetPixel(bmp.Width - MEASURE_OFFSET, bmp.Height - MEASURE_OFFSET));

            //Calculate difference to white
            bool IsInMargin = true;
            foreach (Color measurePoint in measurePoints)
            {
                int combinedPoint = measurePoint.R + measurePoint.G + measurePoint.G;
                int whitePoint = 3 * 255;
                double differenceToWhite = 1.0d - (double)combinedPoint / whitePoint;
                //Console.WriteLine("Difference to white: " + differenceToWhite.ToString() + ", " + combinedPoint);

                //If any measurement is above the error margin, no flash is detected
                if (differenceToWhite > MEASURE_MARGIN)
                {
                    IsInMargin = false;
                }
            }

            /* FLASH DETECTED, DO STUFF HERE */
            if (IsInMargin)
            {
                Console.WriteLine("Flashed!");
            }

            //Clean-Up
            bmp.Dispose();
            gr.Dispose();
        }

        //Not used
        public static void InitializeWindowSizes()
        {
            //Get primary screen properties
            Win_Width = Screen.PrimaryScreen.Bounds.Width;
            Win_Height = Screen.PrimaryScreen.Bounds.Height;

            //Calculate screenshot heights
            double cheight = Win_Height * (MEASURE_HEIGHT_PERC * 0.01);
            MSR_Height = (int)Math.Floor(cheight);
            MSR_Start = (int)Screen.PrimaryScreen.Bounds.Y * (START_MEASURE_PERC / 100);
            MSR_Stop = MSR_Start - MSR_Height;
        }

        #endregion Public Methods

        #region Private Methods

        //Main
        private static void Main(string[] args)
        {
            InitializeWindowSizes();
            SetTimer();
            Console.ReadLine();
        }

        //Timer Event
        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            //Console.WriteLine("The Elapsed event was raised at {0:HH:mm:ss.fff}", e.SignalTime);
            DetectColor();
        }

        //Start Timer
        private static void SetTimer()
        {
            // Create a timer with a two second interval.
            Clock = new System.Timers.Timer(MEASURE_FREQUENCY_MS);
            // Hook up the Elapsed event for the timer.
            Clock.Elapsed += OnTimedEvent;
            Clock.AutoReset = true;
            Clock.Enabled = true;
        }

        #endregion Private Methods
    }
}