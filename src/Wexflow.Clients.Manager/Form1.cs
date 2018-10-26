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
using System.ServiceProcess;

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

            LoadWorkflows();

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
            buttonRestart.Enabled = !Program.DebugMode;

            dataGridViewWorkflows.MouseWheel += new MouseEventHandler(MouseWheelEvt);
        }

        private void MouseWheelEvt(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0 && dataGridViewWorkflows.FirstDisplayedScrollingRowIndex > 0)
            {
                dataGridViewWorkflows.FirstDisplayedScrollingRowIndex--;
            }
            else if (e.Delta < 0)
            {
                dataGridViewWorkflows.FirstDisplayedScrollingRowIndex++;
            }
        }

        private void LoadWorkflows()
        {
            _exception = null;

            textBoxInfo.Text = @"Loading workflows...";

            backgroundWorker1.RunWorkerAsync();
        }

        void BackgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            try
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
                else if (!Program.DebugMode && !Program.IsWexflowWindowsServiceRunning())
                {
                    _exception = new Exception();
                }
                else
                {
                    _workflows = new WorkflowInfo[] { };
                    textBoxInfo.Text = "";
                }
            }
            catch (Exception)
            {
                ShowError();
            }
        }

        private void ShowError()
        {
            MessageBox.Show(
                    @"An error occured while retrieving workflows. Check that Wexflow Windows Service is running and check Wexflow Web Service Uri in the settings.",
                    @"Wexflow", MessageBoxButtons.OK);
        }

        void BackgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BindDataGridView();

            if (serviceRestarted)
            {
                System.Threading.Thread.Sleep(1000);
                textBoxInfo.Text = "Wexflow server was restarted with success.";
                serviceRestarted = false;
            }
        }

        void BindDataGridView()
        {
            if (_exception != null)
            {
	            textBoxInfo.Text = "";
	            dataGridViewWorkflows.DataSource = new SortableBindingList<WorkflowDataInfo>();
                ShowError();
                return;
            }

            var sworkflows = new SortableBindingList<WorkflowDataInfo>();
            _workflowsPerId = new Dictionary<int, WorkflowInfo>();
            foreach (WorkflowInfo workflow in _workflows)
            {
                sworkflows.Add(new WorkflowDataInfo(workflow.Id, workflow.Name, workflow.LaunchType, workflow.IsEnabled, workflow.Description));

                if (!_workflowsPerId.ContainsKey(workflow.Id))
                {
                    _workflowsPerId.Add(workflow.Id, workflow);
                }
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

        void ButtonStart_Click(object sender, EventArgs e)
        {
            var wfId = GetSlectedWorkflowId();
            if (wfId > -1)
            {
                _wexflowServiceClient.StartWorkflow(wfId);
            }
        }

        void ButtonPause_Click(object sender, EventArgs e)
        {
            var wfId = GetSlectedWorkflowId();
            if (wfId > -1)
            {
                _wexflowServiceClient.SuspendWorkflow(wfId);
                UpdateButtons(wfId, true);
            }
        }

        void ButtonResume_Click(object sender, EventArgs e)
        {
            var wfId = GetSlectedWorkflowId();
            if (wfId > -1)
            {
                _wexflowServiceClient.ResumeWorkflow(wfId);
            }
        }

        void ButtonStop_Click(object sender, EventArgs e)
        {
            var wfId = GetSlectedWorkflowId();
            if (wfId > -1)
            {
                _wexflowServiceClient.StopWorkflow(wfId);
                UpdateButtons(wfId, true);
            }
        }

        void DataGridViewWorkflows_SelectionChanged(object sender, EventArgs e)
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
            if (_workflowsPerId.ContainsKey(workflow.Id))
            {
                var changed = _workflowsPerId[workflow.Id].IsRunning != workflow.IsRunning || _workflowsPerId[workflow.Id].IsPaused != workflow.IsPaused;
                _workflowsPerId[workflow.Id].IsRunning = workflow.IsRunning;
                _workflowsPerId[workflow.Id].IsPaused = workflow.IsPaused;
                return changed;
            }

            return false;
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

        void DataGridViewWorkflows_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            var wfId = GetSlectedWorkflowId();
            if (wfId > -1)
            { 
                var workflow = GetWorkflow(wfId);

                if (workflow != null && workflow.IsEnabled)
                {
                    if (!workflow.IsRunning && !workflow.IsPaused)
                    {
                        ButtonStart_Click(this, null);
                    }
                    else if(workflow.IsPaused)
                    {
                        ButtonResume_Click(this, null);
                    }
                }
            }
        }

        void DataGridViewWorkflows_ColumnDividerDoubleClick(object sender, DataGridViewColumnDividerDoubleClickEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                dataGridViewWorkflows.AutoResizeColumn(e.ColumnIndex, DataGridViewAutoSizeColumnMode.DisplayedCellsExceptHeader);
            }
            e.Handled = true;
        }

        private void ButtonLog_Click(object sender, EventArgs e)
        {
            string logFile = @"..\" + _logfile;
            if (File.Exists(logFile))
            {
                Process.Start("notepad.exe", logFile);
            }
        }

        private void ButtonDesign_Click(object sender, EventArgs e)
        {
            if (File.Exists(DesignerWebFile))
            {
                Process.Start(DesignerWebFile, "");
            }
        }

        private void ToolStripMenuItem1_Click(object sender, EventArgs e)
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

        private void HelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/aelassas/Wexflow/wiki");
        }

        private void ButtonRefresh_Click(object sender, EventArgs e)
        {
            LoadWorkflows();
        }

        private void ButtonRestart_Click(object sender, EventArgs e)
        {
            _timer.Stop();
            textBoxInfo.Text = "Restarting Wexflow server...";
            _workflows = new WorkflowInfo[] { };
            BindDataGridView();
            backgroundWorker2.RunWorkerAsync();
        }

        private bool serviceRestarted;

        private void BackgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            string errorMsg;
            serviceRestarted = RestartWindowsService(Program.WexflowServiceName, out errorMsg);
            
            if (!serviceRestarted)
            {
                MessageBox.Show("An error occurred while restoring Wexflow server: " + errorMsg);
            }
        }

        private void BackgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            LoadWorkflows();
        }

        private bool RestartWindowsService(string serviceName, out string errorMsg)
        {
            errorMsg = string.Empty;
            ServiceController serviceController = new ServiceController(serviceName);
            try
            {
                if ((serviceController.Status.Equals(ServiceControllerStatus.Running)) || (serviceController.Status.Equals(ServiceControllerStatus.StartPending)))
                {
                    serviceController.Stop();
                }
                serviceController.WaitForStatus(ServiceControllerStatus.Stopped);
                serviceController.Start();
                serviceController.WaitForStatus(ServiceControllerStatus.Running);
                return true;
            }
            catch (Exception e)
            {
                errorMsg = e.Message;
                return false;
            }
        }

    }
}
