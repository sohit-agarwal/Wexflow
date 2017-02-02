package com.wexflow;

import android.os.AsyncTask;
import android.util.Log;
import android.widget.Toast;

class ActionTask extends AsyncTask<ActionType, Void, String> {

    private final MainActivity activity;
    private final WexflowServiceClient client;
    private Exception exception;
    private ActionType actionType;

    ActionTask(MainActivity activity) {
        this.activity = activity;
        this.client = new WexflowServiceClient(activity.getUri());
    }

    @Override
    protected String doInBackground(ActionType... params) {
        try {
            this.actionType = params[0];
            switch (this.actionType) {
                case Start:
                    this.client.start(this.activity.getWorkflowId());
                    break;
                case Suspend:
                    this.client.suspend(this.activity.getWorkflowId());
                    break;
                case Resume:
                    this.client.resume(this.activity.getWorkflowId());
                    break;
                case Stop:
                    this.client.stop(this.activity.getWorkflowId());
                    break;
            }
            return null;
        } catch (Exception e) {
            this.exception = e;
            return null;
        }
    }

    protected void onPostExecute(String str) {
        if (this.exception != null) {
            // TODO
            Log.e("Wexflow", this.exception.toString());
        } else {
            if (this.actionType == ActionType.Suspend || this.actionType == ActionType.Stop) {
                UpdateButtonsTask updateButtonsTask = new UpdateButtonsTask(this.activity);
                updateButtonsTask.execute(true);
            }

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.append("Workflow ").append(this.activity.getWorkflowId()).append(" ");

            switch (this.actionType) {
                case Start:
                    stringBuilder.append("started.");
                    break;
                case Suspend:
                    stringBuilder.append("was suspended.");
                    break;
                case Resume:
                    stringBuilder.append("was resumed.");
                    break;
                case Stop:
                    stringBuilder.append("was stopped.");
                    break;
            }

            Toast.makeText(this.activity.getBaseContext(), stringBuilder.toString(), Toast.LENGTH_SHORT).show();
        }
    }

}
