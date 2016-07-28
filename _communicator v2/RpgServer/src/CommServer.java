import java.io.DataInputStream;
import java.io.DataOutputStream;
import java.io.IOException;
import java.net.*;
import java.util.HashMap;

/**
 * Created by Tom on 21-07-2016.
 */
public class CommServer {

    private ServerSocket serverSocket;
    private GameServer gameServer;

    private Thread discoveryThread;
    private Thread acceptThread;
    private HashMap<String,Thread> connectionThreads;

    private HashMap<String,ConnectionHandler> connectionHandlers;


    public CommServer(GameServer gameServer) {
        this.gameServer = gameServer;
        connectionThreads = new HashMap<>();
        connectionHandlers = new HashMap<>();

        startDiscoveryService();
        startAccepting();
    }

    private void startDiscoveryService() {
        discoveryThread = new Thread(new DiscoveryThread());
        discoveryThread.start();
    }

    public void startAccepting() {
        acceptThread = new Thread(new AcceptThread());
        acceptThread.start();
    }

    public void stopAccepting() {
        acceptThread.interrupt();
    }

    public boolean isAccepting() {
        return acceptThread != null && !discoveryThread.isInterrupted();
    }

    private synchronized void receive(String id, String msg) {
        gameServer.receive(id, msg);
    }

    public void send(String id, String msg){
        ConnectionHandler handler = connectionHandlers.get(id);
        handler.send("MESSAGE " + msg);
    }

    // Listens for and responds to UDP traffic
    private class DiscoveryThread implements Runnable {

        private static final String TAG = "DiscoveryThread";
        private static final String request  = "COC_SERVER_DISCOVERY_REQUEST";
        private static final String response = "COC_SERVER_DISCOVERY_RESPONSE";

        @Override
        public void run() {
            try {
                DatagramSocket socket = new DatagramSocket(8888, InetAddress.getByName("0.0.0.0"));
                socket.setBroadcast(true);

                System.out.println(TAG + ": Ready to respond to broadcasts");

                boolean running = true;
                while(running) {
                    // Try to receive UDP packet
                    try {
                        byte[] receiveBuffer = new byte[1024];
                        DatagramPacket packet = new DatagramPacket(receiveBuffer, receiveBuffer.length);
                        socket.receive(packet);

                        System.out.println(TAG + ": Packet received from " + packet.getAddress());

                        // check if packet contents has correct format
                        String msg = new String(packet.getData()).trim();
                        if (msg.equals(request)) {
                            // Respond with server IP and port numbers
                            byte[] sendData = (response + " " + serverSocket.getLocalPort()).getBytes();
                            DatagramPacket responsePacket = new DatagramPacket(sendData, sendData.length, packet.getAddress(), packet.getPort());
                            socket.send(responsePacket);

                            System.out.println(TAG + ": Response sent to " + packet.getAddress());
                        } else {
                            System.out.println(TAG + ": Packet did not contain discovery request");
                        }

                    } catch (IOException e) {
                        // failed to receive or send on socket
                        running = false;
                    }

                }

            } catch (IOException e) {
                // failed to create socket
                e.printStackTrace();
            }
        }
    }

    // Listens for connection requests and spawns ConnectionHandler threads
    private class AcceptThread implements Runnable {

        private final CommServer commServer;
        private boolean running;

        AcceptThread() {
            commServer = CommServer.this;
            running = true;
        }

        @Override
        public void run() {
            System.out.println("AcceptThread running");
            try {
                serverSocket = new ServerSocket(54311);
            } catch (IOException e) {

            }
            while(running) {
                try {

                    System.out.println("IP:" + InetAddress.getLocalHost().getHostAddress() + " PORT:" + serverSocket.getLocalPort());
                    Socket socket = serverSocket.accept();
                    DataInputStream in = new DataInputStream(socket.getInputStream());
                    DataOutputStream out = new DataOutputStream(socket.getOutputStream());

                    // Handshake
                    String[] msg = in.readUTF().split(" ");
                    String id = msg[1];

                    if (msg[0].equals("CONNECT")) {
                        if (gameServer.canConnect(id)) {
                            createHandler(socket, in, out, id);
                            gameServer.registerHandler(id);
                        } else {
                            System.err.println("GameServer did not permit ID:" + id + " to connect to game");
                        }
                    } else if (msg[0].equals("RECONNECT")) {
                        // Kill thread and remove handler of previous connection
                        connectionHandlers.remove(id);
                        connectionThreads.get(id).interrupt();

                        if (gameServer.canReconnect(id)) {
                            createHandler(socket, in, out, id);
                            gameServer.registerHandler(id);
                        } else {
                            System.err.println("GameServer did not permit ID:" + id + " to reconnect to game");
                        }
                    } else {
                        System.err.println("Connection attempt failed: wrong message format");
                    }


                } catch (IOException e) {
                    e.printStackTrace();
                }
            }
        }

        void createHandler(Socket socket, DataInputStream in, DataOutputStream out, String id) {
            // create handler and store reference under id
            ConnectionHandler handler = new ConnectionHandler(socket,in,out,id);
            connectionHandlers.put(id,handler);

            // create thread, store reference and run handler on it
            Thread thread = new Thread(handler);
            connectionThreads.put(id,thread);
            thread.start();
        }

        public synchronized void setRunning(boolean running) {
            this.running = running;
        }
    }

    private class ConnectionHandler implements Runnable {

        private final Socket socket;
        private final DataInputStream in;
        private final DataOutputStream out;
        private final String id;
        private final CommServer commServer;

        ConnectionHandler(Socket socket, DataInputStream in, DataOutputStream out, String id) {
            this.socket = socket;
            this.in = in;
            this.out = out;
            this.id = id;
            commServer = CommServer.this;
        }

        @Override
        public void run() {
            System.out.println("ConnectionHandler running for ID:" + id);

            boolean running = true;
            while(running) {
                try {
                    String[] msg = in.readUTF().split(" ");

                    if(msg[0].equals("MESSAGE")) {
                        commServer.receive(msg[1], msg[0]);
                    } else if(msg[0].equals("DISCONNECT")) {
                        running = false;
                    } else {
                        System.err.println("Received message of unknown format: '" + msg[0] + "'");
                    }
                }  catch (IOException e) {
                    e.printStackTrace();
                    running = false;
                }
            }

            disconnect();
        }

        Thread disconnect() {
            System.out.println("Disconnecting ID:" + id);
            gameServer.setPlayerDisconnected(id);
            connectionHandlers.remove(id);
            return connectionThreads.remove(id);
        }

        public void send(String msg) {
            Thread thread = new Thread(new Sender(msg));
            thread.start();
        }

        public void shutDownConnection() {
            Thread thread = new Thread(new ShutDownSender());
            thread.start();
        }

        // Runnable used to send messages from ConnectionHandler to Client
        private class Sender implements Runnable {

            private final String msg;

            Sender(String msg) {
                this.msg = msg;
            }

            @Override
            public void run() {
                try {
                    out.writeUTF(msg);
                    out.flush();
                } catch (IOException e) {
                    System.err.println("Failed to send message");
                    e.printStackTrace();
                }
            }
        }

        // Runnable used to inform client this connection is being shut down
        private class ShutDownSender implements Runnable {

            @Override
            public void run() {
                try {
                    out.writeUTF("SHUTDOWN");
                    out.flush();
                    disconnect().interrupt();
                } catch (IOException e) {
                    System.err.println("Failed to send shutdown message");
                    e.printStackTrace();
                }
            }
        }
    }
}
