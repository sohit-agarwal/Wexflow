using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Configuration;
using Wexflow.Core.Service.Contracts;
using Wexflow.Core.Service.Client;
using System.Diagnostics;
using System.IO;
using System.Resources;
using System.Xml;

namespace Wexflow.Clients.Manager
{
    public partial class Form1 : Form
    {
        static readonly string WexflowWebServiceUri = ConfigurationManager.AppSettings["WexflowWebServiceUri"];

        const string ColumnId = "Id";
        const string ColumnEnabled = "Enabled";
        const string WexflowWindowsServicePath = @"..\Wexflow.Clients.WindowsService.exe.config";
        const string DesignerWebFile = @"..\Web Designer\index.html";

        WexflowServiceClient _wexflowServiceClient;
        WorkflowInfo[] _workflows;
        Dictionary<int, WorkflowInfo> _workflowsPerId;
        bool _windowsServiceWasStopped;
        Timer _timer;
        Exception _exception;
        readonly string _logfile;
        readonly ResourceManager _resources = new ResourceManager(typeof(Form1));

        public Form1()
        {
            InitializeComponent();

            textBoxInfo.Text = @"Loading workflows...";

            backgroundWorker1.RunWorkerAsync();

            if (File.Exists(WexflowWindowsServicePath))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(WexflowWindowsServicePath);
                XmlElement root = doc.DocumentElement;
                if (root != null)
                {
                    XmlNodeList nodeList = root.SelectNodes("/configuration/log4net/appender/file/@value");
                    if (nodeList != null && nodeList.Count > 0) {
                        _logfile = nodeList[0].Value;
                    }
                }
            }

            buttonLogs.Enabled = !string.IsNullOrEmpty(_logfile);
            buttonDesigner.Enabled = File.Exists(DesignerWebFile);
        }

        void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            if (Program.DebugMode || Program.IsWexflowWindowsServiceRunning())
            {
                try
                {
                    _wexflowServiceClient = new WexflowServiceClient(WexflowWebServiceUri);
                    _workflows = _wexflowServiceClient.GetWorkflows();
                }
                catch (Exception ex)
                {
                    _exception = ex;
                }
            }
            else 
            {
                _workflows = new WorkflowInfo[] { };
                textBoxInfo.Text = "";
            }
        }

        void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BindDataGridView();
        }

        void BindDataGridView()
        {
            if (_exception != null)
            {
                MessageBox.Show(
                    @"An error occured while retrieving workflows. Check Wexflow Web Service Uri and check that Wexflow Windows Service is running correctly.",
                    @"Wexflow", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBoxInfo.Text = "";
                return;
            }

            var sworkflows = new SortableBindingList<WorkflowDataInfo>();
            _workflowsPerId = new Dictionary<int, WorkflowInfo>();
            foreach (WorkflowInfo workflow in _workflows)
            {
                sworkflows.Add(new WorkflowDataInfo(workflow.Id, workflow.Name, workflow.LaunchType, workflow.IsEnabled, workflow.Description));
                _workflowsPerId.Add(workflow.Id, workflow);
            }
            dataGridViewWorkflows.DataSource = sworkflows;

            dataGridViewWorkflows.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            dataGridViewWorkflows.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewWorkflows.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            dataGridViewWorkflows.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            dataGridViewWorkflows.Columns[3].Name = ColumnEnabled;
            dataGridViewWorkflows.Columns[3].HeaderText = ColumnEnabled;
            dataGridViewWorkflows.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            dataGridViewWorkflows.Sort(dataGridViewWorkflows.Columns[0], ListSortDirection.Ascending);
        }

        int GetSlectedWorkflowId()
        {
            var wfId = -1;
            if (dataGridViewWorkflows.SelectedRows.Count > 0)
            {
                if(Program.DebugMode || Program.IsWexflowWindowsServiceRunning())
                {
                    wfId = int.Parse(dataGridViewWorkflows.SelectedRows[0].Cells[ColumnId].Value.ToString());
                }
                else
                {
                    HandleNonRunningWindowsService();
                }
            }
            return wfId;
        }

        WorkflowInfo GetWorkflow(int id)
        {
            if (Program.DebugMode || Program.IsWexflowWindowsServiceRunning())
            {
                if (_windowsServiceWasStopped)
                {
                    _wexflowServiceClient = new WexflowServiceClient(WexflowWebServiceUri);
                    _windowsServiceWasStopped = false;
                    backgroundWorker1.RunWorkerAsync();
                }
                return _wexflowServiceClient.GetWorkflow(id);
            }
            
			_windowsServiceWasStopped = true;
			HandleNonRunningWindowsService();

            return null;
        }

        void HandleNonRunningWindowsService()
        {
            buttonStart.Enabled = buttonPause.Enabled = buttonResume.Enabled = buttonStop.Enabled = false;
            textBoxInfo.Text = @"Wexflow Windows Service is not running.";
        }

        void buttonStart_Click(object sender, EventArgs e)
        {
            var wfId = GetSlectedWorkflowId();
            if (wfId > -1)
            {
                _wexflowServiceClient.StartWorkflow(wfId);
            }
        }

        void buttonPause_Click(object sender, EventArgs e)
        {
            var wfId = GetSlectedWorkflowId();
            if (wfId > -1)
            {
                _wexflowServiceClient.SuspendWorkflow(wfId);
                UpdateButtons(wfId, true);
            }
        }

        void buttonResume_Click(object sender, EventArgs e)
        {
            var wfId = GetSlectedWorkflowId();
            if (wfId > -1)
            {
                _wexflowServiceClient.ResumeWorkflow(wfId);
            }
        }

        void buttonStop_Click(object sender, EventArgs e)
        {
            var wfId = GetSlectedWorkflowId();
            if (wfId > -1)
            {
                _wexflowServiceClient.StopWorkflow(wfId);
                UpdateButtons(wfId, true);
            }
        }

        void dataGridViewWorkflows_SelectionChanged(object sender, EventArgs e)
        {
            var wfId = GetSlectedWorkflowId();

            if (wfId > -1)
            {
                var workflow = GetWorkflow(wfId);

                if (_timer != null)
                {
                    _timer.Stop();
                    _timer.Dispose();
                }

                if (workflow != null && workflow.IsEnabled)
                {
                    _timer = new Timer {Interval = 500};
                    _timer.Tick += (o, ea) => UpdateButtons(wfId, false);
                    _timer.Start();

                    UpdateButtons(wfId, true);
                }
                else
                {
                    UpdateButtons(wfId, true);
                }
            }
        }

        bool WorkflowStatusChanged(WorkflowInfo workflow)
        {
            var changed = _workflowsPerId[workflow.Id].IsRunning != workflow.IsRunning || _workflowsPerId[workflow.Id].IsPaused != workflow.IsPaused;
            _workflowsPerId[workflow.Id].IsRunning = workflow.IsRunning;
            _workflowsPerId[workflow.Id].IsPaused = workflow.IsPaused;
            return changed;
        }

        void UpdateButtons(int wfId, bool force)
        {
            if (wfId > -1)
            {
                var workflow = GetWorkflow(wfId);

                if (workflow != null)
                {
                    if (!workflow.IsEnabled)
                    {
                        textBoxInfo.Text = @"This workflow is disabled.";
                        buttonStart.Enabled = buttonPause.Enabled = buttonResume.Enabled = buttonStop.Enabled = false;
                    }
                    else
                    {
                        if (!force && !WorkflowStatusChanged(workflow)) return;

                        buttonStart.Enabled = !workflow.IsRunning;
                        buttonStop.Enabled = workflow.IsRunning && !workflow.IsPaused;
                        buttonPause.Enabled = workflow.IsRunning && !workflow.IsPaused;
                        buttonResume.Enabled = workflow.IsPaused;

                        if (workflow.IsRunning && !workflow.IsPaused)
                        {
                            textBoxInfo.Text = @"This workflow is running...";
                        }
                        else if (workflow.IsPaused)
                        {
                            textBoxInfo.Text = @"This workflow is suspended.";
                        }
                        else
                        {
                            textBoxInfo.Text = "";
                        }
                    }
                }
                else
                {
                    buttonStart.Enabled = false;
                    buttonStop.Enabled = false;
                    buttonPause.Enabled = false;
                    buttonResume.Enabled = false;
                    if (_timer != null)
                    {
                        _timer.Stop();
                        _timer.Dispose();
                    }
                }
            }
        }

        void dataGridViewWorkflows_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            var wfId = GetSlectedWorkflowId();
            if (wfId > -1)
            { 
                var workflow = GetWorkflow(wfId);

                if (workflow != null && workflow.IsEnabled)
                {
                    if (!workflow.IsRunning && !workflow.IsPaused)
                    {
                        buttonStart_Click(this, null);
                    }
                    else if(workflow.IsPaused)
                    {
                        buttonResume_Click(this, null);
                    }
                }
            }
        }

        void dataGridViewWorkflows_ColumnDividerDoubleClick(object sender, DataGridViewColumnDividerDoubleClickEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                dataGridViewWorkflows.AutoResizeColumn(e.ColumnIndex, DataGridViewAutoSizeColumnMode.DisplayedCellsExceptHeader);
            }
            e.Handled = true;
        }

        private void buttonLog_Click(object sender, EventArgs e)
        {
            string logFile = @"..\" + _logfile;
            if (File.Exists(logFile))
            {
                Process.Start("notepad.exe", logFile);
            }
        }

        private void buttonDesign_Click(object sender, EventArgs e)
        {
            if (File.Exists(DesignerWebFile))
            {
                Process.Start(DesignerWebFile, "");
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var about = _resources.GetString("Form1_toolStripMenuItem1_Click_About");
            var title = _resources.GetString("Form1_toolStripMenuItem1_Click_About_Title");
                
            if (MessageBox.Show(about
                , title
                , MessageBoxButtons.YesNo
                , MessageBoxIcon.Information) == DialogResult.Yes)
            {
                Process.Start("https://github.com/aelassas/Wexflow/releases/latest");
            }
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/aelassas/Wexflow/wiki");
        }
    }
}