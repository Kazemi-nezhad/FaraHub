// services/apiService.js
import axios from "axios";

// آدرس سرور بک‌اند شما (مثلاً اگر بک‌اند روی localhost:5000 اجرا می‌شود)
const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_BASE_URL || "http://localhost:5000/api"; // یا هر آدرس دیگری

const apiService = axios.create({
  baseURL: API_BASE_URL,
});

// افزودن interceptor قبل از هر درخواست
apiService.interceptors.request.use(
  (config) => {
    // گرفتن توکن از localStorage
    const token = localStorage.getItem("token");
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// افزودن interceptor بعد از هر پاسخ (برای مدیریت خطاها، مثلاً 401 Unauthorized)
apiService.interceptors.response.use(
  (response) => {
    return response;
  },
  (error) => {
    if (error.response?.status === 401) {
      // اگر توکن نامعتبر یا منقضی بود، کاربر را به صفحه ورود هدایت کن
      localStorage.removeItem("token");
      localStorage.removeItem("user");
      window.location.href = "/login"; // یا از Router.push('/login') در Next.js استفاده کنید
    }
    return Promise.reject(error);
  }
);

export default apiService;
