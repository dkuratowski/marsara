using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace RC.Common.Diagnostics.RCLReader
{
    public partial class mainForm : Form
    {
        public mainForm()
        {
            InitializeComponent();
        }

        private void mainForm_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files != null && files.Length == 1)
            {
                FileStream stream = null;
                BinaryReader reader = null;
                
                try
                {
                    stream = new FileStream(files[0], FileMode.Open);
                    reader = new BinaryReader(stream);

                    byte[] byteArray = reader.ReadBytes((int)stream.Length);
                    List<RCPackage> logPackages = GetLogPackages(byteArray);

                    PopulateGrid(logPackages);

                    reader.Close();
                    reader.Dispose();
                    stream.Close();
                    stream.Dispose();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Error");
                }
                finally
                {
                    if (reader != null)
                    {
                        reader.Close();
                        reader.Dispose();
                    }
                    if (stream != null)
                    {
                        stream.Close();
                        stream.Dispose();
                    }
                }
            }
            return;
        }

        private void mainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="byteArray"></param>
        private List<RCPackage> GetLogPackages(byte[] byteArray)
        {
            int offset = 0;
            int remaining = byteArray.Length;
            List<RCPackage> packages = new List<RCPackage>();

            while (offset < byteArray.Length)
            {
                int parsedBytes;
                RCPackage logPackage = RCPackage.Parse(byteArray, offset, remaining, out parsedBytes);
                if (logPackage.IsCommitted &&
                   (logPackage.PackageFormat.ID == Program.EVENT_FORMAT ||
                    logPackage.PackageFormat.ID == Program.FORK_FORMAT ||
                    logPackage.PackageFormat.ID == Program.JOIN_FORMAT ||
                    logPackage.PackageFormat.ID == Program.EXCEPTION_FORMAT))
                {
                    packages.Add(logPackage);
                }
                else
                {
                    throw new Exception(string.Format("Unexpected package format ID: {0}", logPackage.PackageFormat.ID));
                }
                offset += parsedBytes;
                remaining -= parsedBytes;
            }
            return packages;
        }

        /// <summary>
        /// 
        /// </summary>
        private void PopulateGrid(List<RCPackage> logPackages)
        {
            this.gridLog.Columns.Clear();
            this.gridLog.Rows.Clear();
            this.doubleSelections.Clear();
            this.threadsToColumns.Clear();
            this.normalExceptions.Clear();
            this.fatalExceptions.Clear();

            foreach (RCPackage package in logPackages)
            {
                if (package.PackageFormat.ID == Program.EVENT_FORMAT)
                {
                    CreateEventFormatRow(package);
                }
                else if (package.PackageFormat.ID == Program.FORK_FORMAT)
                {
                    CreateForkRow(package);
                }
                else if (package.PackageFormat.ID == Program.JOIN_FORMAT)
                {
                    CreateJoinRow(package);
                }
                else if (package.PackageFormat.ID == Program.EXCEPTION_FORMAT)
                {
                    CreateExceptionRow(package);
                }
            }

            this.gridLog.AutoResizeRowHeadersWidth(DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="package"></param>
        private void CreateEventFormatRow(RCPackage package)
        {
            string threadName = package.ReadString(1);
            long timestamp = package.ReadLong(2);
            string evt = package.ReadString(3);

            if (!this.threadsToColumns.ContainsKey(threadName))
            {
                int idxOfCol = this.gridLog.Columns.Add(threadName, threadName);
                this.threadsToColumns.Add(threadName, idxOfCol);
            }

            object[] rowContent = new object[this.threadsToColumns.Count];
            rowContent[this.threadsToColumns[threadName]] = evt;

            int idxOfRow = this.gridLog.Rows.Add(rowContent);
            this.gridLog.Rows[idxOfRow].HeaderCell.Value = timestamp.ToString();

            DataGridViewCell cellOfEvent = this.gridLog[this.threadsToColumns[threadName], idxOfRow];
            cellOfEvent.Style.BackColor = Color.LightGray;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="package"></param>
        private void CreateForkRow(RCPackage package)
        {
            string newThreadName = package.ReadString(1);
            string parentThreadName = package.ReadString(3);
            long timestamp = package.ReadLong(4);

            if (!this.threadsToColumns.ContainsKey(newThreadName))
            {
                int idxOfCol = this.gridLog.Columns.Add(newThreadName, newThreadName);
                this.threadsToColumns.Add(newThreadName, idxOfCol);
            }
            if (!this.threadsToColumns.ContainsKey(parentThreadName))
            {
                int idxOfCol = this.gridLog.Columns.Add(parentThreadName, parentThreadName);
                this.threadsToColumns.Add(parentThreadName, idxOfCol);
            }

            object[] rowContent = new object[this.threadsToColumns.Count];
            rowContent[this.threadsToColumns[newThreadName]] = "THREAD_START";
            rowContent[this.threadsToColumns[parentThreadName]] = "THREAD_FORK";

            int idxOfRow = this.gridLog.Rows.Add(rowContent);
            this.gridLog.Rows[idxOfRow].HeaderCell.Value = timestamp.ToString();

            DataGridViewCell cellOfNew = this.gridLog[this.threadsToColumns[newThreadName], idxOfRow];
            DataGridViewCell cellOfParent = this.gridLog[this.threadsToColumns[parentThreadName], idxOfRow];

            cellOfNew.Style.BackColor = Color.LightGreen;
            cellOfParent.Style.BackColor = Color.LightGreen;

            if (!this.doubleSelections.ContainsKey(cellOfNew))
            {
                this.doubleSelections.Add(cellOfNew, cellOfParent);
            }
            if (!this.doubleSelections.ContainsKey(cellOfParent))
            {
                this.doubleSelections.Add(cellOfParent, cellOfNew);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="package"></param>
        private void CreateJoinRow(RCPackage package)
        {
            string runningThreadName = package.ReadString(1);
            string waitingThreadName = package.ReadString(3);
            long timestamp = package.ReadLong(4);

            if (!this.threadsToColumns.ContainsKey(runningThreadName))
            {
                int idxOfCol = this.gridLog.Columns.Add(runningThreadName, runningThreadName);
                this.threadsToColumns.Add(runningThreadName, idxOfCol);
            }
            if (!this.threadsToColumns.ContainsKey(waitingThreadName))
            {
                int idxOfCol = this.gridLog.Columns.Add(waitingThreadName, waitingThreadName);
                this.threadsToColumns.Add(waitingThreadName, idxOfCol);
            }

            object[] rowContent = new object[this.threadsToColumns.Count];
            rowContent[this.threadsToColumns[runningThreadName]] = "THREAD_FINISH";
            rowContent[this.threadsToColumns[waitingThreadName]] = "THREAD_JOIN";

            int idxOfRow = this.gridLog.Rows.Add(rowContent);
            this.gridLog.Rows[idxOfRow].HeaderCell.Value = timestamp.ToString();

            DataGridViewCell cellOfRunning = this.gridLog[this.threadsToColumns[runningThreadName], idxOfRow];
            DataGridViewCell cellOfWaiting = this.gridLog[this.threadsToColumns[waitingThreadName], idxOfRow];

            cellOfRunning.Style.BackColor = Color.LightPink;
            cellOfWaiting.Style.BackColor = Color.LightPink;

            if (!this.doubleSelections.ContainsKey(cellOfRunning))
            {
                this.doubleSelections.Add(cellOfRunning, cellOfWaiting);
            }
            if (!this.doubleSelections.ContainsKey(cellOfWaiting))
            {
                this.doubleSelections.Add(cellOfWaiting, cellOfRunning);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="package"></param>
        private void CreateExceptionRow(RCPackage package)
        {
            string threadName = package.ReadString(1);
            long timestamp = package.ReadLong(2);
            bool isFatal = (package.ReadByte(3) == (byte)0x00) ? false : true;
            string ex = package.ReadString(4);

            if (!this.threadsToColumns.ContainsKey(threadName))
            {
                int idxOfCol = this.gridLog.Columns.Add(threadName, threadName);
                this.threadsToColumns.Add(threadName, idxOfCol);
            }

            object[] rowContent = new object[this.threadsToColumns.Count];
            rowContent[this.threadsToColumns[threadName]] = isFatal ? "FATAL_EXCEPTION" : "EXCEPTION";

            int idxOfRow = this.gridLog.Rows.Add(rowContent);
            this.gridLog.Rows[idxOfRow].HeaderCell.Value = timestamp.ToString();

            DataGridViewCell cellOfException = this.gridLog[this.threadsToColumns[threadName], idxOfRow];
            cellOfException.Style.BackColor = isFatal ? Color.Red : Color.Yellow;

            if (isFatal)
            {
                if (!this.fatalExceptions.ContainsKey(cellOfException))
                {
                    this.fatalExceptions.Add(cellOfException, ex);
                }
            }
            else
            {
                if (!this.normalExceptions.ContainsKey(cellOfException))
                {
                    this.normalExceptions.Add(cellOfException, ex);
                }
            }
        }

        /// <summary>
        /// Called when the selection has been changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gridLog_SelectionChanged(object sender, EventArgs e)
        {
            if (!changingSelectionFromProgram)
            {
                DataGridViewSelectedCellCollection selectedCells = this.gridLog.SelectedCells;
                if (selectedCells.Count > 0)
                {
                    changingSelectionFromProgram = true;
                    int cellIdx = 0;
                    foreach (DataGridViewCell cell in selectedCells)
                    {
                        if (cellIdx > 0)
                        {
                            cell.Selected = false;
                        }
                        cellIdx++;
                    }

                    if (this.doubleSelections.ContainsKey(selectedCells[0]))
                    {
                        int rowIdx = selectedCells[0].RowIndex;
                        int colIdx0 = (selectedCells[0].ColumnIndex <= this.doubleSelections[selectedCells[0]].ColumnIndex)
                                    ? selectedCells[0].ColumnIndex
                                    : this.doubleSelections[selectedCells[0]].ColumnIndex;
                        int colIdx1 = (selectedCells[0].ColumnIndex > this.doubleSelections[selectedCells[0]].ColumnIndex)
                                    ? selectedCells[0].ColumnIndex
                                    : this.doubleSelections[selectedCells[0]].ColumnIndex;

                        for (int i = colIdx0; i <= colIdx1; i++)
                        {
                            this.gridLog[i, rowIdx].Selected = true;
                        }
                    }

                    if (this.normalExceptions.ContainsKey(selectedCells[0]))
                    {
                        MessageBox.Show(this.normalExceptions[selectedCells[0]], "Exception details");
                    }

                    if (this.fatalExceptions.ContainsKey(selectedCells[0]))
                    {
                        MessageBox.Show(this.fatalExceptions[selectedCells[0]], "Fatal exception details");
                    }

                    changingSelectionFromProgram = false;
                }
            }
        }

        /// <summary>
        /// This flag indicates whether the current selection is being changed by the program or not.
        /// </summary>
        private bool changingSelectionFromProgram = false;

        /// <summary>
        /// This dictionary maps the thread names to columns.
        /// </summary>
        private Dictionary<string, int> threadsToColumns = new Dictionary<string, int>();

        /// <summary>
        /// Cell pairs that have to be selected if one of them is selected.
        /// </summary>
        private Dictionary<DataGridViewCell, DataGridViewCell> doubleSelections = new Dictionary<DataGridViewCell, DataGridViewCell>();

        /// <summary>
        /// This dictionary maps the exception cells to the actual exception message to display.
        /// </summary>
        private Dictionary<DataGridViewCell, string> normalExceptions = new Dictionary<DataGridViewCell, string>();

        /// <summary>
        /// This dictionary maps the fatal exception cells to the actual exception message to display.
        /// </summary>
        private Dictionary<DataGridViewCell, string> fatalExceptions = new Dictionary<DataGridViewCell, string>();
    }
}
