package dk.itu.tmch.rpgclient;

import android.content.Intent;
import android.os.AsyncTask;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.Button;

import java.io.IOException;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;
import java.net.InterfaceAddress;
import java.net.NetworkInterface;
import java.util.Enumeration;

public class MainActivity extends AppCompatActivity {

    String mIp = "10.26.39.80";
    int mPort = 54311;

    Button mConnectButton;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        mConnectButton = (Button) findViewById(R.id.connect_button);
        //mConnectButton.setEnabled(false);
        mConnectButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                // Connect ofc!
                Intent i = new Intent(MainActivity.this, GameActivity.class);
                i.putExtra("IP",mIp);
                i.putExtra("PORT",mPort);
                startActivity(i);
            }
        });

        // UDP multicast gets server info
        new Search().execute();
    }

    public void updateServerInfo(String ip, int port) {
        mIp = ip;
        mPort = port;
        mConnectButton.setEnabled(true);
    }

    private class Search extends AsyncTask {

        private static final String TAG = "SearchAsyncTask";

        String ip;
        int port;
        boolean mServerFound = false;

        @Override
        protected Object doInBackground(Object[] params) {

            try {
                DatagramSocket c = new DatagramSocket();
                c.setBroadcast(true);

                byte[] sendData = "COC_SERVER_DISCOVERY_REQUEST".getBytes();

                // Try the 255.255.255.255 first (why tho??)
                try {
                    DatagramPacket sendPacket = new DatagramPacket(sendData, sendData.length, InetAddress.getByName("255.255.255.255"), 8888);
                    c.send(sendPacket);
                    Log.i(TAG, "Request packet sent to: 255.255.255.255 (DEFAULT)");
                } catch (IOException e) {
                    Log.i(TAG, e.toString());
                }

                // Broadcast the message over all the network interfaces
                Enumeration interfaces = NetworkInterface.getNetworkInterfaces();
                while (interfaces.hasMoreElements()) {
                    NetworkInterface networkInterface = (NetworkInterface) interfaces.nextElement();
                    if (networkInterface.isLoopback() || !networkInterface.isUp()) {
                        // Don't want to broadcast to the loopback interface
                        continue;
                    }

                    for (InterfaceAddress interfaceAddress : networkInterface.getInterfaceAddresses()) {
                        InetAddress broadcast = interfaceAddress.getBroadcast();
                        if (broadcast == null) {
                            continue;
                        }

                        // Send the broadcast package
                        try {
                            DatagramPacket sendPacket = new DatagramPacket(sendData, sendData.length, broadcast, 8888);
                            c.send(sendPacket);
                        } catch (IOException e) {
                            Log.i(TAG, e.toString());
                        }

                        Log.i(TAG, "Request packet sent to: " + broadcast.getHostAddress() + "; Interface: " + networkInterface.getDisplayName());
                    }
                }

                Log.i(TAG, "Done looping over all network interfaces. Now waiting for a reply");

                // Wait for a response
                byte[] receiveBuffer = new byte[15000];
                DatagramPacket receivePacket = new DatagramPacket(receiveBuffer, receiveBuffer.length);
                c.receive(receivePacket);

                // We get signal
                Log.i(TAG, "Broadcast response from server: " + receivePacket.getAddress().getHostAddress());

                // Check if message is correct
                String message = new String(receivePacket.getData()).trim();
                String[] arr = message.split(" ");
                if (arr[0].equals("COC_SERVER_DISCOVERY_RESPONSE")) {
                    mServerFound = true;
                    ip = receivePacket.getAddress().toString().substring(1);
                    port = Integer.parseInt(arr[1]);
                }

                Log.i(TAG, "Response from server: " + message);

                // Close port
                c.close();

            } catch (IOException e) {
                Log.i(TAG, e.toString());
            }

            return null;
        }

        @Override
        protected void onPostExecute(Object o) {
            if (mServerFound) {
                updateServerInfo(ip, port);
            }
        }
    }
}
