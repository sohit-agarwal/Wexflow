using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Forms;
using Eto.Drawing;
using System.Configuration;
using Wexflow.Core.Service.Client;
using Wexflow.Core.Service.Contracts;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Wexflow.Clients.Eto.Manager
{
    public class Form1 : Form
    {
		static readonly string WexflowWebServiceUri = ConfigurationManager.AppSettings["WexflowWebServiceUri"];

        readonly Button _startButton;
        readonly Button _suspendButton;
        readonly Button _resumeButton;
        readonly Button _stopButton;
        readonly DynamicLayout _buttonsLayout;
        readonly PixelLayout _buttonsPixelLayout;
        readonly TextBox _txtInfo;
        readonly GridView _gvWorkflows;
        readonly WexflowServiceClient _wexflowServiceClient;
		Dictionary<int, WorkflowInfo> _workflowsPerId;
        readonly UITimer _timer = new UITimer { Interval = 0.5 };

        public Form1()
        {
            ClientSize = new Size(792, 566);
            Title = "Wexflow";

			_startButton = new Button(StartClick) { Text = "Start", Width = 75, Enabled = false };
			_suspendButton = new Button(SuspendClick) { Text = "Suspend", Width = 75, Enabled = false };
			_resumeButton = new Button(ResumeClick) { Text = "Resume", Width = 75, Enabled = false };
			_stopButton = new Button(StopClick) { Text = "Stop", Width = 75, Enabled = false };

			_buttonsLayout = new DynamicLayout { Padding = new Padding(10) };
			_buttonsLayout.BeginVertical();
			_buttonsLayout.BeginHorizontal();
			_buttonsLayout.Add(_startButton);
			_buttonsLayout.Add(_suspendButton);
			_buttonsLayout.Add(_resumeButton);
			_buttonsLayout.Add(_stopButton);
			_buttonsLayout.EndHorizontal();
			_buttonsLayout.EndVertical();

			_buttonsPixelLayout = new PixelLayout();
			_buttonsPixelLayout.Add(_buttonsLayout, 0, 0);

			var layout = new DynamicLayout();
			layout.BeginVertical();
			layout.BeginHorizontal();
			layout.Add(_buttonsPixelLayout);
			layout.EndHorizontal();
			layout.BeginHorizontal();

			_txtInfo = new TextBox { ReadOnly = true };
			var txtInfoLayout = new DynamicLayout { Padding = new Padding(10, 0, 10, 10) };
			txtInfoLayout.BeginVertical();
			txtInfoLayout.BeginHorizontal();
			txtInfoLayout.Add(_txtInfo);
			txtInfoLayout.EndHorizontal();
			txtInfoLayout.EndVertical();

			layout.Add(txtInfoLayout);
			layout.EndHorizontal();
			layout.BeginHorizontal();

			_gvWorkflows = new GridView();
			_gvWorkflows.SelectionChanged += GvSelectionChanged;
			_gvWorkflows.CellDoubleClick += GvCellDoubleClick;

			var gvWorkflowsLayout = new DynamicLayout { Padding = new Padding(10, 0, 10, 10) };
			gvWorkflowsLayout.BeginVertical();
			gvWorkflowsLayout.BeginHorizontal();
			gvWorkflowsLayout.Add(_gvWorkflows);
			gvWorkflowsLayout.EndHorizontal();
			gvWorkflowsLayout.EndVertical();

			layout.Add(gvWorkflowsLayout);
			layout.EndVertical();

			Content = layout;

			_wexflowServiceClient = new WexflowServiceClient(WexflowWebServiceUri);
			BindGridView();
        }

		void BindGridView() 
		{
			try
			{
				var workflows = new List<WorkflowDataInfo>();
				var wfs = _wexflowServiceClient.GetWorkflows();
				_workflowsPerId = new Dictionary<int, WorkflowInfo>();
				foreach (var w in wfs)
				{
					workflows.Add(new WorkflowDataInfo(w.Id.ToString(CultureInfo.CurrentCulture), w.Name, w.LaunchType.ToString(), w.IsEnabled, w.Description));
					_workflowsPerId.Add(w.Id, w);
				}
				var collection = new ObservableCollection<WorkflowDataInfo>(workflows.OrderBy(w => int.Parse(w.Id)));
				_gvWorkflows.DataStore = collection;


				_gvWorkflows.Columns.Add(new GridColumn
				{
					DataCell = new TextBoxCell { Binding = Binding.Property<WorkflowDataInfo, string>(w => w.Id) },
					HeaderText = "Id"
				});

				_gvWorkflows.Columns.Add(new GridColumn
				{
					DataCell = new TextBoxCell { Binding = Binding.Property<WorkflowDataInfo, string>(w => w.Name) },
					HeaderText = "Name"
				});

				_gvWorkflows.Columns.Add(new GridColumn
				{
					DataCell = new TextBoxCell { Binding = Binding.Property<WorkflowDataInfo, string>(w => w.LaunchType) },
					HeaderText = "LaunchType"
				});

				_gvWorkflows.Columns.Add(new GridColumn
				{
					DataCell = new CheckBoxCell { Binding = Binding.Property<WorkflowDataInfo, bool?>(w => w.IsEnabled) },
					HeaderText = "Enabled"
				});

				_gvWorkflows.Columns.Add(new GridColumn
				{
					DataCell = new TextBoxCell { Binding = Binding.Property<WorkflowDataInfo, string>(w => w.Description) },
					HeaderText = "Description"
				});
			}
			catch (Exception e) 
			{
				MessageBox.Show("An error occured while retrieving workflows. Check Wexflow Web Service Uri and check that Wexflow Windows Service is running correctly. Error: " + e.Message, MessageBoxType.Error);
			}
		}


		int GetSlectedWorkflowId()
		{ 
			var wf = _gvWorkflows.SelectedItem as WorkflowDataInfo;
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

			_timer.Stop();

			if (wf.IsEnabled)
			{
				_timer.Elapsed += (s, ea) => UpdateButtons(id, false);
				_timer.Start();

				UpdateButtons(id, true);
			}
			else
			{
				UpdateButtons(id, true);
			}
		}

		WorkflowInfo GetWorkflow(int id)
		{ 
			return _wexflowServiceClient.GetWorkflow(id);
		}

		bool WorkflowStatusChanged(WorkflowInfo workflow)
		{
			bool changed = _workflowsPerId[workflow.Id].IsRunning != workflow.IsRunning || _workflowsPerId[workflow.Id].IsPaused != workflow.IsPaused;
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
						_txtInfo.Text = "This workflow is disabled.";
						_startButton.Enabled = _suspendButton.Enabled = _resumeButton.Enabled = _stopButton.Enabled = false;
					}
					else
					{
						if (!force && !WorkflowStatusChanged(workflow)) return;

						_startButton.Enabled = !workflow.IsRunning;
						_stopButton.Enabled = workflow.IsRunning && !workflow.IsPaused;
						_suspendButton.Enabled = workflow.IsRunning && !workflow.IsPaused;
						_resumeButton.Enabled = workflow.IsPaused;

						if (workflow.IsRunning && !workflow.IsPaused)
						{
							_txtInfo.Text = "This workflow is running...";
						}
						else if (workflow.IsPaused)
						{
							_txtInfo.Text = "This workflow is suspended.";
						}
						else
						{
							_txtInfo.Text = "";
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
				_wexflowServiceClient.StartWorkflow(wfId);
			}
		}

		void SuspendClick(object sender, EventArgs e)
		{
			var wfId = GetSlectedWorkflowId();
			if (wfId > -1)
			{
				_wexflowServiceClient.SuspendWorkflow(wfId);
				UpdateButtons(wfId, true);
			}
		}

		void ResumeClick(object sender, EventArgs e)
		{
			var wfId = GetSlectedWorkflowId();
			if (wfId > -1)
			{
				_wexflowServiceClient.ResumeWorkflow(wfId);
			}
		}

		void StopClick(object sender, EventArgs e)
		{
			var wfId = GetSlectedWorkflowId();
			if (wfId > -1)
			{
				_wexflowServiceClient.StopWorkflow(wfId);
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
