import { configureStore } from "@reduxjs/toolkit";
import authReducer from "./authSlice";
import ticketReducer from "./ticketSlice";
import userReducer from "./userSlice";

export const store = configureStore({
  reducer: {
    auth: authReducer,
    tickets: ticketReducer,
    users: userReducer,
    // سایر reducerهای شما اینجا اضافه می‌شوند
  },
  // تنظیمات دیگر (مثل middleware برای logging یا devTools) می‌تواند اینجا اضافه شود
  // devTools: process.env.NODE_ENV !== 'production', // فعال‌سازی DevTools در حالت development (پیش‌فرض)
});
