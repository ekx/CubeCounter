using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.Media;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace CubeCounter
{
    public partial class Form1 : Form
    {
        private static bool count = false;
        private static Stopwatch sw;
        delegate void SetTextCallback(string text);
        private static bool pressed = false;
        private static int timer = 0;


        public Form1()
        {
            InitializeComponent();
            checkBox1.Checked = Properties.Settings.Default.inspectiontimer;
            checkBox2.Checked = Properties.Settings.Default.beep;
            checkBox3.Checked = Properties.Settings.Default.savebest;
            checkBox4.Checked = Properties.Settings.Default.saveall;
            comboBox1.SelectedIndex = Properties.Settings.Default.inspectionsec;

            if (checkBox3.Checked) label11.Text = Properties.Settings.Default.besttime;
            if (checkBox4.Checked)
            {
                try
                {
                    Stream stream = File.Open("times.dat", FileMode.Open);
                    BinaryFormatter bformatter = new BinaryFormatter();

                    ArrayList temp = (ArrayList)bformatter.Deserialize(stream);
                    foreach (object entry in temp)
                    {
                        checkedListBox1.Items.Add(entry);
                    }

                    stream.Close();
                }
                catch (Exception e)
                {
                    String temp = e.ToString();
                    temp = "File times.dat missing";
                    MessageBox.Show(temp);
                }
            }

            updateRecords();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            toggleCounter();
        }

        private void toggleCounter()
        {
            if (count == false)
            {
                timer = comboBox1.SelectedIndex;
                count = true;
                Thread t = new Thread(counttime);
                t.Start();
                label2.Text = "To stop the counter press space";
            }
            else
            {
                count = false;
                label2.Text = "To start the counter press space";

                if (label1.Text.Length > 2)
                {
                    checkedListBox1.Items.Add(label1.Text);
                    updateRecords();
                }
                else
                {
                    label1.Text = "00:00:00.00";
                }
            }
        }

        private void counttime()
        {
            if (checkBox1.Checked)
            {
                switch (timer)
                {
                    case 0:
                        timer = 3;
                        break;
                    case 1:
                        timer = 5;
                        break;
                    case 2:
                        timer = 10;
                        break;
                    case 3:
                        timer = 15;
                        break;
                    default:
                        timer = 0;
                        break;
                }

                while (timer > 0 && count)
                {
                    this.SetText(timer.ToString());
                    timer--;

                    if (timer == 1 && checkBox2.Checked)
                    {
                        SoundPlayer p = new SoundPlayer();
                        p.SoundLocation = "countdown.wav";
                        p.Play();
                    }

                    System.Threading.Thread.Sleep(1000);
                }
            }

            sw = new Stopwatch();
            sw.Start();
            while (count)
            {
                string temp = sw.Elapsed.ToString();
                if (temp.Length < 11) this.SetText(temp);
                else this.SetText(temp.Remove(11));
            }
            sw.Stop();
        }

        private void SetText(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.label1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.label1.Text = text;
            }
        }

        private void Form1_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.Space) && !pressed)
            {
                toggleCounter();
            }
            if (pressed) pressed = false;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.Space) && count)
            {
                toggleCounter();
                pressed = true;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.inspectiontimer = checkBox1.Checked;
            Properties.Settings.Default.Save();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.beep = checkBox2.Checked;
            Properties.Settings.Default.Save();
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.savebest = checkBox3.Checked;
            Properties.Settings.Default.Save();
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.saveall = checkBox4.Checked;
            Properties.Settings.Default.Save();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            checkBox3.Checked = false;
            Properties.Settings.Default.savebest = checkBox3.Checked;
            Properties.Settings.Default.Save();

            Properties.Settings.Default.besttime = "--:--:--.--";
            Properties.Settings.Default.Save();

            label11.Text = Properties.Settings.Default.besttime;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            checkBox4.Checked = false;
            Properties.Settings.Default.saveall = checkBox4.Checked;
            Properties.Settings.Default.Save();

            File.Delete("times.dat");
            checkedListBox1.Items.Clear();
            updateRecords();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.inspectionsec = comboBox1.SelectedIndex;
            Properties.Settings.Default.Save();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            checkedListBox1.Items.Clear();
            updateRecords();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            int temp = 0;

            foreach (int indexChecked in checkedListBox1.CheckedIndices)
            {
                checkedListBox1.Items.RemoveAt(indexChecked - temp);
                temp++;
            }

            updateRecords();
        }

        private void updateRecords()
        {
            ArrayList temp = new ArrayList();
            foreach (object entry in checkedListBox1.Items)
            {
                temp.Add(TimeSpan.Parse(checkedListBox1.GetItemText(entry)));
            }

            //determine best time

            TimeSpan shortest = new TimeSpan();
            shortest = TimeSpan.FromDays(200);

            foreach (TimeSpan ts in temp)
            {
                if (ts < shortest) shortest = ts;
            }

            if (checkBox3.Checked)
            {
                if (Properties.Settings.Default.besttime == "--:--:--.--")
                {
                    if (shortest.ToString().Length > 11)
                        Properties.Settings.Default.besttime = shortest.ToString().Remove(11);
                    else
                        Properties.Settings.Default.besttime = shortest.ToString();

                    Properties.Settings.Default.Save();

                    label11.Text = Properties.Settings.Default.besttime;
                }
                else
                {
                    TimeSpan best = shortest + shortest + shortest;

                    foreach (TimeSpan ts in temp)
                    {
                        if(ts < best) best = ts;
                    }

                    if (shortest < best)
                    {
                        if (shortest.ToString().Length > 11)
                            Properties.Settings.Default.besttime = shortest.ToString().Remove(11);
                        else
                            Properties.Settings.Default.besttime = shortest.ToString();

                        Properties.Settings.Default.Save();

                        label11.Text = Properties.Settings.Default.besttime;
                    }
                    else label11.Text = Properties.Settings.Default.besttime;
                }
            }
            else
            {
                if (shortest.TotalDays != 200)
                {
                    if (shortest.ToString().Length > 11)
                        label11.Text = shortest.ToString().Remove(11);
                    else
                        label11.Text = shortest.ToString();
                }
                else label11.Text = "--:--:--.--";
            }

            //determine average time

            if (temp.Count > 0)
            {
                double average = 0;

                foreach (TimeSpan ts in temp)
                {
                    average += ts.TotalMilliseconds;
                }

                average /= temp.Count;
                TimeSpan av = TimeSpan.FromMilliseconds(average);
                if (av.ToString().Length > 11)
                    label10.Text = av.ToString().Remove(11);
                else
                    label10.Text = av.ToString();
            }
            else label10.Text = "--:--:--.--";

            //determine avg last 5

            if (temp.Count >= 5)
            {
                double avg5 = 0;
                TimeSpan lol = TimeSpan.Zero;

                for (int c = 1; c < 6; c++)
                {
                    lol = (TimeSpan)temp[temp.Count - c];
                    avg5 += lol.TotalMilliseconds;
                }

                avg5 /= 5;
                TimeSpan av = TimeSpan.FromMilliseconds(avg5);
                if (av.ToString().Length > 11)
                    label12.Text = av.ToString().Remove(11);
                else
                    label12.Text = av.ToString();
            }
            else label12.Text = "--:--:--.--";

            //determine avg last 10

            if (temp.Count >= 10)
            {
                double avg10 = 0;
                TimeSpan lol = TimeSpan.Zero;

                for (int c = 1; c < 11; c++)
                {
                    lol = (TimeSpan)temp[temp.Count - c];
                    avg10 += lol.TotalMilliseconds;
                }

                avg10 /= 10;
                TimeSpan av = TimeSpan.FromMilliseconds(avg10);
                if (av.ToString().Length > 11)
                    label14.Text = av.ToString().Remove(11);
                else
                    label14.Text = av.ToString();
            }
            else label14.Text = "--:--:--.--";

            //determine 3 of last 5

            if (temp.Count >= 5)
            {
                double[] avg3 = new double[5];
                TimeSpan lol = TimeSpan.Zero;

                for (int c = 1; c < 6; c++)
                {
                    lol = (TimeSpan)temp[temp.Count - c];
                    avg3[c - 1] = lol.TotalMilliseconds;
                }

                Array.Sort(avg3);

                double avg = 0;

                for (int c = 1; c < 4; c++)
                {
                    avg += avg3[c];
                }

                avg /= 3;
                TimeSpan av = TimeSpan.FromMilliseconds(avg);
                if (av.ToString().Length > 11)
                    label13.Text = av.ToString().Remove(11);
                else
                    label13.Text = av.ToString();
            }
            else label13.Text = "--:--:--.--";

            //determine 10 of last 12

            if (temp.Count >= 12)
            {
                double[] avg12 = new double[12];
                TimeSpan lol = TimeSpan.Zero;

                for (int c = 1; c < 13; c++)
                {
                    lol = (TimeSpan)temp[temp.Count - c];
                    avg12[c - 1] = lol.TotalMilliseconds;
                }

                Array.Sort(avg12);

                double avg = 0;

                for (int c = 1; c < 11; c++)
                {
                    avg += avg12[c];
                }

                avg /= 10;
                TimeSpan av = TimeSpan.FromMilliseconds(avg);
                if (av.ToString().Length > 11)
                    label15.Text = av.ToString().Remove(11);
                else
                    label15.Text = av.ToString();
            }
            else label15.Text = "--:--:--.--";

            //save times

            if (checkBox4.Checked)
            {
                Stream stream = File.Open("times.dat", FileMode.Create);
                BinaryFormatter bformatter = new BinaryFormatter();

                ArrayList seri = new ArrayList();

                foreach (object entry in checkedListBox1.Items)
                {
                    seri.Add(checkedListBox1.GetItemText(entry));
                }

                bformatter.Serialize(stream, seri);
                stream.Close();
            }

            //generate new scramble algorithm

            label3.Text = "Scramble Algorithm: " + genScrambleAlg();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SaveFileDialog DialogSave = new SaveFileDialog();

            // Default file extension
            DialogSave.DefaultExt = "txt";

            // Available file extensions
            DialogSave.Filter = "Text file (*.txt)|*.txt|All files (*.*)|*.*";

            // Adds a extension if the user does not
            DialogSave.AddExtension = true;

            // Restores the selected directory, next time
            DialogSave.RestoreDirectory = true;

            // Dialog title
            DialogSave.Title = "Where do you want to save the file?";

            // Startup directory
            DialogSave.InitialDirectory = @"C:/";

            // Show the dialog and process the result
            if (DialogSave.ShowDialog() == DialogResult.OK)
            {
                TextWriter tw = new StreamWriter(DialogSave.FileName);

                tw.WriteLine("Best time: " + label11.Text);
                tw.WriteLine("Average time: " + label10.Text);
                tw.WriteLine("Average of last 5 times: " + label12.Text);
                tw.WriteLine("Average of last 10 times: " + label14.Text);
                tw.WriteLine("3 of 5: " + label13.Text);
                tw.WriteLine("10 of 12: " + label15.Text);

                tw.WriteLine("");
                tw.WriteLine("List of all times:");
                tw.WriteLine("==================");
                tw.WriteLine("");

                foreach (object entry in checkedListBox1.Items)
                {
                    tw.WriteLine(checkedListBox1.GetItemText(entry));
                }

                tw.Close();
            }
            else
            {
                
            }

            DialogSave.Dispose();
            DialogSave = null;
        }

        private string genScrambleAlg()
        {
            String alg = "";

            Scramble scr = new Scramble();
            alg = scr.generateScramble();

            return alg;
        }
    }
}
