using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Forms;
using Eto.Drawing;
using System.Configuration;
using Wexflow.Core.Service.Client;
using Wexflow.Core.Service.Contracts;
using System.Collections.ObjectModel;
using System.Timers;

namespace Wexflow.Clients.Eto.Manager
{
    public class Form1 : Form
    {
		static string WexflowWebServiceUri = ConfigurationManager.AppSettings["WexflowWebServiceUri"];

		Button startButton;
		Button suspendButton;
		Button resumeButton;
		Button stopButton;
		DynamicLayout buttonsLayout;
		PixelLayout buttonsPixelLayout;
		TextBox txtInfo;
		GridView gvWorkflows;
		WexflowServiceClient wexflowServiceClient;
		Dictionary<int, WorkflowInfo> workflowsPerId;
		Timer timer = new Timer(500);

        public Form1()
        {
            ClientSize = new Size(792, 566);
            Title = "Wexflow";

			startButton = new Button(StartClick) { Text = "Start", Width = 75, Enabled = false };
			suspendButton = new Button(SuspendClick) { Text = "Suspend", Width = 75, Enabled = false };
			resumeButton = new Button(ResumeClick) { Text = "Resume", Width = 75, Enabled = false };
			stopButton = new Button(StopClick) { Text = "Stop", Width = 75, Enabled = false };

			buttonsLayout = new DynamicLayout { Padding = new Padding(10) };
			buttonsLayout.BeginVertical();
			buttonsLayout.BeginHorizontal();
			buttonsLayout.Add(startButton);
			buttonsLayout.Add(suspendButton);
			buttonsLayout.Add(resumeButton);
			buttonsLayout.Add(stopButton);
			buttonsLayout.EndHorizontal();
			buttonsLayout.EndVertical();

			buttonsPixelLayout = new PixelLayout();
			buttonsPixelLayout.Add(buttonsLayout, 0, 0);

			var layout = new DynamicLayout();
			layout.BeginVertical();
			layout.BeginHorizontal();
			layout.Add(buttonsPixelLayout);
			layout.EndHorizontal();
			layout.BeginHorizontal();

			txtInfo = new TextBox { ReadOnly = true };
			var txtInfoLayout = new DynamicLayout { Padding = new Padding(10, 0, 10, 10) };
			txtInfoLayout.BeginVertical();
			txtInfoLayout.BeginHorizontal();
			txtInfoLayout.Add(txtInfo);
			txtInfoLayout.EndHorizontal();
			txtInfoLayout.EndVertical();

			layout.Add(txtInfoLayout);
			layout.EndHorizontal();
			layout.BeginHorizontal();

			gvWorkflows = new GridView();
			gvWorkflows.SelectionChanged += GvSelectionChanged;
			gvWorkflows.CellDoubleClick += GvCellDoubleClick;

			var gvWorkflowsLayout = new DynamicLayout { Padding = new Padding(10, 0, 10, 10) };
			gvWorkflowsLayout.BeginVertical();
			gvWorkflowsLayout.BeginHorizontal();
			gvWorkflowsLayout.Add(gvWorkflows);
			gvWorkflowsLayout.EndHorizontal();
			gvWorkflowsLayout.EndVertical();

			layout.Add(gvWorkflowsLayout);
			layout.EndVertical();

			Content = layout;

			wexflowServiceClient = new WexflowServiceClient(WexflowWebServiceUri);
			BindGridView();
        }

		void BindGridView() 
		{
			var workflows = new List<WorkflowDataInfo>();
			var wfs = wexflowServiceClient.GetWorkflows();
			workflowsPerId = new Dictionary<int, WorkflowInfo>();
			foreach (var w in wfs)
			{
				workflows.Add(new WorkflowDataInfo(w.Id.ToString(), w.Name, w.LaunchType.ToString(), w.IsEnabled, w.Description));
				workflowsPerId.Add(w.Id, w);
			}
			var collection = new ObservableCollection<WorkflowDataInfo>(workflows.OrderBy(w => int.Parse(w.Id)));
			gvWorkflows.DataStore = collection;


			gvWorkflows.Columns.Add(new GridColumn
			{
				DataCell = new TextBoxCell { Binding = Binding.Property<WorkflowDataInfo, string>(w =>  w.Id) },
				HeaderText = "Id"
			});

			gvWorkflows.Columns.Add(new GridColumn
			{
				DataCell = new TextBoxCell { Binding = Binding.Property<WorkflowDataInfo, string>(w => w.Name) },
				HeaderText = "Name"
			});

			gvWorkflows.Columns.Add(new GridColumn
			{
				DataCell = new TextBoxCell { Binding = Binding.Property<WorkflowDataInfo, string>(w => w.LaunchType) },
				HeaderText = "LaunchType"
			});

			gvWorkflows.Columns.Add(new GridColumn
			{
				DataCell = new CheckBoxCell { Binding = Binding.Property<WorkflowDataInfo, bool?>(w => w.IsEnabled) },
				HeaderText = "Enabled"
			});

			gvWorkflows.Columns.Add(new GridColumn
			{
				DataCell = new TextBoxCell { Binding = Binding.Property<WorkflowDataInfo, string>(w => w.Description) },
				HeaderText = "Description"
			});
		}


		int GetSlectedWorkflowId()
		{ 
			var wf = gvWorkflows.SelectedItem as WorkflowDataInfo;
			if (wf != null)
			{
				return int.Parse(wf.Id);
			}
			return -1;
		}

		void GvSelectionChanged(object sender, EventArgs e)
		{
			var id = GetSlectedWorkflowId();
			var wf = GetWorkflow(id);

			if (wf.IsEnabled)
			{
				timer.Stop();
				timer.Elapsed += (s, ea) => { UpdateButtons(id, false); };
				timer.Start();

				UpdateButtons(id, true);
			}
			else
			{
				UpdateButtons(id, true);
			}
		}

		WorkflowInfo GetWorkflow(int id)
		{ 
			return wexflowServiceClient.GetWorkflow(id);
		}

		bool WorkflowStatusChanged(WorkflowInfo workflow)
		{
			bool changed = workflowsPerId[workflow.Id].IsRunning != workflow.IsRunning || workflowsPerId[workflow.Id].IsPaused != workflow.IsPaused;
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
						txtInfo.Text = "This workflow is disabled.";
						startButton.Enabled = suspendButton.Enabled = resumeButton.Enabled = stopButton.Enabled = false;
					}
					else
					{
						if (!force && !WorkflowStatusChanged(workflow)) return;

						startButton.Enabled = !workflow.IsRunning;
						stopButton.Enabled = workflow.IsRunning && !workflow.IsPaused;
						suspendButton.Enabled = workflow.IsRunning && !workflow.IsPaused;
						resumeButton.Enabled = workflow.IsPaused;

						if (workflow.IsRunning && !workflow.IsPaused)
						{
							txtInfo.Text = "This workflow is running...";
						}
						else if (workflow.IsPaused)
						{
							txtInfo.Text = "This workflow is suspended.";
						}
						else
						{
							txtInfo.Text = "";
						}
					}
				}
			}
		}

		void StartClick(object sender, EventArgs e)
		{ 
			var wfId = GetSlectedWorkflowId();
			if (wfId > -1)
			{
				wexflowServiceClient.StartWorkflow(wfId);
			}
		}

		void SuspendClick(object sender, EventArgs e)
		{
			var wfId = GetSlectedWorkflowId();
			if (wfId > -1)
			{
				wexflowServiceClient.SuspendWorkflow(wfId);
				UpdateButtons(wfId, true);
			}
		}

		void ResumeClick(object sender, EventArgs e)
		{
			var wfId = GetSlectedWorkflowId();
			if (wfId > -1)
			{
				wexflowServiceClient.ResumeWorkflow(wfId);
			}
		}

		void StopClick(object sender, EventArgs e)
		{
			var wfId = GetSlectedWorkflowId();
			if (wfId > -1)
			{
				wexflowServiceClient.StopWorkflow(wfId);
				UpdateButtons(wfId, true);
			}
		}

		void GvCellDoubleClick(object sender, EventArgs e)
		{ 
			var wfId = GetSlectedWorkflowId();
			if (wfId > -1)
			{
				var workflow = GetWorkflow(wfId);

				if (workflow != null && workflow.IsEnabled)
				{
					if (!workflow.IsRunning && !workflow.IsPaused)
					{
						StartClick(this, null);
					}
					else if (workflow.IsPaused)
					{
						ResumeClick(this, null);
					}
				}
			}
		}
    }
}
