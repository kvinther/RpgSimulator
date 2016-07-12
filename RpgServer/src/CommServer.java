import java.io.IOException;
import java.net.*;
import java.util.HashMap;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;

public class CommServer {

	private static final String TAG = "CommServer";

	GameServer gameServer;
	HashMap<Integer,ConnectionHandler> connections;
	ExecutorService exec;
	volatile int connectCount;

	protected ServerSocket serverSocket;

	// Handles communication between gameserver and clients
	public CommServer(GameServer gameServer) {
		System.out.println(TAG + ": Started");
		this.gameServer = gameServer;

		connections = new HashMap<>();
		exec = Executors.newWorkStealingPool();
		connectCount = 0;

		try {
			serverSocket = new ServerSocket(0);
		} catch (IOException e) {
			e.printStackTrace();
		}

		exec.execute(new DiscoveryThread());
		exec.execute(new ConnectThread(this));
	}

	public void sendMessage(String msg, int id) {
		connections.get(id).send(msg);
	}

	public synchronized void receiveMessage(String msg) {
		try {
			gameServer.msgQueue.put(msg);
		} catch (InterruptedException e) {
			e.printStackTrace();
		}
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

				while(true) {
					// Try to receive UDP packet
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
				}
			} catch (IOException e) {
				e.printStackTrace();
			}
		}
	}

	// Listens for connection requests and spawns ConnectionHandler threads
	private class ConnectThread implements Runnable {

		CommServer commServer;

		public ConnectThread(CommServer commServer) {
			this.commServer = commServer;
		}

		@Override
		public void run() {
			System.out.println(TAG + ": Listening for connections");

			while(true) {
				try {
					Socket socket = serverSocket.accept();
					ConnectionHandler connectionHandler = new ConnectionHandler(socket,commServer,connectCount);
					connections.put(connectCount++, connectionHandler);
					exec.execute(connectionHandler);
				} catch (IOException e) {
					e.printStackTrace();
				}
			}
		}
	}
}