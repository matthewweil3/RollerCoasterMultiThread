using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Schema;

namespace RollerCoasterMultiThread
{
    public partial class Form1 : Form
    {
        delegate void SetTextCallback(string text, string textBoxName);
        private static AutoResetEvent[] NotFull;
        private static AutoResetEvent[] NotEmpty;
        Thread rider;
        Thread[] rides;
        private Object[] lineLock; //locks for lines
        int[] lines; 
        
        int numOfRiders;
        int rideLength;
        int maxQueue;
        int rideCount;
        Random r = new Random();
        //int ridenum;

        
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
             
            rideLength = int.Parse(textBox1.Text) * 1000; //time of ride
            maxQueue = int.Parse(textBox2.Text); //max amount of people in line
            numOfRiders = int.Parse(textBox3.Text);//number of riders from form
            rideCount = int.Parse(textBox4.Text); //number of ride threads

            rider = new Thread(new ThreadStart(this.Rider)); //new person thread
            rides = new Thread[rideCount]; //creates array of threads based on number of rides inputted by user
            NotFull = new AutoResetEvent[rideCount];
            for(int i = 0; i < rideCount; i++)
            {
                NotFull[i] = new AutoResetEvent(true);
            }
            NotEmpty = new AutoResetEvent[rideCount];
            for (int i = 0; i < rideCount; i++)
            {
                NotEmpty[i] = new AutoResetEvent(false);
            }

            lines = new int[rideCount];
            for(int i = 0; i < rideCount; i++)
            {
                lines[i] = 0;
            }

            lineLock = new Object[rideCount];
            for(int i = 0; i < rideCount; i++)
            {
                lineLock[i] = new Object();
            }




            for (int i = 0; i < rideCount; i++)
            {
                Thread t = new Thread(()=> this.RideA(i)); //fix here throws exception on starting ride
                rides[i] = t;
                rides[i].Start();
            }


            rider.Start();

           /* for(int i = 0; i < rideCount; i++)
            {
                
            }
           */

        }


        private void Rider()
        {
            while (true)
            {
                int ridenum;
                for (int i = 0; i < numOfRiders; i++)
                {
                    do
                    {
                        ridenum = r.Next(0, rideCount);
                    }
                    while (lines[ridenum] == maxQueue);
                    Stopwatch wait = Stopwatch.StartNew();
                    NotFull[ridenum].WaitOne();
                    lock (lineLock[ridenum])
                    {
                        lines[ridenum]++;
                        SetText("In Line for ride " + ridenum + Environment.NewLine, "textBox5");
                    }
                    NotEmpty[ridenum].Set();
                    wait.Stop();
                    long waitTime = wait.ElapsedMilliseconds;
                    SetText("Wait time was " + waitTime.ToString() + Environment.NewLine, "textBox5");

                }
            }
        }

        private void RideA(int ridenum)
        {

            while (true)
            {
                NotEmpty[ridenum - 1].WaitOne(); //something wrong here
                lock (lineLock[ridenum - 1])
                {
                    lines[ridenum - 1]--;
                }
                SetText("Riding " + (ridenum - 1) + Environment.NewLine, "textBox5");
                Thread.Sleep(rideLength);
                NotFull[ridenum - 1].Set();
            }
           
        }



        private void SetText(string text, string textBoxName)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (textBoxName == "textBox5")
            {
                if (this.textBox5.InvokeRequired)
                {
                    SetTextCallback d = new SetTextCallback(SetText);
                    this.Invoke(d, new object[] { text, textBoxName });
                }
                else
                {
                    this.textBox5.Text += text;
                }
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            for(int i = 0; i < rideCount; i++)
            {
                rides[i].Abort();
            }
            rider.Abort();
            SetText("All Riders on their last rides" + Environment.NewLine, "textBox5");

            SetText("Park is now closed!" + Environment.NewLine, "textBox5");
            

        }
    }
}
