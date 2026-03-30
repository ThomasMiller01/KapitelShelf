package com.kapitelshelf.app;

import android.content.pm.ActivityInfo;
import android.content.res.Configuration;
import android.view.Surface;
import com.getcapacitor.Plugin;
import com.getcapacitor.PluginCall;
import com.getcapacitor.PluginMethod;
import com.getcapacitor.annotation.CapacitorPlugin;

@CapacitorPlugin(name = "ReaderOrientation")
public class ReaderOrientationPlugin extends Plugin {

    @PluginMethod
    public void restoreAppOrientation(PluginCall call) {
        getActivity().runOnUiThread(() -> {
            getActivity().setRequestedOrientation(ActivityInfo.SCREEN_ORIENTATION_PORTRAIT);
            call.resolve();
        });
    }

    @PluginMethod
    public void setReaderOrientationLocked(PluginCall call) {
        boolean locked = call.getBoolean("locked", false);

        getActivity().runOnUiThread(() -> {
            if (!locked) {
                getActivity().setRequestedOrientation(ActivityInfo.SCREEN_ORIENTATION_UNSPECIFIED);
                call.resolve();
                return;
            }

            int rotation = getActivity().getDisplay().getRotation();
            int orientation = getActivity().getResources().getConfiguration().orientation;
            getActivity().setRequestedOrientation(getRequestedOrientation(rotation, orientation));
            call.resolve();
        });
    }

    private int getRequestedOrientation(int rotation, int orientation) {
        if (orientation == Configuration.ORIENTATION_LANDSCAPE) {
            if (rotation == Surface.ROTATION_180 || rotation == Surface.ROTATION_270) {
                return ActivityInfo.SCREEN_ORIENTATION_REVERSE_LANDSCAPE;
            }

            return ActivityInfo.SCREEN_ORIENTATION_LANDSCAPE;
        }

        if (rotation == Surface.ROTATION_180 || rotation == Surface.ROTATION_270) {
            return ActivityInfo.SCREEN_ORIENTATION_REVERSE_PORTRAIT;
        }

        return ActivityInfo.SCREEN_ORIENTATION_PORTRAIT;
    }
}
