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

	String connectionId;

	public ConnectionHandler(Socket socket, CommServer commServer) {
		this.socket = socket;
		this.commServer = commServer;

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

        // Expect first message from client to contain CONNECT or RECONNECT message
        try {
            String msg = in.readUTF();
            String[] msgArr = msg.split(" ");

            if (msgArr[1].equals("CONNECT") || msgArr[1].equals("RECONNECT")) {
                connectionId = msgArr[0];
                commServer.associateConnectionWithPlayer(this);
            } else {
                throw new IOException("First message from client was not CONNECT or RECONNECT");
            }

        } catch (IOException e) {
            System.out.println(TAG + ": " + e.toString());
        }

		// Listening for subsequent messages from client
		while(true) {
			try {
				String msg = in.readUTF();
				System.out.println(TAG + ": Message received");

				String[] msgArr = msg.split(" ");

                // Sanity check: ignores message if sender ID differs from stored ID of handler
                if (connectionId != null && !connectionId.equals(msgArr[0])) {
                    System.out.println(TAG + ": received message from wrong sender?!");
                    continue;
                }

                switch (msgArr[1]) {
                    case "DISCONNECT":
                        // TODO: trigger disassociation of PLAYER object and this connection, kill handler

                        break;
                    case "FIVEBYFIVE":
                        // TODO: notify server that thread lives? Or do nothing, I guess?

                        break;
                    case "MESSAGE":
                        // Actual message from client
                        String msgData = getDataFromMsg(msg, msgArr);
                        commServer.receiveMessage(msgData);

                        break;
                    case "DESTROYCONNECTION":
                        // TODO: kill player object? I guess?
                        break;
                }




				commServer.receiveMessage(msg);
			} catch (IOException e) {
				e.printStackTrace();
				break;
			}
		}
	}

    public String getDataFromMsg(String msg, String[] msgArr) {
        return msg.substring(msgArr[0].length() + msgArr[1].length() + 2);
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