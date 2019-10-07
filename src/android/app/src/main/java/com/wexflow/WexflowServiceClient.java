package com.wexflow;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.io.BufferedInputStream;
import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.net.HttpURLConnection;
import java.net.URL;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.io.InputStream;
import java.io.OutputStream;

class WexflowServiceClient {

    private static final int READ_TIMEOUT = 10000;
    private static final int CONNECTION_TIMEOUT = 15000;

    private final String uri;

    WexflowServiceClient(String uri) {
        this.uri = uri.replaceAll("/+$", "");
        preferIPv4Stack();
        disableAddressCache();
        disableKeepAlive();
    }

    private static boolean post(String urlString) throws IOException {
        HttpURLConnection urlConnection;

        URL url = new URL(urlString);

        urlConnection = (HttpURLConnection) url.openConnection();
        urlConnection.setRequestMethod("POST");
        urlConnection.setRequestProperty("Connection", "close");
        urlConnection.setUseCaches(false);
        urlConnection.setReadTimeout(READ_TIMEOUT);
        urlConnection.setConnectTimeout(CONNECTION_TIMEOUT);
        urlConnection.setDoOutput(true);
        urlConnection.connect();

        BufferedReader br = new BufferedReader(new InputStreamReader(urlConnection.getInputStream(), "utf-8"));
        StringBuilder sb = new StringBuilder();
        String responseLine;
        while ((responseLine = br.readLine()) != null) {
            sb.append(responseLine.trim());
        }

        String response = sb.toString();

        if(response.isEmpty()){
            return true;
        }

        return  Boolean.valueOf(response);
    }


    private static String getString(String urlString) throws IOException {
        HttpURLConnection urlConnection;

        URL url = new URL(urlString);

        urlConnection = (HttpURLConnection) url.openConnection();
        urlConnection.setRequestMethod("GET");
        urlConnection.setRequestProperty("Connection", "close");
        urlConnection.setUseCaches(false);
        urlConnection.setReadTimeout(READ_TIMEOUT);
        urlConnection.setConnectTimeout(CONNECTION_TIMEOUT);
        urlConnection.setDoOutput(true);
        urlConnection.connect();

        BufferedReader br = new BufferedReader(new InputStreamReader(url.openStream()));

        StringBuilder sb = new StringBuilder();
        String line;
        while ((line = br.readLine()) != null) {
            sb.append(line);
        }
        br.close();

        return sb.toString();
    }

    private static JSONArray getJSONArray(String url) throws IOException, JSONException {
        String json = getString(url);
        return new JSONArray(json);
    }

    private static JSONObject getJSONObject(String url) throws IOException, JSONException {
        String json = getString(url);
        return new JSONObject(json);
    }

    private static void preferIPv4Stack() {
        System.setProperty("java.net.preferIPv4Stack", "true");
    }

    private static void disableKeepAlive() {
        System.setProperty("http.keepAlive.", "false");
    }

    private static void disableAddressCache() {
        System.setProperty("networkaddress.cache.ttl", "0");
        System.setProperty("networkaddress.cache.negative.ttl", "0");
    }

    List<Workflow> getWorkflows() throws IOException, JSONException {
        String uri = this.uri + "/search?s=&u=" + LoginActivity.Username + "&p=" + LoginActivity.Password;
        JSONArray jsonArray = getJSONArray(uri);
        List<Workflow> workflows = new ArrayList<>();
        for (int i = 0; i < jsonArray.length(); i++) {
            JSONObject jsonObject = jsonArray.getJSONObject(i);
            workflows.add(Workflow.fromJSONObject(jsonObject));
        }
        Collections.sort(workflows, new WorkflowComparator());
        return workflows;
    }

    Workflow getWorkflow(String username, String password, int id) throws IOException, JSONException {
        String uri = this.uri + "/workflow?u=" + username + "&p=" + password + "&w=" + id;
        JSONObject jsonObject = getJSONObject(uri);
        return Workflow.fromJSONObject(jsonObject);
    }

    User getUser(String qusername, String qpassword, String username)throws IOException, JSONException {
        String uri = this.uri + "/user?qu="+qusername+"&qp="+qpassword+"&username=" + username;
        JSONObject jsonObject = getJSONObject(uri);
        return User.fromJSONObject(jsonObject);
    }

    Boolean start(int id) throws IOException {
        String uri = this.uri + "/start?w=" + id + "&u=" + LoginActivity.Username + "&p=" + LoginActivity.Password;
        return post(uri);
    }

    Boolean suspend(int id) throws IOException {
        String uri = this.uri + "/suspend?w=" + id + "&u=" + LoginActivity.Username + "&p=" + LoginActivity.Password;
        return post(uri);
    }

    Boolean resume(int id) throws IOException {
        String uri = this.uri + "/resume?w=" + id + "&u=" + LoginActivity.Username + "&p=" + LoginActivity.Password;
        return post(uri);
    }

    Boolean stop(int id) throws IOException {
        String uri = this.uri + "/stop?w=" + id + "&u=" + LoginActivity.Username + "&p=" + LoginActivity.Password;
        return post(uri);
    }

    Boolean approve(int id) throws IOException {
        String uri = this.uri + "/approve?w=" + id + "&u=" + LoginActivity.Username + "&p=" + LoginActivity.Password;
        return post(uri);
    }

    Boolean disapprove(int id) throws IOException {
        String uri = this.uri + "/disapprove?w=" + id + "&u=" + LoginActivity.Username + "&p=" + LoginActivity.Password;
        return post(uri);
    }
}
