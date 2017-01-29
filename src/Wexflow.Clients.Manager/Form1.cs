using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using Wexflow.Core;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.IO;
using Wexflow.Core.Service.Contracts;
using System.Windows.Threading;
using System.Diagnostics;
using System.ServiceModel.Security;
using Wexflow.Core.Service.Client;

namespace Wexflow.Clients.Manager
{
    // v1.0.5

    // TODO Linux Rest
    // TODO Linux test + setup.sh

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
        Timer timer = new Timer() { Interval = 500 };

        public Form1()
        {
            InitializeComponent();

            this.textBoxInfo.Text = "Loading workflows...";

            this.backgroundWorker1.RunWorkerAsync();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            if (Program.DEBUG_MODE || Program.IsWexflowWindowsServiceRunning())
            {
                this.wexflowServiceClient = new WexflowServiceClient(WexflowWebServiceUri);
                this.workflows = wexflowServiceClient.GetWorkflows();
            }
            else 
            {
                this.workflows = new WorkflowInfo[] { };
                this.textBoxInfo.Text = "";
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BindDataGridView();
        }

        private void BindDataGridView()
        {
            SortableBindingList<WorkflowDataInfo> workflows = new SortableBindingList<WorkflowDataInfo>();
            workflowsPerId = new Dictionary<int, WorkflowInfo>();
            foreach (WorkflowInfo workflow in this.workflows)
            {
                workflows.Add(new WorkflowDataInfo(workflow.Id, workflow.Name, workflow.LaunchType, workflow.IsEnabled, workflow.Description));
                workflowsPerId.Add(workflow.Id, workflow);
            }
            this.dataGridViewWorkflows.DataSource = workflows;

            this.dataGridViewWorkflows.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.dataGridViewWorkflows.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewWorkflows.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.dataGridViewWorkflows.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.dataGridViewWorkflows.Columns[3].Name = ColumnEnabled;
            this.dataGridViewWorkflows.Columns[3].HeaderText = ColumnEnabled;
            this.dataGridViewWorkflows.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            this.dataGridViewWorkflows.Sort(this.dataGridViewWorkflows.Columns[0], ListSortDirection.Ascending);
        }

        private int GetSlectedWorkflowId()
        {
            int wfId = -1;
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

        private WorkflowInfo GetWorkflow(int id)
        {
            if (Program.DEBUG_MODE || Program.IsWexflowWindowsServiceRunning())
            {
                if (this.windowsServiceWasStopped)
                {
                    this.wexflowServiceClient = new WexflowServiceClient(WexflowWebServiceUri);
                    this.windowsServiceWasStopped = false;
                    this.backgroundWorker1.RunWorkerAsync();
                }
                return this.wexflowServiceClient.GetWorkflow(id);
            }
            else
            {
                this.windowsServiceWasStopped = true;
                HandleNonRunningWindowsService();
            }

            return null;
        }

        private void HandleNonRunningWindowsService()
        {
            this.buttonStart.Enabled = this.buttonPause.Enabled = this.buttonResume.Enabled = this.buttonStop.Enabled = false;
            this.textBoxInfo.Text = "Wexflow Windows Service is not running.";
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            int wfId = GetSlectedWorkflowId();
            if (wfId > -1)
            {
                this.wexflowServiceClient.StartWorkflow(wfId);
            }
        }

        private void buttonPause_Click(object sender, EventArgs e)
        {
            int wfId = GetSlectedWorkflowId();
            if (wfId > -1)
            {
                this.wexflowServiceClient.SuspendWorkflow(wfId);
                this.UpdateButtons(wfId, true);
            }
        }

        private void buttonResume_Click(object sender, EventArgs e)
        {
            int wfId = GetSlectedWorkflowId();
            if (wfId > -1)
            {
                this.wexflowServiceClient.ResumeWorkflow(wfId);
            }
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            int wfId = GetSlectedWorkflowId();
            if (wfId > -1)
            {
                this.wexflowServiceClient.StopWorkflow(wfId);
                this.UpdateButtons(wfId, true);
            }
        }

        private void dataGridViewWorkflows_SelectionChanged(object sender, EventArgs e)
        {
            int wfId = GetSlectedWorkflowId();

            if (wfId > -1)
            {
                WorkflowInfo workflow = this.GetWorkflow(wfId);

                if (workflow.IsEnabled)
                {
                    timer.Stop();
                    timer.Tick += new EventHandler((o, ea) =>
                    {
                        this.UpdateButtons(wfId, false);
                    });
                    timer.Start();

                    this.UpdateButtons(wfId, true);
                }
                else
                {
                    this.UpdateButtons(wfId, true);
                }
            }
        }

        private bool WorkflowStatusChanged(WorkflowInfo workflow)
        {
            bool changed = this.workflowsPerId[workflow.Id].IsRunning != workflow.IsRunning || this.workflowsPerId[workflow.Id].IsPaused != workflow.IsPaused;
            this.workflowsPerId[workflow.Id].IsRunning = workflow.IsRunning;
            this.workflowsPerId[workflow.Id].IsPaused = workflow.IsPaused;
            return changed;
        }

        private void UpdateButtons(int wfId, bool force)
        {
            if (wfId > -1)
            {
                WorkflowInfo workflow = this.GetWorkflow(wfId);

                if (workflow != null)
                {
                    if (!workflow.IsEnabled)
                    {
                        this.textBoxInfo.Text = "This workflow is disabled.";
                        this.buttonStart.Enabled = this.buttonPause.Enabled = this.buttonResume.Enabled = this.buttonStop.Enabled = false;
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
                            this.textBoxInfo.Text = "This workflow is running...";
                        }
                        else if (workflow.IsPaused)
                        {
                            this.textBoxInfo.Text = "This workflow is suspended.";
                        }
                        else
                        {
                            this.textBoxInfo.Text = "";
                        }
                    }
                }
            }
        }

        private void dataGridViewWorkflows_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            int wfId = this.GetSlectedWorkflowId();
            if (wfId > -1)
            { 
                WorkflowInfo workflow = this.GetWorkflow(wfId);

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

        private void WriteEventLog(string msg, EventLogEntryType eventLogEntryType)
        {
            using (EventLog eventLog = new EventLog("Application"))
            {
                eventLog.Source = "Application";
                eventLog.WriteEntry(msg, eventLogEntryType, 101, 1);
            }
        }

        private void WriteEventLogError(string msg)
        {
            this.WriteEventLog(msg, EventLogEntryType.Error);
        }

        private void dataGridViewWorkflows_ColumnDividerDoubleClick(object sender, DataGridViewColumnDividerDoubleClickEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.dataGridViewWorkflows.AutoResizeColumn(e.ColumnIndex, DataGridViewAutoSizeColumnMode.DisplayedCellsExceptHeader);
            }
            e.Handled = true;
        }
    }
}