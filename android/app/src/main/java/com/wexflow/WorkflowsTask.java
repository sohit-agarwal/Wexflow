package com.wexflow;


import android.os.AsyncTask;
import android.util.Log;
import android.view.View;
import android.widget.AdapterView;
import android.widget.ListView;
import android.widget.Toast;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Timer;
import java.util.TimerTask;

import static com.wexflow.Constants.COL_ID;
import static com.wexflow.Constants.COL_LAUNCHTYPE;
import static com.wexflow.Constants.COL_NAME;

class WorkflowsTask extends AsyncTask<String, Void, List<Workflow>> {

    private final MainActivity activity;
    private final WexflowServiceClient client;
    private Exception exception;
    private Timer timer;

    WorkflowsTask(MainActivity activity) {
        this.activity = activity;
        this.client = new WexflowServiceClient(activity.getUri());
    }

    @Override
    protected List<Workflow> doInBackground(String... params) {
        try {
            return this.client.getWorkflows();
        } catch (Exception e) {
            this.exception = e;
            return null;
        }
    }

    protected void onPostExecute(List<Workflow> workflows) {
        if (this.exception != null) {
            Log.e("Wexflow", exception.toString());
            Toast.makeText(this.activity.getBaseContext(), R.string.workflows_error, Toast.LENGTH_LONG).show();
        } else {
            ArrayList<HashMap<String, String>> list = new ArrayList<>();

            for (Workflow workflow : workflows) {
                HashMap<String, String> temp = new HashMap<>();
                temp.put(COL_ID, String.valueOf(workflow.getId()));
                temp.put(COL_NAME, workflow.getName());
                temp.put(COL_LAUNCHTYPE, String.valueOf(workflow.getLaunchType()));
                list.add(temp);
                this.activity.getWorkflows().put(workflow.getId(), workflow);
            }

            final ListViewAdapter adapter = new ListViewAdapter(this.activity, list);
            ListView lvWorkflows = this.activity.getLvWorkflows();
            lvWorkflows.setAdapter(adapter);

            lvWorkflows.setOnItemClickListener(new AdapterView.OnItemClickListener() {
                @Override
                public void onItemClick(AdapterView<?> parent, View view, int position, long id) {
                    adapter.setSelection(position);
                    activity.setWorkflowId((int) id);

                    if (timer != null) {
                        timer.cancel();
                        timer.purge();
                    }

                    if (activity.getWorkflows().get((int) id).getEnabled()) {
                        timer = new Timer();
                        timer.schedule(new TimerTask() {
                            @Override
                            public void run() {
                                UpdateButtonsTask updateButtonsTask = new UpdateButtonsTask(activity);
                                updateButtonsTask.execute(false);
                            }
                        }, 0, 500);
                        UpdateButtonsTask updateButtonsTask = new UpdateButtonsTask(activity);
                        updateButtonsTask.execute(true);
                    } else {
                        UpdateButtonsTask updateButtonsTask = new UpdateButtonsTask(activity);
                        updateButtonsTask.execute(true);
                    }

                }
            });
        }
    }
}
