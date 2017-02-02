package com.wexflow;


import android.os.AsyncTask;
import android.util.Log;

class UpdateButtonsTask extends AsyncTask<Boolean, Void, Workflow> {

    private final MainActivity activity;
    private final WexflowServiceClient client;
    private Exception exception;
    private Boolean force;

    UpdateButtonsTask(MainActivity activity) {
        this.activity = activity;
        this.client = new WexflowServiceClient(activity.getUri());
    }


    @Override
    protected Workflow doInBackground(Boolean... params) {
        try {
            this.force = params[0];
            return this.client.getWorkflow(this.activity.getWorkflowId());
        } catch (Exception e) {
            this.exception = e;
            return null;
        }
    }


    protected void onPostExecute(Workflow workflow) {
        if (this.exception != null) {
            Log.e("Wexflow", this.exception.toString());
            // TODO
        } else {
            if (!workflow.getEnabled()) {
                this.activity.getTxtInfo().setText(R.string.workflow_disabled);
                this.activity.getBtnStart().setEnabled(false);
                this.activity.getBtnSuspend().setEnabled(false);
                this.activity.getBtnResume().setEnabled(false);
                this.activity.getBtnStop().setEnabled(false);
            } else {
                if (!this.force && !this.activity.workflowStatusChanged(workflow)) return;

                this.activity.getBtnStart().setEnabled(!workflow.getRunning());
                this.activity.getBtnStop().setEnabled(workflow.getRunning() && !workflow.getPaused());
                this.activity.getBtnSuspend().setEnabled(workflow.getRunning() && !workflow.getPaused());
                this.activity.getBtnResume().setEnabled(workflow.getPaused());

                if (workflow.getRunning() && !workflow.getPaused()) {
                    this.activity.getTxtInfo().setText(R.string.workflow_running);
                } else if (workflow.getPaused()) {
                    this.activity.getTxtInfo().setText(R.string.workflow_suspended);
                } else {
                    this.activity.getTxtInfo().setText("");
                }
            }
        }
    }
}
