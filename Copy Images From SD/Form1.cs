using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Copy_Images_From_SD
{
    public partial class Form1 : Form
    {
        BackgroundWorker m_oWorker;
        List<string> allowedCopy;
        public Form1()
        {
            InitializeComponent();
            m_oWorker = new BackgroundWorker();

            // Create a background worker thread that ReportsProgress &
            // SupportsCancellation
            // Hook up the appropriate events.
            m_oWorker.DoWork += new DoWorkEventHandler(m_oWorker_DoWork);
            m_oWorker.ProgressChanged += new ProgressChangedEventHandler
                    (m_oWorker_ProgressChanged);
            m_oWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler
                    (m_oWorker_RunWorkerCompleted);
            m_oWorker.WorkerReportsProgress = true;
            m_oWorker.WorkerSupportsCancellation = true;
        }
        #region Background worker

        
        /// <summary>
        /// On completed do the appropriate task
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_oWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // The background process is complete. We need to inspect
            // our response to see if an error occurred, a cancel was
            // requested or if we completed successfully.  
            if (e.Cancelled)
            {
                toolStripStatusLabel1.Text = "Task Cancelled.";
            }

            // Check to see if an error occurred in the background process.

            else if (e.Error != null)
            {
                toolStripStatusLabel1.Text = "Error while performing background operation.";
            }
            else
            {
                // Everything completed normally.
                toolStripStatusLabel1.Text = "Task Completed...";
            }

            //Change the status of the buttons on the UI accordingly
            buttonCopy.Enabled = true;
            buttonCancel.Enabled = false;
        }

        /// <summary>
        /// Notification is performed here to the progress bar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_oWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

            // This function fires on the UI thread so it's safe to edit

            // the UI control directly, no funny business with Control.Invoke :)

            // Update the progressBar with the integer supplied to us from the

            // ReportProgress() function.  

            toolStripProgressBar1.Value = e.ProgressPercentage;
            toolStripStatusLabel1.Text = "Processing......" + toolStripProgressBar1.Value.ToString() + "%";
        }

        /// <summary>
        /// Time consuming operations go here </br>
        /// i.e. Database operations,Reporting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_oWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string[] allfiles = System.IO.Directory.GetFiles(textBoxSrc.Text, "*.*", System.IO.SearchOption.AllDirectories);
            double precntFactor = (double)100 / allfiles.Length;
            for (int i = 0; i < allfiles.Length; i++)
            {
                processSingleFile(allfiles, i);


                //Progress reporting 
                m_oWorker.ReportProgress(Convert.ToInt32(i * precntFactor));
                if (m_oWorker.CancellationPending)
                {
                    // Set the e.Cancel flag so that the WorkerCompleted event
                    // knows that the process was cancelled.
                    e.Cancel = true;
                    m_oWorker.ReportProgress(0);
                    return;
                }
            }

            //Report 100% completion on operation completed
            m_oWorker.ReportProgress(100);
        }


        #endregion

        #region Events
        private void buttonBrowse1_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(textBoxSrc.Text))
                folderBrowserDialog1.SelectedPath = textBoxSrc.Text;
            folderBrowserDialog1.ShowDialog();
            textBoxSrc.Text = folderBrowserDialog1.SelectedPath;
        }

        private void buttonBrowse2_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(textBoxDest.Text))
                folderBrowserDialog1.SelectedPath = textBoxDest.Text;
            folderBrowserDialog1.ShowDialog();
            textBoxDest.Text = folderBrowserDialog1.SelectedPath;
        }

        private void buttonCopy_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(textBoxDest.Text) || !Directory.Exists(textBoxSrc.Text))
            { //TODO: make alerts looks more pro
                MessageBox.Show("Direcotories do not exist");
                return;
            }
            if (!buildAllowedList())
            {
                MessageBox.Show("Please mark at least one type to copy");
                return;
            }
              //TODO: add auto save settings  

            buttonCopy.Enabled = false;
            buttonCancel.Enabled = true;
            m_oWorker.RunWorkerAsync();
            
        }


        private void buttonCancel_Click(object sender, EventArgs e)
        {
            if (m_oWorker.IsBusy)
            {

                // Notify the worker thread that a cancel has been requested.

                // The cancel will not actually happen until the thread in the

                // DoWork checks the m_oWorker.CancellationPending flag. 

                m_oWorker.CancelAsync();
            }
        }
        #endregion

        #region Methods

        private void processSingleFile(string[] allfiles, int i)
        {
            FileInfo info = new FileInfo(allfiles[i]);
            if (fileShouldBeCopy(info))
            {
                string tempPath = getFileDestPath(info);
                if (!Directory.Exists(textBoxDest.Text + "\\" + tempPath))
                    Directory.CreateDirectory(textBoxDest.Text + "\\" + tempPath);
                File.Copy(info.FullName, textBoxDest.Text + "\\" + tempPath + "\\" + info.Name, true);
            }
        }

        private bool fileShouldBeCopy(FileInfo info)
        {
            if (allowedCopy.Contains(info.Extension.ToLower()))
                return true;

            return false;               
        }
        private string getFileDestPath(FileInfo info)
        {
            DateTime lastModified = System.IO.File.GetLastWriteTime(info.FullName);
            return  info.Extension.Replace(".","") + "\\" + lastModified.ToString("yyyy.MM.dd");

        }

        private bool buildAllowedList()
        {
            //TODO: add more file types 
            allowedCopy = new List<string>();
            if (!checkBoxJPG.Checked && !checkBoxRaw.Checked && !checkBoxVideo.Checked)
                return false;
            if (checkBoxJPG.Checked)
                allowedCopy.Add(".jpg");
            if (checkBoxRaw.Checked)
            {
                allowedCopy.Add(".arw");
            }
            if (checkBoxVideo.Checked)
            {
                allowedCopy.Add(".mp4");
            }
            return true;

        }       
        #endregion
    }
}
