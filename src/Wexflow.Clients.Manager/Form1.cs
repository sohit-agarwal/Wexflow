using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Configuration;
using Wexflow.Core.Service.Contracts;
using Wexflow.Core.Service.Client;

namespace Wexflow.Clients.Manager
{
    // v1.0.6
    // TODO refactor (monodevelop)
	// TODO test DoIf DoWhile (tasks)
    // TODO DoIf, DoWhile, Switch/Case
    // TODO Android Manager??
    // TODO Wexflow Manager wf status live + row background color
    // TODO Test and fix ftps
    // TODO FluentFtpHelper (after FTPS fix)

    // TODO YouTube?
    // TODO Wexflow Editor
    // TODO Wexflow Designer
    // TODO Wexflow Web Manager

    public partial class Form1 : Form
    {
        static string WexflowWebServiceUri = ConfigurationManager.AppSettings["WexflowWebServiceUri"];

        const string ColumnId = "Id";
        const string ColumnEnabled = "Enabled";

        WexflowServiceClient wexflowServiceClient;
        WorkflowInfo[] workflows;
        Dictionary<int, WorkflowInfo> workflowsPerId;
        bool windowsServiceWasStopped;
        Timer timer = new Timer { Interval = 500 };

        public Form1()
        {
            InitializeComponent();

            textBoxInfo.Text = "Loading workflows...";

            backgroundWorker1.RunWorkerAsync();
        }

        void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            if (Program.DEBUG_MODE || Program.IsWexflowWindowsServiceRunning())
            {
                wexflowServiceClient = new WexflowServiceClient(WexflowWebServiceUri);
                workflows = wexflowServiceClient.GetWorkflows();
            }
            else 
            {
                workflows = new WorkflowInfo[] { };
                textBoxInfo.Text = "";
            }
        }

        void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BindDataGridView();
        }

        void BindDataGridView()
        {
            var sworkflows = new SortableBindingList<WorkflowDataInfo>();
            workflowsPerId = new Dictionary<int, WorkflowInfo>();
            foreach (WorkflowInfo workflow in workflows)
            {
                sworkflows.Add(new WorkflowDataInfo(workflow.Id, workflow.Name, workflow.LaunchType, workflow.IsEnabled, workflow.Description));
                workflowsPerId.Add(workflow.Id, workflow);
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
                if(Program.DEBUG_MODE || Program.IsWexflowWindowsServiceRunning())
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
            if (Program.DEBUG_MODE || Program.IsWexflowWindowsServiceRunning())
            {
                if (windowsServiceWasStopped)
                {
                    wexflowServiceClient = new WexflowServiceClient(WexflowWebServiceUri);
                    windowsServiceWasStopped = false;
                    backgroundWorker1.RunWorkerAsync();
                }
                return wexflowServiceClient.GetWorkflow(id);
            }
            
			windowsServiceWasStopped = true;
			HandleNonRunningWindowsService();

            return null;
        }

        void HandleNonRunningWindowsService()
        {
            buttonStart.Enabled = buttonPause.Enabled = buttonResume.Enabled = buttonStop.Enabled = false;
            textBoxInfo.Text = "Wexflow Windows Service is not running.";
        }

        void buttonStart_Click(object sender, EventArgs e)
        {
            var wfId = GetSlectedWorkflowId();
            if (wfId > -1)
            {
                wexflowServiceClient.StartWorkflow(wfId);
            }
        }

        void buttonPause_Click(object sender, EventArgs e)
        {
            var wfId = GetSlectedWorkflowId();
            if (wfId > -1)
            {
                wexflowServiceClient.SuspendWorkflow(wfId);
                UpdateButtons(wfId, true);
            }
        }

        void buttonResume_Click(object sender, EventArgs e)
        {
            var wfId = GetSlectedWorkflowId();
            if (wfId > -1)
            {
                wexflowServiceClient.ResumeWorkflow(wfId);
            }
        }

        void buttonStop_Click(object sender, EventArgs e)
        {
            var wfId = GetSlectedWorkflowId();
            if (wfId > -1)
            {
                wexflowServiceClient.StopWorkflow(wfId);
                UpdateButtons(wfId, true);
            }
        }

        void dataGridViewWorkflows_SelectionChanged(object sender, EventArgs e)
        {
            var wfId = GetSlectedWorkflowId();

            if (wfId > -1)
            {
                var workflow = GetWorkflow(wfId);

                if (workflow.IsEnabled)
                {
                    timer.Stop();
                    timer.Tick += (o, ea) => UpdateButtons(wfId, false);
                    timer.Start();

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
            var changed = workflowsPerId[workflow.Id].IsRunning != workflow.IsRunning || workflowsPerId[workflow.Id].IsPaused != workflow.IsPaused;
            workflowsPerId[workflow.Id].IsRunning = workflow.IsRunning;
            workflowsPerId[workflow.Id].IsPaused = workflow.IsPaused;
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
                        textBoxInfo.Text = "This workflow is disabled.";
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
                            textBoxInfo.Text = "This workflow is running...";
                        }
                        else if (workflow.IsPaused)
                        {
                            textBoxInfo.Text = "This workflow is suspended.";
                        }
                        else
                        {
                            textBoxInfo.Text = "";
                        }
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
    }
}