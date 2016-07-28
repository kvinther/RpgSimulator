package cthulhu.of.call.rpgclient;

import android.content.Context;
import android.util.Log;
import android.widget.Toast;

import java.io.DataOutputStream;
import java.io.IOException;

/**
 * Created by Tom on 12-07-2016.
 */
public class Sender implements Runnable {

    private static final String TAG = "Sender";

    private final String error;
    private final Context context;
    private final String msg;
    private final DataOutputStream out;

    public Sender(String msg, String error, Context context, DataOutputStream out) {
        this.msg = msg;
        this.error = error;
        this.context = context;
        this.out = out;
    }

    @Override
    public void run() {
        try {
            out.writeUTF(msg);
            out.flush();
        } catch (IOException e) {
            Log.i(TAG, e.toString());
            Toast.makeText(context, error, Toast.LENGTH_SHORT).show();
        }
    }
}
