// pages/_app.js
import "../styles/globals.css";
import { useRouter } from "next/router";
import { useEffect, useState } from "react";
// import { getAuthToken, getCurrentUser } from '../services/authService'; // دیگر نیازی نیست

// یک API ساده برای چک کردن وضعیت احراز هویت (اختیاری)
import apiService from "../services/apiService";

export default function App({ Component, pageProps }) {
  const router = useRouter();
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const checkAuthStatus = async () => {
      if (router.pathname === "/login") {
        setIsAuthenticated(false);
        setLoading(false);
        return;
      }

      try {
        // فراخوانی یک endpoint سبک برای چک کردن احراز هویت
        // این endpoint باید در بک‌اند ایجاد شود (مثلاً GET /api/account/status)
        // که فقط در صورت وجود توکن معتبر (خوانده شده از کوکی) پاسخ موفقیت‌آمیز بدهد
        const response = await apiService.get("/account/status"); // این endpoint رو باید در بک‌اند بسازید
        setIsAuthenticated(true);
        // اگر نیاز به ذخیره اطلاعات کاربر دارید، می‌توانید از اینجا بگیرید
        // setCurrentUser(response.data.user);
      } catch (error) {
        // اگر خطا 401 باشد، یعنی کاربر وارد نیست
        if (error.response?.status === 401) {
          setIsAuthenticated(false);
          if (router.pathname !== "/login") {
            router.push("/login");
          }
        } else {
          // سایر خطاها
          console.error("Auth check error:", error);
          // ممکن است بخواهید کاربر را به صفحه ورود هدایت کنید یا وضعیت خطا نمایش دهید
          setIsAuthenticated(false);
          if (router.pathname !== "/login") {
            router.push("/login");
          }
        }
      } finally {
        setLoading(false);
      }
    };

    checkAuthStatus();
  }, [router.pathname]);

  if (loading) {
    return <div>Loading...</div>;
  }

  if (router.pathname === "/login") {
    return <Component {...pageProps} />;
  }

  if (!isAuthenticated && router.pathname !== "/login") {
    return null; // یا یک کامپوننت Redirect
  }

  return <Component {...pageProps} />;
}
