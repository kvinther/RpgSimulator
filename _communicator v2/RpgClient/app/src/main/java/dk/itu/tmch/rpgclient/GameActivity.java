package dk.itu.tmch.rpgclient;

import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.TextView;

import java.util.UUID;

public class GameActivity extends AppCompatActivity {

    volatile Thread mCommClientThread;
    volatile CommClient mCommClient;

    String mIp;
    int mPort;

    String id;
    boolean connected;

    Button mSendButton;
    TextView mId;
    TextView mMessage;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_game);

        mIp = getIntent().getStringExtra("IP");
        mPort = getIntent().getIntExtra("PORT", 0);

        if (savedInstanceState != null) {
            //if (savedInstanceState.getString("ID") != null) {
                id = savedInstanceState.getString("ID");
            //} else {
            //    id = UUID.randomUUID().toString();
            //}

            connected = savedInstanceState.getBoolean("CONNECTED");
        } else {
            id = UUID.randomUUID().toString();
            connected = false;
        }

        mSendButton = (Button) findViewById(R.id.send);
        mSendButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                String msg = UUID.randomUUID().toString().substring(0,8);
                mCommClient.sendMsg(msg);
            }
        });

        mId = (TextView) findViewById(R.id.client_id);
        mId.setText("ID: " + id);

        mMessage = (TextView) findViewById(R.id.message);
    }

    @Override
    public void onResume(){
        super.onResume();

        mCommClient = new CommClient(mIp,mPort,id,this);
        mCommClientThread = new Thread(mCommClient);
        mCommClientThread.start();
    }

    public synchronized void stopCommClient() {
        mCommClientThread.interrupt();
    }

    public synchronized void setConnected(boolean isConnected) {
        connected = isConnected;
    }

    public synchronized boolean isConnected() {
        return connected;
    }

    public synchronized String getId() {
        return id;
    }

    public synchronized void setId(String id) {
        this.id = id;
    }

    public void receive(String msg) {
        // TODO update UI based on received message
        String[] msgs = msg.split(" ");
        mMessage.setText("From ID: " + msgs[0] + "\nMessage: " + msgs[1]);
    }

    @Override
    protected void onSaveInstanceState(Bundle bundle) {
        super.onSaveInstanceState(bundle);
        bundle.putString("ID", id);
        bundle.putBoolean("CONNECTED",connected);
    }

    @Override
    protected void onDestroy() {
        super.onDestroy();
        mCommClient.disconnect();
    }
}
