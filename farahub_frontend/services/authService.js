// services/authService.js
// دیگر نیازی به کار با توکن در کلاینت نیست، فقط کوکی مدیریت می‌شود
// اما می‌توانیم وضعیت ورود و اطلاعات کاربر را مدیریت کنیم

export const setCurrentUser = (user) => {
  if (user) {
    localStorage.setItem("user", JSON.stringify(user)); // یا sessionStorage
  } else {
    localStorage.removeItem("user"); // یا sessionStorage
  }
};

export const getCurrentUser = () => {
  const userStr = localStorage.getItem("user"); // یا sessionStorage
  if (userStr) {
    return JSON.parse(userStr);
  }
  return null;
};

export const logout = async () => {
  try {
    // فراخوانی endpoint خروج برای حذف کوکی در سمت سرور
    await apiService.post("/account/logout");
  } catch (error) {
    console.error("Logout API error:", error);
    // حتی اگر API خطا داد، می‌توانیم سعی کنیم کوکی را از سمت کلاینت پاک کنیم
    // (هرچند این کار کامل نیست، حذف کوکی در سمت سرور مهم‌تر است)
    // document.cookie = "auth_token=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";
  }
  // حذف اطلاعات کاربر از localStorage
  localStorage.removeItem("user");
  // هدایت به صفحه ورود
  window.location.href = "/login"; // یا Router.push('/login')
};

// تابع setAuthToken را حذف کنید (اگر وجود داشت)
// export const setAuthToken = (token) => { ... };
// export const getAuthToken = () => { ... }; // این هم دیگر لازم نیست
