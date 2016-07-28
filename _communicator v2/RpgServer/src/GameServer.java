import java.util.HashMap;

/**
 * Created by Tom on 21-07-2016.
 */
public class GameServer {

    private int maxPlayers = 4;
    private HashMap<String,Player> players;
    private boolean acceptingPlayers = true;
    private CommServer commServer;

    public GameServer() {
        this.commServer = new CommServer(this);
        players = new HashMap<>();
    }

    public static void main(String[] args) {
        GameServer gameServer = new GameServer();
    }

    public void receive(String id, String msg) {
        // TODO Game logic stuff
        System.out.println("Client ID:" + id + " sends his regards");
    }

    public void send(String id, String msg) {
        commServer.send(id,msg);
    }

    public void startAcceptingPlayers() {
        commServer.startAccepting();
    }

    public void stopAcceptingPlayers() {
        commServer.stopAccepting();
    }

    synchronized boolean canConnect(String id) {
        return players.size() < maxPlayers;
    }

    synchronized boolean canReconnect(String id) {
        return players.containsKey(id);
    }

    public synchronized void registerHandler(String id) {
        if (!players.containsKey(id)) {
            Player player = new Player(commServer);
            player.setConnected(true);
            players.put(id,player);

            if (players.size() >= maxPlayers) {
                stopAcceptingPlayers();
            }
        } else {
            Player player = players.get(id);
            player.setConnected(true);
        }
    }

    public synchronized void quitGame(String id) {
        players.remove(id);
        if (!commServer.isAccepting()) {
            startAcceptingPlayers();
        }
    }

    public synchronized void setPlayerDisconnected(String id) {
        if (players.containsKey(id)) {
            players.get(id).setConnected(false);
        }
    }
}
