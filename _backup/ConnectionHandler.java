
import java.net.*;
import java.io.*;

/**
 * Created by Tom on 30-06-2016.
 */
public class ConnectionHandler implements Runnable {

    private Socket socket;
    private DataInputStream in;
    private DataOutputStream out;

    private CommServer server;

    private int connectionId;

    public ConnectionHandler(Socket socket, CommServer server, int connectionId) throws IOException {
        this.socket = socket;
        in = new DataInputStream(socket.getInputStream());
        out = new DataOutputStream(socket.getOutputStream());
        this.server = server;
        this.connectionId = connectionId;
    }

    public void sendMessage(final String message) {

        try {
            out.writeUTF(message);
            out.flush();
        } catch (Exception e) {
            System.out.println("Failed to send message: " + message);
            e.printStackTrace();
        }
    }

    public void receiveMessages() {
        while(true) {
            try {
                String message = in.readUTF();
                server.handleMessage(message);
            } catch (Exception e) {
                System.out.println("Could not receive message");
                e.printStackTrace();
            }
        }
    }

    public void closeConnection() throws IOException {
        try {
            in.close();
            socket.close();
        } catch (Exception e) {
            System.out.println("Could not close connection");
            e.printStackTrace();
        }
    }

    @Override
    public void run() {
        receiveMessages();
    }
}
