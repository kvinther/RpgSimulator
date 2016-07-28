import java.util.HashMap;
import java.util.concurrent.BlockingQueue;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.LinkedBlockingQueue;

public class GameServer {

	private static final String TAG = "GameServer";
	
	HashMap<Integer,Integer> state;

	HashMap<String,Player> players;
	int totalSum;
	BlockingQueue<String> msgQueue;
	CommServer commServer;

	public GameServer() {
		System.out.println(TAG + ": Started");
		state = new HashMap<Integer,Integer>();
		players = new HashMap<>();
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

    public void addPlayer(Player player) {
		Runnable task = new Runnable() {
			@Override
			public void run() {

			}
		};

		ExecutorService exec = Executors.newWorkStealingPool();

		Future<boolean> fut = exec.submit(task);
    }

	public void updateState(String msg) {
		String[] arr = msg.split(" ");
		int id = Integer.parseInt(arr[0]);

		totalSum++;
		if(state.containsKey(id)) {
			state.put(id,state.get(id)+1);
		} else {
			state.put(id, 1);
		}

		sendStateToAll();
	}

    public synchronized void sendState(int id) {
		String response;
		if(state.containsKey(id)) {
			response = id + " " + state.get(id) + " " + totalSum;
		} else {
			response = id + " 0 " + totalSum;
		}

        commServer.sendMessage(response, id);
    }

	public void sendStateToAll() {
		for(int id : state.keySet()) {
			sendState(id);
		}
	}
}