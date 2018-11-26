using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Resources;
using System.ServiceProcess;
using System.Windows.Forms;
using System.Xml;
using Wexflow.Core.Service.Client;
using Wexflow.Core.Service.Contracts;

namespace Wexflow.Clients.Manager
{
    public partial class Manager : Form
    {
        private static readonly string WexflowWebServiceUri = ConfigurationManager.AppSettings["WexflowWebServiceUri"];

        private const string ColumnId = "Id";
        private const string ColumnEnabled = "Enabled";
        private const string WexflowServerPath = @"..\Wexflow.Server.exe.config";
        private const string Backend = @"..\Backend\index.html";

        private WexflowServiceClient _wexflowServiceClient;
        private WorkflowInfo[] _workflows;
        private Dictionary<int, WorkflowInfo> _workflowsPerId;
        private bool _windowsServiceWasStopped;
        private Timer _timer;
        private Exception _exception;
        private readonly string _logfile;
        private readonly ResourceManager _resources = new ResourceManager(typeof(Manager));
        private bool _serviceRestarted;

        public Manager()
        {
            InitializeComponent();

            LoadWorkflows();

            if (File.Exists(WexflowServerPath))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(WexflowServerPath);
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
            buttonBackend.Enabled = File.Exists(Backend);
            buttonRestart.Enabled = !Program.DebugMode;

            dataGridViewWorkflows.MouseWheel += new MouseEventHandler(MouseWheelEvt);
        }

        private void MouseWheelEvt(object sender, MouseEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void LoadWorkflows()
        {
            _exception = null;

            textBoxInfo.Text = @"Loading workflows...";

            backgroundWorker1.RunWorkerAsync();
        }

        private void BackgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (Program.DebugMode || Program.IsWexflowWindowsServiceRunning())
                {
                    try
                    {
                        _wexflowServiceClient = new WexflowServiceClient(WexflowWebServiceUri);

                        var keyword = textBoxSearch.Text.ToUpper();
                        _workflows = _wexflowServiceClient.Search(keyword);
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
            MessageBox.Show(@"An error occured while retrieving workflows.", "Wexflow", MessageBoxButtons.OK);
        }

        private void BackgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BindDataGridView();

            if (_serviceRestarted)
            {
                System.Threading.Thread.Sleep(1000);
                textBoxInfo.Text = "Wexflow server was restarted with success.";
                _serviceRestarted = false;
            }
        }

        private void BindDataGridView()
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

        private int GetSlectedWorkflowId()
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

        private WorkflowInfo GetWorkflow(int id)
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

        private void HandleNonRunningWindowsService()
        {
            buttonStart.Enabled = buttonPause.Enabled = buttonResume.Enabled = buttonStop.Enabled = false;
            textBoxInfo.Text = @"Wexflow Windows Service is not running.";
        }

        private void ButtonStart_Click(object sender, EventArgs e)
        {
            var wfId = GetSlectedWorkflowId();
            if (wfId > -1)
            {
                _wexflowServiceClient.StartWorkflow(wfId);
            }
        }

        private void ButtonPause_Click(object sender, EventArgs e)
        {
            var wfId = GetSlectedWorkflowId();
            if (wfId > -1)
            {
                _wexflowServiceClient.SuspendWorkflow(wfId);
                UpdateButtons(wfId, true);
            }
        }

        private void ButtonResume_Click(object sender, EventArgs e)
        {
            var wfId = GetSlectedWorkflowId();
            if (wfId > -1)
            {
                _wexflowServiceClient.ResumeWorkflow(wfId);
            }
        }

        private void ButtonStop_Click(object sender, EventArgs e)
        {
            var wfId = GetSlectedWorkflowId();
            if (wfId > -1)
            {
                _wexflowServiceClient.StopWorkflow(wfId);
                UpdateButtons(wfId, true);
            }
        }

        private void DataGridViewWorkflows_SelectionChanged(object sender, EventArgs e)
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

        private bool WorkflowStatusChanged(WorkflowInfo workflow)
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

        private void UpdateButtons(int wfId, bool force)
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

        private void DataGridViewWorkflows_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
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

        private void DataGridViewWorkflows_ColumnDividerDoubleClick(object sender, DataGridViewColumnDividerDoubleClickEventArgs e)
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

        private void ButtonBackend_Click(object sender, EventArgs e)
        {
            if (File.Exists(Backend))
            {
                Process.Start(Backend, "");
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

        private void BackgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            string errorMsg;
            _serviceRestarted = RestartWindowsService(Program.WexflowServiceName, out errorMsg);
            
            if (!_serviceRestarted)
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

        private void buttonSearch_Click(object sender, EventArgs e)
        {
            LoadWorkflows();
        }

        private void textBoxSearch_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                LoadWorkflows();
            }
        }
    }
}
