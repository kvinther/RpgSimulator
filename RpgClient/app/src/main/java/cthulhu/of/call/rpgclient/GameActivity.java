package cthulhu.of.call.rpgclient;

import android.os.AsyncTask;
import android.os.Parcelable;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.TextView;

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
    int connectionId;

    int mySum;
    int totalSum;

    CommClient commClient;

    Button mPlusButton;
    TextView mMySum;
    TextView mTotalSum;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_game);

        mIp = getIntent().getStringExtra("IP");
        mPort = getIntent().getIntExtra("PORT", 0);
        Log.i(TAG,"IP:" + mIp + " - Port:" + mPort);

        if (savedInstanceState != null) {
            mySum = savedInstanceState.getInt("MYSUM");
            totalSum = savedInstanceState.getInt("TOTALSUM");
        }

        commClient = new CommClient(mIp,mPort,this);
        new Thread(commClient).start();

        mPlusButton = (Button) findViewById(R.id.plus_button);
        mPlusButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                Log.i(TAG,"Sending message");
                String msg = new Date().toString();
                commClient.send(msg);
            }
        });

        mMySum = (TextView) findViewById(R.id.my_sum);
        mTotalSum = (TextView) findViewById(R.id.total_sum);
    }

    public void receive(String receivedMsg) {
        String[] msg = receivedMsg.split(" ");
        if (msg.length > 1) {
            String id = msg[0];
            String my = msg[1];
            String total = msg[2];

            mTotalSum.setText(total);
            mMySum.setText(my);

            mySum = Integer.parseInt(my);
            totalSum = Integer.parseInt(total);

        } else {
            connectionId =  Integer.parseInt(msg[0]);
            Log.i(TAG, "Setting connection ID: " + connectionId);
        }
    }

    @Override
    protected void onSaveInstanceState(Bundle outState) {
        super.onSaveInstanceState(outState);
        outState.putInt("MYSUM", mySum);
        outState.putInt("TOTALSUM", totalSum);
    }


}
