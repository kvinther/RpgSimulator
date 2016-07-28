package dk.itu.tmch.rpgclient;

import android.content.Context;
import android.net.wifi.WifiInfo;
import android.net.wifi.WifiManager;
import android.util.Log;
import android.widget.Toast;

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

    String ip;
    int port;
    String id;

    GameActivity mGameActivity;

    Socket socket;
    DataInputStream in;
    DataOutputStream out;

    boolean receiving = true;

    public CommClient(String ip, int port, String id, GameActivity gameActivity) {
        this.ip = ip;
        this.port = port;
        this.id = id;
        mGameActivity = gameActivity;
    }

    @Override
    public void run() {
        try {
            socket = new Socket(ip,port);
            in = new DataInputStream(socket.getInputStream());
            out = new DataOutputStream(socket.getOutputStream());

            Log.i(TAG,"set up sockets and streams");

            if(!mGameActivity.isConnected()) {
                out.writeUTF("CONNECT " + mGameActivity.getId());
                out.flush();
            } else {
                out.writeUTF("RECONNECT " + mGameActivity.getId());
                out.flush();
            }

            mGameActivity.setConnected(true);
            Log.i(TAG,"setting connected to:" + mGameActivity.isConnected());

            try {
                while(receiving) {
                    final String msg = in.readUTF();
                    final String flag = msg.split(" ")[0];

                    if (flag.equals("MESSAGE")) {
                        Log.i(TAG, "Message received: " + msg);
                        mGameActivity.runOnUiThread(new Runnable() {
                            public void run() {
                                mGameActivity.receive(msg.substring(flag.length() + 1));
                            }
                        });
                    } else if(flag.equals("SHUTDOWN")) {
                        quitGame();
                        Log.i(TAG, "Shutdown message received from server, shutting down client");
                    } else {
                        Log.e(TAG, "Malformed message received: " + flag);
                    }
                }
            } catch (IOException e) {
                Log.e(TAG, "Failed to receive message from server");
            }

        } catch (IOException e) {
            Log.e(TAG,"Failed to connect to server");
            mGameActivity.setConnected(false);
        }
    }

    public void disconnect() {
        send("DISCONNECT");
        receiving = false;
        mGameActivity.stopCommClient();
    }

    public void quitGame() {
        send("QUIT");
        receiving = false;
        mGameActivity.stopCommClient();
        mGameActivity.finish();
    }

    public void sendMsg(String msg) {
        send("MESSAGE " + id + " " + msg);
    }

    public void send(String msg) {
        new Thread(new Sender(msg)).start();
    }

    private class Sender implements Runnable {

        private final String msg;

        public Sender(String msg) {
            this.msg = msg;
        }

        @Override
        public void run() {
            try {
                out.writeUTF(msg);
                out.flush();
            } catch (IOException e) {

            }
        }
    }

    private class ShutDownSender implements Runnable {

        @Override
        public void run() {
            // TODO
        }
    }
}
