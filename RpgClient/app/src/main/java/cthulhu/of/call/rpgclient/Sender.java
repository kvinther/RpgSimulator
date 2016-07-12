package cthulhu.of.call.rpgclient;

import android.util.Log;

import java.io.DataOutputStream;
import java.io.IOException;

/**
 * Created by Tom on 12-07-2016.
 */
public class Sender implements Runnable {

    private static final String TAG = "Sender";

    String msg;
    DataOutputStream out;

    public Sender(String msg, DataOutputStream out) {
        this.msg = msg;
        this.out = out;
    }

    @Override
    public void run() {
        try {
            out.writeUTF(msg);
            out.flush();
        } catch (IOException e) {
            Log.i(TAG, e.toString());
        }
    }
}
