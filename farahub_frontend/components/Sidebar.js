import { useSelector } from "react-redux";
import { selectUser, selectIsAuthenticated } from "../store/authSlice";
import Link from "next/link";
import { useRouter } from "next/router";

const Sidebar = () => {
  const router = useRouter();
  const user = useSelector(selectUser);
  const isAuthenticated = useSelector(selectIsAuthenticated);

  const getMenus = (roles) => {
    const menus = [
      { name: "تیکت‌ها", path: "/tickets" },
      { name: "پروفایل", path: "/profile" },
    ];

    if (
      roles &&
      (roles.includes("Admin") || roles.includes("TechnicalManager"))
    ) {
      menus.push(
        { name: "مدیریت کاربران", path: "/users" },
        { name: "گزارشات", path: "/reports" }
      );
    }

    if (roles && roles.includes("SalesManager")) {
      menus.push({ name: "داشبورد فروش", path: "/sales" });
    }

    menus.push({ name: "خروج", path: "/logout" });

    return menus;
  };

  if (!isAuthenticated || !user) {
    return null;
  }

  const menus = getMenus(user.Roles);

  return (
    <div className="w-64 bg-gray-800 text-white h-screen p-4 flex flex-col">
      <h2 className="text-xl font-bold mb-4">FaraHub</h2>
      {/* نمایش نام کاربر */}
      <div className="mb-4 p-2 bg-gray-700 rounded">
        <p className="text-sm">خوش آمدی، {user.FullName || user.UserName}</p>
        <p className="text-xs text-gray-300">
          نقش: {user.Roles?.join(", ") || "کاربر"}
        </p>
      </div>
      <nav className="flex-1">
        <ul className="space-y-2">
          {menus.map((menu, index) => (
            <li key={index}>
              {/* اگر مسیر خروج بود، یک تگ <a> برای ارسال درخواست POST به /api/account/logout استفاده کنیم */}
              {menu.path === "/logout" ? (
                <a
                  href="#"
                  onClick={(e) => {
                    e.preventDefault();
                    // dispatch یا فراخوانی API خروج را اینجا اضافه کنید
                    // مثلاً: dispatch(logout());
                    // یا: await apiService.post('/account/logout');
                    // سپس هدایت به صفحه ورود
                    window.location.href = "/login"; // یا از Router.push('/login') استفاده کنید
                  }}
                  className="block py-2 px-4 hover:bg-gray-700 rounded cursor-pointer"
                >
                  {menu.name}
                </a>
              ) : (
                <Link
                  href={menu.path}
                  className={`block py-2 px-4 rounded ${
                    router.pathname === menu.path
                      ? "bg-gray-700"
                      : "hover:bg-gray-700"
                  }`}
                >
                  {menu.name}
                </Link>
              )}
            </li>
          ))}
        </ul>
      </nav>
    </div>
  );
};

export default Sidebar;
