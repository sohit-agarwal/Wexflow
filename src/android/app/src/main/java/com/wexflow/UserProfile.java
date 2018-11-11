package com.wexflow;

enum UserProfile {
    Administrator,
    Restricted;

    public static UserProfile fromInteger(int x) {
        switch (x) {
            case 0:
                return Administrator;
            case 1:
                return Restricted;
            default:
                return null;
        }
    }
}
