package com.kapitelshelf.app;

import android.content.Context;
import android.content.SharedPreferences;
import android.content.pm.ActivityInfo;
import android.content.res.Configuration;
import android.view.Surface;
import com.getcapacitor.Plugin;
import com.getcapacitor.PluginCall;
import com.getcapacitor.PluginMethod;
import com.getcapacitor.annotation.CapacitorPlugin;

@CapacitorPlugin(name = "ReaderOrientation")
public class ReaderOrientationPlugin extends Plugin {
    private static final String PREFERENCES_NAME = "kapitelshelf_reader_orientation";
    private static final String READER_ORIENTATION_KEY = "reader_orientation";

    @PluginMethod
    public void restoreAppOrientation(PluginCall call) {
        getActivity().runOnUiThread(() -> {
            getActivity().setRequestedOrientation(ActivityInfo.SCREEN_ORIENTATION_PORTRAIT);
            call.resolve();
        });
    }

    @PluginMethod
    public void lockCurrentReaderOrientation(PluginCall call) {
        getActivity().runOnUiThread(() -> {
            int requestedOrientation = getCurrentRequestedOrientation();
            saveReaderOrientation(requestedOrientation);
            getActivity().setRequestedOrientation(requestedOrientation);
            call.resolve();
        });
    }

    @PluginMethod
    public void restoreReaderOrientation(PluginCall call) {
        getActivity().runOnUiThread(() -> {
            Integer savedOrientation = getSavedReaderOrientation();
            if (savedOrientation == null) {
                int requestedOrientation = getCurrentRequestedOrientation();
                saveReaderOrientation(requestedOrientation);
                getActivity().setRequestedOrientation(requestedOrientation);
                call.resolve();
                return;
            }

            getActivity().setRequestedOrientation(savedOrientation);
            call.resolve();
        });
    }

    @PluginMethod
    public void unlockReaderOrientation(PluginCall call) {
        getActivity().runOnUiThread(() -> {
            clearSavedReaderOrientation();
            getActivity().setRequestedOrientation(ActivityInfo.SCREEN_ORIENTATION_UNSPECIFIED);
            call.resolve();
        });
    }

    private int getCurrentRequestedOrientation() {
        int rotation = getActivity().getDisplay().getRotation();
        int orientation = getActivity().getResources().getConfiguration().orientation;
        return getRequestedOrientation(rotation, orientation);
    }

    private SharedPreferences getPreferences() {
        return getContext().getSharedPreferences(PREFERENCES_NAME, Context.MODE_PRIVATE);
    }

    private Integer getSavedReaderOrientation() {
        SharedPreferences preferences = getPreferences();
        if (!preferences.contains(READER_ORIENTATION_KEY)) {
            return null;
        }

        return preferences.getInt(READER_ORIENTATION_KEY, ActivityInfo.SCREEN_ORIENTATION_UNSPECIFIED);
    }

    private void saveReaderOrientation(int orientation) {
        getPreferences().edit().putInt(READER_ORIENTATION_KEY, orientation).apply();
    }

    private void clearSavedReaderOrientation() {
        getPreferences().edit().remove(READER_ORIENTATION_KEY).apply();
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
