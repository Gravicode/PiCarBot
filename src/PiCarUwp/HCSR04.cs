using GHIElectronics.UWP.Shields;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitsNet;
using Windows.Devices.Gpio;
namespace PiCarUwp
{
    public class HCSR04
    {
        private GpioController gpio = GpioController.GetDefault();

        private FEZHAT.DigitalPin trig;
        private FEZHAT.DigitalPin echo;

        //private int trig_Pin;
        //private int echo_Pin;

        FEZHAT hat;

        static Object deviceLock = new object();

        Stopwatch sw = new Stopwatch();

        public int TimeoutMilliseconds { get; set; } = 20;

        /// <summary>
        /// Create an HCSR04 Sensor
        /// </summary>
        /// <param name="trig_Pin"></param>
        /// <param name="echo_Pin"></param>
        /// <param name="timeoutMilliseconds">defaults to 20</param>
        public HCSR04(FEZHAT hat, FEZHAT.DigitalPin trig_Pin, FEZHAT.DigitalPin echo_Pin, int timeoutMilliseconds = 20)
        {
            this.hat = hat;
            Initialise(trig_Pin, echo_Pin, timeoutMilliseconds);
        }

        /// <summary>
        /// Initialise ultra sonic distance sensor
        /// </summary>
        /// <param name="trig_Pin"></param>
        /// <param name="echo_Pin"></param>
        /// <param name="maxDistance">Set Ultra Sonic maximum distance to detect.  This is approximate only.  Based on 34.3 cm per millisecond, 20 degrees C at sea level</param>
        public HCSR04(FEZHAT hat, FEZHAT.DigitalPin trig_Pin, FEZHAT.DigitalPin echo_Pin, Length maxDistance)
        {
            this.hat = hat;
            int milliSeconds = (int)(maxDistance.Centimeters / 34.3 * 2);
            Initialise(trig_Pin, echo_Pin, milliSeconds);
        }

        private void Initialise(FEZHAT.DigitalPin trig_Pin, FEZHAT.DigitalPin echo_Pin, int timeoutMilliseconds)
        {
            this.trig = trig_Pin;
            this.echo = echo_Pin;

            TimeoutMilliseconds = timeoutMilliseconds;

            //trig = gpio.OpenPin(trig_Pin);
            //echo = gpio.OpenPin(echo_Pin);

            //trig.SetDriveMode(GpioPinDriveMode.Output);
            //echo.SetDriveMode(GpioPinDriveMode.Input);

            hat.WriteDigital(trig, false);
        }

        /// <summary>
        /// Set Ultra Sonic maximum distance to detect.  This is approximate only.  Based on 34.3 cm per millisecond, 20 degrees C at sea level
        /// </summary>
        /// <param name="maxDistance"></param>
        /// <returns></returns>
        public Length GetDistance(Length maxDistance)
        {
            int milliSeconds = (int)(maxDistance.Centimeters / 34.3 * 2);
            return GetDistance(milliSeconds);
        }

        public Length GetDistance()
        {
            return GetDistance(TimeoutMilliseconds);
        }

        /// <summary>
        /// Measures distance in centimeters
        /// </summary>
        /// <param name="timeoutMilliseconds">20 milliseconds is enough time to measure up to 3 meters</param>
        /// <returns></returns>
        public Length GetDistance(int timeoutMilliseconds = 20)
        {
            lock (deviceLock)
            {

                //http://www.c-sharpcorner.com/UploadFile/167ad2/how-to-use-ultrasonic-sensor-hc-sr04-in-arduino/
                //http://www.modmypi.com/blog/hc-sr04-ultrasonic-range-sensor-on-the-raspberry-pi


                hat.WriteDigital(trig,false);                     // ensure the trigger is off
                Task.Delay(TimeSpan.FromMilliseconds(1)).Wait();  // wait for the sensor to settle

                hat.WriteDigital(trig, true);                          // turn on the pulse
                Task.Delay(TimeSpan.FromMilliseconds(.01)).Wait();      // let the pulse run for 10 microseconds
                hat.WriteDigital(trig, false);                           // turn off the pulse

                var time = PulseIn(echo, true, timeoutMilliseconds);

                // https://en.wikipedia.org/wiki/Speed_of_sound
                // speed of sound is 34300 cm per second or 34.3 cm per millisecond
                // since the sound waves traveled to the obstacle and back to the sensor
                // I am dividing by 2 to represent travel time to the obstacle                

                return Length.FromCentimeters(time * 34.3 / 2.0); // at 20 degrees at sea level
            }
        }

        private double PulseIn(FEZHAT.DigitalPin pin, bool value, int timeout)
        {
            sw.Restart();

            // Wait for pulse
            while (sw.ElapsedMilliseconds < timeout && hat.ReadDigital(pin) != value) { }

            if (sw.ElapsedMilliseconds >= timeout)
            {
                sw.Stop();
                return 0;
            }
            sw.Restart();

            // Wait for pulse end
            while (sw.ElapsedMilliseconds < timeout && hat.ReadDigital(pin) == value) { }

            sw.Stop();

            return sw.ElapsedMilliseconds < timeout ? sw.Elapsed.TotalMilliseconds : 0;
        }
    }
}
