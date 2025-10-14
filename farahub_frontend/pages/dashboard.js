import Sidebar from "../components/Sidebar";
import { useSelector } from "react-redux";
import { selectIsAuthenticated } from "../store/authSlice";
import { useRouter } from "next/router";
import { useEffect } from "react";

const Dashboard = () => {
  const isAuthenticated = useSelector(selectIsAuthenticated);
  const router = useRouter();

  useEffect(() => {
    if (!isAuthenticated) {
      router.push("/login");
    }
  }, [isAuthenticated, router]);

  if (!isAuthenticated) {
    return <div>Loading...</div>;
  }

  return (
    <div className="flex h-screen bg-gray-100">
      {" "}
      {/* یک کانتینر فلکس برای قرار دادن Sidebar و Main Content */}
      <Sidebar />
      <main className="flex-1 p-6 overflow-y-auto">
        {" "}
        {/* Main Content */}
        <h1 className="text-2xl font-bold mb-4">داشبورد</h1>
        <p>خوش آمدید، به داشبورد FaraHub.</p>
        {/* محتوای داشبورد اینجا قرار می‌گیرد */}
        {/* می‌توانید کامپوننت‌های دیگر را اینجا رندر کنید یا صفحات مختلف روت Next.js در این ناحیه نمایش داده شوند */}
      </main>
    </div>
  );
};

export default Dashboard;
