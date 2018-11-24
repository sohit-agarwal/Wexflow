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
        private static readonly string WexflowWebServiceUri = ConfigurationManager.AppSettings["WexflowWebServiceUri"];

        private readonly Button _startButton;
        private readonly Button _suspendButton;
        private readonly Button _resumeButton;
        private readonly Button _stopButton;
        private readonly TextBox _txtInfo;
        private readonly GridView _gvWorkflows;
        private readonly WexflowServiceClient _wexflowServiceClient;
        private Dictionary<int, WorkflowInfo> _workflowsPerId;
        private UITimer _timer;

        public Form1()
        {
            ClientSize = new Size(792, 566);
            Title = "Wexflow";

			_startButton = new Button(StartClick) { Text = "Start", Width = 75, Enabled = false };
			_suspendButton = new Button(SuspendClick) { Text = "Suspend", Width = 75, Enabled = false };
			_resumeButton = new Button(ResumeClick) { Text = "Resume", Width = 75, Enabled = false };
			_stopButton = new Button(StopClick) { Text = "Stop", Width = 75, Enabled = false };

			var buttonsLayout = new DynamicLayout { Padding = new Padding(10) };
			buttonsLayout.BeginVertical();
			buttonsLayout.BeginHorizontal();
			buttonsLayout.Add(_startButton);
			buttonsLayout.Add(_suspendButton);
			buttonsLayout.Add(_resumeButton);
			buttonsLayout.Add(_stopButton);
			buttonsLayout.EndHorizontal();
			buttonsLayout.EndVertical();

			var buttonsPixelLayout = new PixelLayout();
			buttonsPixelLayout.Add(buttonsLayout, 0, 0);

			var layout = new DynamicLayout();
			layout.BeginVertical();
			layout.BeginHorizontal();
			layout.Add(buttonsPixelLayout);
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

        private void BindGridView() 
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

        private int GetSlectedWorkflowId()
		{ 
			var wf = _gvWorkflows.SelectedItem as WorkflowDataInfo;
			if (wf != null)
			{
				return int.Parse(wf.Id);
			}
			return -1;
		}

        private void GvSelectionChanged(object sender, EventArgs e)
		{
			var id = GetSlectedWorkflowId();
			var wf = GetWorkflow(id);

		    if (_timer != null && _timer.Started)
		    {
		        _timer.Stop();
                _timer.Dispose();
            }

			if (wf != null && wf.IsEnabled)
			{
                _timer = new UITimer { Interval = 0.5 };
                _timer.Elapsed += (s, ea) => UpdateButtons(id, false);
				_timer.Start();

				UpdateButtons(id, true);
			}
			else
			{
				UpdateButtons(id, true);
			}
		}

        private WorkflowInfo GetWorkflow(int id)
		{ 
			return _wexflowServiceClient.GetWorkflow(id);
		}

        private bool WorkflowStatusChanged(WorkflowInfo workflow)
		{
			bool changed = _workflowsPerId[workflow.Id].IsRunning != workflow.IsRunning || _workflowsPerId[workflow.Id].IsPaused != workflow.IsPaused;
			_workflowsPerId[workflow.Id].IsRunning = workflow.IsRunning;
			_workflowsPerId[workflow.Id].IsPaused = workflow.IsPaused;
			return changed;
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
				else
				{
				    _startButton.Enabled = false;
				    _stopButton.Enabled = false;
				    _suspendButton.Enabled = false;
				    _resumeButton.Enabled = false;
                    if (_timer != null && _timer.Started)
				    {
				        _timer.Stop();
				        _timer.Dispose();
				    }
                }
			}
		}

        private void StartClick(object sender, EventArgs e)
		{ 
			var wfId = GetSlectedWorkflowId();
			if (wfId > -1)
			{
				_wexflowServiceClient.StartWorkflow(wfId);
			}
		}

        private void SuspendClick(object sender, EventArgs e)
		{
			var wfId = GetSlectedWorkflowId();
			if (wfId > -1)
			{
				_wexflowServiceClient.SuspendWorkflow(wfId);
				UpdateButtons(wfId, true);
			}
		}

        private void ResumeClick(object sender, EventArgs e)
		{
			var wfId = GetSlectedWorkflowId();
			if (wfId > -1)
			{
				_wexflowServiceClient.ResumeWorkflow(wfId);
			}
		}

        private void StopClick(object sender, EventArgs e)
		{
			var wfId = GetSlectedWorkflowId();
			if (wfId > -1)
			{
				_wexflowServiceClient.StopWorkflow(wfId);
				UpdateButtons(wfId, true);
			}
		}

        private void GvCellDoubleClick(object sender, EventArgs e)
		{ 
			var wfId = GetSlectedWorkflowId();
		    if (wfId <= -1) return;

		    var workflow = GetWorkflow(wfId);
		    if (workflow == null || !workflow.IsEnabled) return;

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
