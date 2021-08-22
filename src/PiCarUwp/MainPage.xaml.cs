using GHIElectronics.UWP.Shields;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PiCarUwp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        FEZHAT hat;
        HCSR04 dist;
        Random rnd;
        DispatcherTimer timer;
        DispatcherTimer timer2;
        public MainPage()
        {
            this.InitializeComponent();
            Setup();
        }

        private async void Setup()
        {

            hat = await FEZHAT.CreateAsync();
            dist = new HCSR04(hat, FEZHAT.DigitalPin.DIO16, FEZHAT.DigitalPin.DIO26);
            
            rnd = new Random();
            
            timer2 = new DispatcherTimer();
            timer2.Interval = TimeSpan.FromMilliseconds(1000);
            timer2.Tick += Timer2_Tick;
            rnd = new Random();
            timer2.Start();

            Thread threadMoving = new Thread(new ThreadStart(MovingLoop));
            threadMoving.Start();

        }

        async void MovingLoop()
        {
            while(true){
                var length = dist.GetDistance();
                if (length.Centimeters <= 10)
                {
                    Mundur();
                    Thread.Sleep(500);
                }
                else if (length.Centimeters < 15)
                {
                    if (rnd.Next(0, 3) <= 1)
                    {
                        Kiri();
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        Kanan();
                        Thread.Sleep(1000);
                    }
                }
                else if (length.Centimeters >= 15)
                    Maju();

                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                    TxtDist.Text = length.Centimeters.ToString();
                    TxtArah.Text = Arah;
                });
                Thread.Sleep(50);
            }
        }
        static FEZHAT.Color[] Colors = new FEZHAT.Color[] { FEZHAT.Color.Blue, FEZHAT.Color.Cyan, FEZHAT.Color.Green, FEZHAT.Color.Magneta, FEZHAT.Color.Red, FEZHAT.Color.White, FEZHAT.Color.Yellow };
        private void Timer2_Tick(object sender, object e)
        {
            var color = Colors[ rnd.Next(0, Colors.Length)];
            hat.D2.Color = color;
            hat.D3.Color = color;
        }

        string Arah = string.Empty;

        void Maju()
        {
            hat.MotorA.Speed = 0.8;
            hat.MotorB.Speed = 0.8;
            Arah = "Maju";
            
        }
        void Mundur()
        {
            hat.MotorA.Speed = -1;
            hat.MotorB.Speed = -1;
            Arah = "Mundur";

        }
        void Kanan()
        {
            hat.MotorA.Speed = -0.8;
            hat.MotorB.Speed = 0.8;
            Arah = "Kanan";
        }
        void Kiri()
        {
            hat.MotorA.Speed = 0.8;
            hat.MotorB.Speed = -0.8;
            Arah = "Kiri";
        }
    }
}
