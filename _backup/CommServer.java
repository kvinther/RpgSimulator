import java.net.ServerSocket;
import java.net.Socket;
import java.util.HashMap;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;

/**
 * Created by Tom on 30-06-2016.
 */
public class CommServer implements Runnable {

    GameServer gameServer;
    HashMap<Integer,ConnectionHandler> connections;
    protected static ExecutorService exec;
    ServerSocket serverSocket;

    public CommServer(GameServer gameServer) {
        this.gameServer = gameServer;
        connections = new HashMap<>();
        exec = Executors.newWorkStealingPool();

        try {
            serverSocket = new ServerSocket(3333);
        } catch (Exception e) {
            System.out.println("Could not create ServerSocket");
            e.printStackTrace();
        }
    }

    public void run() {
        try {
            while(true) {
                Socket socket = serverSocket.accept();
                ConnectionHandler connection = new ConnectionHandler(socket, this, connections.size());
                connections.put(connections.size(),connection);
                exec.execute(connection);
                System.out.println("Handler started");
            }
        } catch (Exception e) {
            System.out.println("Could not accept client connect request");
            e.printStackTrace();
        }
    }

    public void sendMessage(int playerId, String message) {
        ConnectionHandler connection = connections.get(playerId);
        if (connection != null) {
            // executes sendMessage on worker thread in execution pool
            exec.execute(new Runnable() {
                public void run() {
                    connection.sendMessage(message);
                }
            });
        } else {
            System.out.println("No connectionHandler with ID: " + playerId);
        }
    }

    public void sendMessageToAll(String message) {
        for(int id : connections.keySet()) {
            sendMessage(id, message);
        }
    }

    public synchronized void handleMessage(String message) {
        gameServer.receiveMessage(message);
    }
}
