import java.io.DataInputStream;
import java.io.DataOutputStream;
import java.io.IOException;
import java.net.Socket;

public class ConnectionHandler implements Runnable {
	
	private static final String TAG = "ConnectionHandler";

	Socket socket;
	CommServer commServer;
	DataInputStream in;
	DataOutputStream out;

	int id;

	public ConnectionHandler(Socket socket, CommServer commServer, int id) {
		this.socket = socket;
		this.commServer = commServer;
		this.id = id;

		try {
			in = new DataInputStream(socket.getInputStream());
			out = new DataOutputStream(socket.getOutputStream());
		} catch (IOException e) {
			e.printStackTrace();
		}
	}

	@Override
	public void run() {
		System.out.println(TAG + ": Started");

		// Sending connection id number to client
		try {
            String msg =  "" + id;
			out.writeUTF(msg);
			out.flush();
		} catch (IOException e) {
			e.printStackTrace();
		}

		// Listening for messages from client
		while(true) {
			try {
				String msg = in.readUTF();
				System.out.println(TAG + ": Message received");
				commServer.receiveMessage(msg);
			} catch (IOException e) {
				e.printStackTrace();
				break;
			}
		}
	}	

	public void send(String msg) {
		System.out.println(TAG + ": Sending message via worker thread");
		commServer.exec.execute(new Sender(msg));
	}

	private class Sender implements Runnable {
		String msg;

		public Sender(String msg) {
			this.msg = msg;
		}

		@Override
		public void run() {
			try {
                out.writeUTF(msg);
                out.flush();
            } catch (IOException e) {
                e.printStackTrace();
            }

		}
	}
}