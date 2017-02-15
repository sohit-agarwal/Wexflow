package com.wexflow;

enum LaunchType {
    Startup,
    Trigger,
    Periodic;

    public static LaunchType fromInteger(int x) {
        switch (x) {
            case 0:
                return Startup;
            case 1:
                return Trigger;
            case 2:
                return Periodic;
        }
        return null;
    }
}
