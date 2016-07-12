package cthulhu.of.call.rpgclient;

import android.util.Log;

import java.io.DataInputStream;
import java.io.DataOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.net.Socket;

/**
 * Created by Tom on 12-07-2016.
 */
public class CommClient implements Runnable {

    private static final String TAG = "CommClient";

    Socket socket;
    DataInputStream in;
    volatile DataOutputStream out;

    String mIp;
    int mPort;
    GameActivity activity;

    int connectionId = Integer.MIN_VALUE;

    public CommClient(String ip, int port, GameActivity gameActivity) {
        mIp = ip;
        mPort = port;
        activity = gameActivity;
        Log.i(TAG,"Started");
    }

    @Override
    public void run() {
        try {
            socket = new Socket(mIp,mPort);
            in = new DataInputStream(socket.getInputStream());
            out = new DataOutputStream(socket.getOutputStream());
            Log.i(TAG,"Sockets and streams ready");
            if (out != null) {
                Log.i(TAG,"(out is not null)");
            }
        } catch (IOException e) {
            Log.i(TAG,e.toString());
        }

        while (true) {
            Log.i(TAG,"Listening for messages...");
            try {
                final String msg = in.readUTF();

                if (msg.split(" ").length == 1) {
                    connectionId = Integer.parseInt(msg);
                }

                activity.runOnUiThread(new Runnable() {
                    @Override
                    public void run() {
                        activity.receive(msg);
                    }
                });
            } catch (IOException e) {
                Log.i(TAG,e.toString());
                break;
            }
        }
    }

    public void send(String msg) {
        msg = connectionId + " " + msg;
        new Thread(new Sender(msg,out)).start();
    }
}
