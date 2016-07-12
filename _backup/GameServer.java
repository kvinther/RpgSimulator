import java.util.Date;

/**
 * Created by Tom on 04-07-2016.
 */
public class GameServer {

    CommServer commServer;

    public GameServer() {
        commServer = new CommServer(this);
        new RandomMessage(this).start();
    }

    public static void main(String[] args) {
        GameServer gameServer = new GameServer();
    }

    public void receiveMessage(String message) {
        // stick it in a queue, do game logic stuff, whatever

        // right now we just echo the message back to sender
        String[] parts = message.split(" ");
        int id = Integer.parseInt(parts[0]);
        String msg = parts[1];
        sendMessage(id,msg);

        System.out.println("ID: " + id + "   MSG: " + msg);
    }

    public void sendMessage(int id, String message) {
        commServer.sendMessage(id,message);
    }

    public void sendMessageToAll(String message) {
        commServer.sendMessageToAll(message);
    }

    private static class RandomMessage extends Thread {

        GameServer gameServer;

        public RandomMessage(GameServer gS) {
            gameServer = gS;
        }

        public void run() {
            while(true) {
                Date date = new Date();
                gameServer.sendMessageToAll(date.toString());
                try {
                    Thread.sleep(5000);
                } catch (InterruptedException e) {
                    e.printStackTrace();
                }
            }
        }
    }
}
