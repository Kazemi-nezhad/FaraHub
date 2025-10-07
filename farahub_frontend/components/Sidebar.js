// components/Sidebar.js
import { getCurrentUser } from "../services/authService";
import Link from "next/link";

const Sidebar = () => {
  const user = getCurrentUser(); // اطلاعات کاربر از localStorage

  // تابعی برای تعیین منوها بسته به نقش
  const getMenus = (roles) => {
    const menus = [
      { name: "تیکت‌ها", path: "/tickets" },
      { name: "پروفایل", path: "/profile" },
    ];

    if (roles.includes("Admin") || roles.includes("TechnicalManager")) {
      menus.push(
        { name: "مدیریت کاربران", path: "/users" },
        { name: "گزارشات", path: "/reports" } // یا هر صفحه دیگری
      );
    }

    // نقش‌های تخصصی مانند TechnicalManager, SalesManager می‌توانند منوی خاصی داشته باشند
    // if (roles.includes('TechnicalManager')) {
    //   menus.push({ name: 'مدیریت فنی', path: '/technical' });
    // }

    return menus;
  };

  const menus = user ? getMenus(user.roles || []) : [];

  return (
    <div className="w-64 bg-gray-800 text-white h-screen p-4">
      <h2 className="text-xl font-bold mb-4">FaraHub</h2>
      <nav>
        <ul className="space-y-2">
          {menus.map((menu, index) => (
            <li key={index}>
              <Link
                href={menu.path}
                className="block py-2 px-4 hover:bg-gray-700 rounded"
              >
                {menu.name}
              </Link>
            </li>
          ))}
        </ul>
      </nav>
    </div>
  );
};

export default Sidebar;
