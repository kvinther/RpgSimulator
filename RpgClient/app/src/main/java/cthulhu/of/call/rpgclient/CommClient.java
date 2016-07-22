package cthulhu.of.call.rpgclient;

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

    Socket socket;
    DataInputStream in;
    volatile DataOutputStream out;

    String mIp;
    int mPort;
    GameActivity gameActivity;

    String connectionId;

    public CommClient(String ip, int port, GameActivity gameActivity) {
        mIp = ip;
        mPort = port;
        this.gameActivity = gameActivity;

        // Storing MAC address to use as message ID
        WifiManager manager = (WifiManager) gameActivity.getSystemService(Context.WIFI_SERVICE);
        WifiInfo info = manager.getConnectionInfo();
        connectionId = info.getMacAddress();

        Log.i(TAG,"Started");
    }

    @Override
    public void run() {
        try {
            socket = new Socket(mIp,mPort);
            in = new DataInputStream(socket.getInputStream());
            out = new DataOutputStream(socket.getOutputStream());
            Log.i(TAG,"Sockets and streams ready");
        } catch (IOException e) {
            Log.i(TAG,e.toString());
            gameActivity.killActivity("Failed to connect to server");
        }

        // Send connect acknowledgement to server
        send("CONNECT", "Failed to send connect acknowledgement to server");

        // Start listening for messages from server
        while (true) {
            Log.i(TAG,"Listening for messages...");
            try {
                String msg = in.readUTF();
                String[] msgArr = msg.split(" ");

                // Checks if message is sent to the right client
                if (!msgArr[0].equals(connectionId)) {
                    Toast.makeText(gameActivity.getApplicationContext(),
                            "Wrong message recipient?!",
                            Toast.LENGTH_SHORT).show();
                    break;
                }

                switch (msgArr[1]) {
                    case "MESSAGE": {
                        // Message from server
                        final String msgData = getDataFromMsg(msg, msgArr);
                        gameActivity.runOnUiThread(new Runnable() {
                            @Override
                            public void run() {
                                gameActivity.receive(msgData);
                            }
                        });

                        break;
                    }
                    case "STILLCONNECTED":
                        // Server checks status of connection
                        send("FIVEBYFIVE", "Failed to send connection status to server");

                        break;
                    case "PLAYERINFO": {
                        // Server informs client about player entity on server (name, number etc.)
                        final String msgData = getDataFromMsg(msg, msgArr);
                        gameActivity.runOnUiThread(new Runnable() {
                            @Override
                            public void run() {
                                gameActivity.setPlayerInfo(msgData);
                            }
                        });

                        break;
                    }
                    case "CLOSECONNECTION":
                        // Server sends message to close connection
                        killConnection();
                        gameActivity.killActivity("Connection closed by server");

                        break;
                    default:
                        // Could not identify type of message from server
                        Toast.makeText(gameActivity.getApplicationContext(),
                                "Unknown message type received",
                                Toast.LENGTH_SHORT).show();
                        break;
                }

            } catch (IOException e) {
                Log.i(TAG,e.toString());
                break;
            }
        }
    }

    public void killConnection() {
        try {
            in.close();
            out.close();
            socket.close();
            Log.i(TAG,"Connection closed");
        } catch (IOException e) {
            Log.i(TAG,e.toString());
        }
    }

    public String getDataFromMsg(String msg, String[] msgArr) {
        return msg.substring(msgArr[0].length() + msgArr[1].length() + 2);
    }

    public void send(String msg, String error) {
        msg = connectionId + " " + msg;
        new Thread(new Sender(msg,error,gameActivity.getApplicationContext(),out)).start();
    }
}
