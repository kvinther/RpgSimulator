import java.util.UUID;

/**
 * Created by Tom on 21-07-2016.
 */
public class Player {

    private UUID id;
    private String playerName;
    private boolean connected;
    private CommServer commServer;

    public Player(CommServer commServer) {
        this.commServer = commServer;
    }

    public void setConnected(boolean status) {
        connected = status;
    }

    public boolean isConnected() {
        return connected;
    }
}
