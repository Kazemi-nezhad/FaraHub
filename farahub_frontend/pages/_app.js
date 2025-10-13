import "../styles/globals.css";
import { useRouter } from "next/router";
import { useEffect, useState } from "react";
import { Provider, useDispatch, useSelector } from "react-redux";
import { store } from "../store/store";
import {
  loginSuccess,
  logout,
  selectIsAuthenticated,
} from "../store/authSlice";
import apiService from "../services/apiService";

export default function App({ Component, pageProps }) {
  return (
    <Provider store={store}>
      {" "}
      {/* اتصال store به اپلیکیشن */}
      <ConnectedApp Component={Component} pageProps={pageProps} />
    </Provider>
  );
}

function ConnectedApp({ Component, pageProps }) {
  const router = useRouter();
  const dispatch = useDispatch();
  const isAuthenticated = useSelector(selectIsAuthenticated);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const checkAuthStatus = async () => {
      if (router.pathname === "/login") {
        setLoading(false);
        return;
      }

      try {
        const response = await apiService.get("/account/status");
        dispatch(loginSuccess(response.data.user));
      } catch (error) {
        if (error.response?.status === 401) {
          dispatch(logout());
          if (router.pathname !== "/login") {
            router.push("/login");
          }
        } else {
          console.error("Auth check error:", error);
          dispatch(logout());
          if (router.pathname !== "/login") {
            router.push("/login");
          }
        }
      } finally {
        setLoading(false);
      }
    };

    checkAuthStatus();
  }, [router.pathname, dispatch]);

  if (loading) {
    return <div>Loading...</div>;
  }

  if (router.pathname === "/login") {
    return <Component {...pageProps} />;
  }

  if (!isAuthenticated && router.pathname !== "/login") {
    return null;
  }

  return <Component {...pageProps} />;
}
