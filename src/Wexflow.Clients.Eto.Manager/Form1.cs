using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Eto.Forms;
using Eto.Drawing;
using System.Configuration;
using Wexflow.Core.Service.Client;
using System.Collections.ObjectModel;
using Wexflow.Clients.Manager.Core;

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

        public Form1()
        {
            ClientSize = new Size(792, 566);
            Title = "Wexflow";
			SizeChanged += WindowSizeChanged;

			startButton = new Button(StartClick) { Text = "Start" };
			suspendButton = new Button(SuspendClick) { Text = "Suspend"};
			resumeButton = new Button(ResumeClick) { Text = "Resume"};
			stopButton = new Button(StopClick) { Text = "Stop"};

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
			buttonsPixelLayout.Add(buttonsLayout, GetButtonsPositionX(), 0);

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
			var workflows = new ObservableCollection<WorkflowDataInfo>();
			var wfs = wexflowServiceClient.GetWorkflows();
			foreach (var w in wfs)
			{
				workflows.Add(new WorkflowDataInfo(w.Id, w.Name, w.LaunchType, w.IsEnabled, w.Description));
			}
			gvWorkflows.DataStore = workflows;
		}

		int GetButtonsPositionX()
		{
			return ClientSize.Width - 340;
		}

		void WindowSizeChanged(object sender, EventArgs e)
		{
			buttonsPixelLayout.Move(buttonsLayout, GetButtonsPositionX(), 0);
		}

		void StartClick(object sender, EventArgs e)
		{ 
		
		}

		void SuspendClick(object sender, EventArgs e)
		{

		}

		void ResumeClick(object sender, EventArgs e)
		{

		}

		void StopClick(object sender, EventArgs e)
		{

		}
    }
}
