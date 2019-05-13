using System.Windows.Forms;
using Framework.Logging;

namespace Framework.Util
{
    /// <summary>
    /// A simple helper class that implements a sleeper function that does not block the event loop.
    /// </summary>
    internal sealed class Sleeper
    {
        private int _mS;

        /// <summary>
        /// Initialises the timer with a given amount of milliseconds.
        /// </summary>
        /// <param name="milliseconds">Amount of milliseconds to assign to this timer, MUST be > 0!</param>
        internal Sleeper(int milliseconds)
        {
            if (milliseconds == 0 || milliseconds < 0)
            {
                Logger.WriteInfo("Framework.Util.Sleep >> Illegal value for milliseconds: '" + milliseconds + "', timer is disabled!");
                this._mS = 0;
            }
            else this._mS = milliseconds;
        }

        /// <summary>
        /// Holds the calling thread for the amount of milliseconds passed during creation.
        /// </summary>
        internal void Wait()
        {
            if (this._mS == 0) return;

            Timer timer1 = new Timer();
            timer1.Interval = this._mS;
            timer1.Enabled = true;
            timer1.Start();
            timer1.Tick += (s, e) =>
            {
                timer1.Enabled = false;
                timer1.Stop();
            };
            while (timer1.Enabled)
            {
                Application.DoEvents();
            }
        }
    }
}
