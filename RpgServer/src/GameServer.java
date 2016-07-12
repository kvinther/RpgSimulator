import java.util.HashMap;
import java.util.concurrent.BlockingQueue;
import java.util.concurrent.LinkedBlockingQueue;

public class GameServer {

	private static final String TAG = "GameServer";
	
	HashMap<Integer,Integer> state;
	int totalSum;
	BlockingQueue<String> msgQueue;
	CommServer commServer;

	public GameServer() {
		System.out.println(TAG + ": Started");
		state = new HashMap<Integer,Integer>();
		totalSum = 0;
		msgQueue = new LinkedBlockingQueue<String>();
		commServer = new CommServer(this);

		while(true) {
			try {
                String msg = msgQueue.take();
                updateState(msg);
            } catch (InterruptedException e) {
                e.printStackTrace();
            }
		}
	}

	public static void main(String[] args) {
		GameServer server = new GameServer();
	}

	public void updateState(String msg) {
		String[] arr = msg.split(" ");
		int id = Integer.parseInt(arr[0]);
		String time = msg.substring(arr[0].length() + 1);

		totalSum++;
		String response = id + " ";

		if(state.containsKey(id)) {
			int mySum = state.get(id) + 1;
			state.put(id,mySum);
			response += mySum + " " + totalSum;
		} else {
			state.put(id,1);
			response += 1 + " " + totalSum;
		}

		commServer.sendMessage(response, id);
	}	
}