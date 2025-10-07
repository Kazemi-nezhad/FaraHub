// pages/_app.js
import "../styles/globals.css";
import { useRouter } from "next/router";
import { useEffect, useState } from "react";
import { getAuthToken, getCurrentUser } from "../services/authService";

// یک Context ساده برای مدیریت وضعیت کاربر (اختیاری، اما توصیه می‌شود)
// import { AuthContext, AuthProvider } from '../context/AuthContext';

export default function App({ Component, pageProps }) {
  const router = useRouter();
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    // بررسی وجود توکن و اطلاعات کاربر در localStorage
    const token = getAuthToken();
    const user = getCurrentUser();

    if (token && user) {
      setIsAuthenticated(true);
    } else {
      // اگر صفحه فعلی صفحه ورود نبود، به صفحه ورود برو
      if (router.pathname !== "/login") {
        router.push("/login");
      }
    }
    setLoading(false);
  }, [router.pathname]);

  // نمایش یک Loading ساده در صورت نیاز
  if (loading) {
    return <div>Loading...</div>;
  }

  // اگر صفحه ورود باشد، بدون چک کردن وضعیت احراز هویت نمایش بده
  if (router.pathname === "/login") {
    return <Component {...pageProps} />;
  }

  // اگر کاربر وارد نشده بود و صفحه ورود نبود، به ورود هدایت کن (این قبلاً در useEffect چک شد، اما یک چک اضافی)
  if (!isAuthenticated && router.pathname !== "/login") {
    return null; // یا یک کامپوننت Redirect
  }

  // در غیر این صورت، کامپوننت صفحه مورد نظر را نمایش بده
  return (
    // اگر از Context استفاده کردید:
    // <AuthProvider value={{ isAuthenticated, setIsAuthenticated }}>
    <Component {...pageProps} />
    // </AuthProvider>
  );
}
