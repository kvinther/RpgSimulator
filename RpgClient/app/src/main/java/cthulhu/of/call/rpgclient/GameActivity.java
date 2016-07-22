package cthulhu.of.call.rpgclient;

import android.content.Context;
import android.os.AsyncTask;
import android.os.Parcelable;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.TextView;
import android.widget.Toast;

import org.w3c.dom.Text;

import java.io.DataInput;
import java.io.DataInputStream;
import java.io.DataOutputStream;
import java.io.IOException;
import java.io.Serializable;
import java.io.SerializablePermission;
import java.net.Socket;
import java.util.Date;

public class GameActivity extends AppCompatActivity {

    private static final String TAG = "GameActivity";

    String mIp;
    int mPort;

    int mySum = 0;
    int totalSum = 0;
    private String playerInfo;

    CommClient commClient;
    Thread commClientThread;

    Button mPlusButton;
    TextView mMySum;
    TextView mTotalSum;
    TextView mPlayerInfo;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_game);

        mIp = getIntent().getStringExtra("IP");
        mPort = getIntent().getIntExtra("PORT", 0);
        if (savedInstanceState != null) {
            mySum = savedInstanceState.getInt("MYSUM");
            totalSum = savedInstanceState.getInt("TOTALSUM");
            playerInfo = savedInstanceState.getString("PLAYERINFO");
        }

        // Creating a commClient, a thread for it to run in, and running it
        commClient = new CommClient(mIp,mPort,this);
        commClientThread = new Thread(commClient);
        commClientThread.start();

        mPlusButton = (Button) findViewById(R.id.plus_button);
        mPlusButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                Log.i(TAG,"Sending message");
                commClient.send("MESSAGE","Failed to send state update message");
            }
        });

        mMySum = (TextView) findViewById(R.id.my_sum);
        mTotalSum = (TextView) findViewById(R.id.total_sum);
        mPlayerInfo = (TextView) findViewById(R.id.player_info);

        updateUI();
    }

    public void receive(String receivedMsg) {
        Log.i(TAG, receivedMsg);

        String[] msg = receivedMsg.split(" ");
        mySum = Integer.parseInt(msg[0]);
        totalSum = Integer.parseInt(msg[1]);

        updateUI();
    }

    public void updateUI() {
        mMySum.setText("My sum: " + mySum);
        mTotalSum.setText("Total: " + totalSum);
        mPlayerInfo.setText("Player " + playerInfo);
    }

    public void killActivity(String toastMsg) {
        Toast toast = Toast.makeText(this, toastMsg, Toast.LENGTH_SHORT);
        toast.show();
        finish();
    }

    @Override
    public void onPause() {
        super.onPause();
        commClient.send("DISCONNECT","Failed to send disconnect message to server");
        // commClient.killConnection();
        commClientThread.interrupt();
    }

    @Override
    protected void onSaveInstanceState(Bundle outState) {
        super.onSaveInstanceState(outState);
        outState.putInt("MYSUM", mySum);
        outState.putInt("TOTALSUM", totalSum);
        outState.putString("PLAYERINFO", playerInfo);
    }

    public void setPlayerInfo(String playerInfo) {
        this.playerInfo = playerInfo;
    }

    public String getPlayerInfo() {
        return playerInfo;
    }
}
