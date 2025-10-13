import { useSelector } from "react-redux";
import { selectUser } from "../store/authSlice";
import Link from "next/link";

const Sidebar = () => {
  const user = useSelector(selectUser);

  const getMenus = (roles) => {
    const menus = [
      { name: "تیکت‌ها", path: "/tickets" },
      { name: "پروفایل", path: "/profile" },
    ];

    if (roles.includes("Admin") || roles.includes("TechnicalManager")) {
      menus.push(
        { name: "مدیریت کاربران", path: "/users" },
        { name: "گزارشات", path: "/reports" }
      );
    }

    return menus;
  };

  const menus = user ? getMenus(user.roles || []) : [];

  return (
    <div className="w-64 bg-gray-800 text-white h-screen p-4">
      <h2 className="text-xl font-bold mb-4">FaraHub</h2>
      {/* نمایش نام کاربر */}
      {user && (
        <div className="mb-4 p-2 bg-gray-700 rounded">
          <p className="text-sm">خوش آمدی، {user.fullName || user.userName}</p>
        </div>
      )}
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
