package com.wexflow;

import android.content.Intent;
import android.content.SharedPreferences;
import android.preference.PreferenceManager;
import android.support.v7.app.AppCompatActivity;

import android.os.AsyncTask;

import android.os.Bundle;
import android.view.KeyEvent;
import android.view.View;
import android.view.View.OnClickListener;
import android.view.inputmethod.EditorInfo;
import android.widget.AutoCompleteTextView;
import android.widget.Button;
import android.widget.EditText;
import android.widget.TextView;
import java.security.*;

/**
 * A login screen that offers login via email/password.
 */
public class LoginActivity extends AppCompatActivity {

    private SharedPreferences sharedPref;

    // UI references.
    private AutoCompleteTextView mUsernameView;
    private EditText mPasswordView;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_login);

        sharedPref = PreferenceManager.getDefaultSharedPreferences(this);

        // Set up the login form.
        mUsernameView = findViewById(R.id.username);

        mPasswordView = findViewById(R.id.password);
        mPasswordView.setOnEditorActionListener(new TextView.OnEditorActionListener() {
            @Override
            public boolean onEditorAction(TextView textView, int id, KeyEvent keyEvent) {
                if (id == EditorInfo.IME_ACTION_DONE || id == EditorInfo.IME_NULL) {
                    login();
                    return true;
                }
                return false;
            }
        });

        Button mEmailSignInButton = findViewById(R.id.email_sign_in_button);
        mEmailSignInButton.setOnClickListener(new OnClickListener() {
            @Override
            public void onClick(View view) {
                login();
            }
        });

        Button mSettingsButton = findViewById(R.id.settings);
        mSettingsButton.setOnClickListener(new OnClickListener() {
           @Override
           public void onClick(View view) {
               Intent intent = new Intent(LoginActivity.this, SettingsActivity.class );
               LoginActivity.this.startActivity(intent);
           }
       });

    }

    private void login() {
        UserLoginTask task = new UserLoginTask(mUsernameView.getText().toString(), mPasswordView.getText().toString());
        task.execute();
    }



    /**
     * Represents an asynchronous login/registration task used to authenticate
     * the user.
     */
    public class UserLoginTask extends AsyncTask<Void, Void, Boolean> {

        private String uri;
        private final String mUsername;
        private final String mPassword;
        private WexflowServiceClient client;
        private Boolean restrictedAccess = false;

        UserLoginTask(String username, String password) {
            mUsername = username;
            mPassword = password;
        }

        @Override
        protected Boolean doInBackground(Void... params) {

            try {
                uri = sharedPref.getString(SettingsActivity.KEY_PREF_WEXFLOW_URI, getResources().getString(R.string.pref_wexflow_defualt_value));
                client = new WexflowServiceClient(uri);
                User user = client.getUser(mUsername);

                String password = user.getPassword();
                UserProfile up = user.getUserProfile();

                if(up.equals(UserProfile.Administrator) &&  password.equals(md5(this.mPassword)))
                {
                    return true;
                }
                else
                {
                    if(up.equals(UserProfile.Restricted)){
                        restrictedAccess = true;
                    }

                    return false;
                }

            } catch (Exception e) {
                return false;
            }

        }

        @Override
        protected void onPostExecute(final Boolean success) {
            if (success) {
                Intent intent = new Intent(LoginActivity.this, MainActivity.class );
                LoginActivity.this.startActivity(intent);
            } else {

                if(restrictedAccess)
                    mPasswordView.setError(getString(R.string.error_restricted_access));
                else {
                    mPasswordView.setError(getString(R.string.error_incorrect_password));
                }

                mPasswordView.requestFocus();
            }
        }

        private String md5(final String s) {
            final String MD5 = "MD5";
            try {
                // Create MD5 Hash
                MessageDigest digest = java.security.MessageDigest.getInstance(MD5);
                digest.update(s.getBytes());
                byte messageDigest[] = digest.digest();

                // Create Hex String
                StringBuilder hexString = new StringBuilder();
                for (byte aMessageDigest : messageDigest) {
                    StringBuilder h = new StringBuilder(Integer.toHexString(0xFF & aMessageDigest));
                    while (h.length() < 2) {
                        h.insert(0, "0");
                    }
                    hexString.append(h);
                }
                return hexString.toString();

            } catch (NoSuchAlgorithmException e) {
                e.printStackTrace();
            }
            return "";
        }
    }
}

