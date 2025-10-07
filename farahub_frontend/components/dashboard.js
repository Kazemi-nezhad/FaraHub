// pages/dashboard.js
import Sidebar from "../components/Sidebar";

export default function Dashboard() {
  // const user = getCurrentUser(); // اگر بخواهید اطلاعات کاربر را در داشبورد نیز نمایش دهید

  return (
    <div className="flex h-screen">
      <Sidebar />
      <main className="flex-1 p-6 bg-gray-100">
        <h1 className="text-2xl font-bold mb-4">داشبورد</h1>
        <p>خوش آمدید، به داشبورد FaraHub.</p>
        {/* محتوای داشبورد اینجا قرار می‌گیرد */}
      </main>
    </div>
  );
}
