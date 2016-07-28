import java.util.Date;

public class CocMessage {

	final int senderId;
	final Date timeStamp;
	final String message;
	
	public CocMessage(int id, Date time, String msg) {
		senderId = id;
		timeStamp = time;
		message = msg;
	}

}