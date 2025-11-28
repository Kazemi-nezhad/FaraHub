import { useEffect } from "react";
import { useDispatch, useSelector } from "react-redux";
import { useRouter } from "next/router";
import {
  fetchTicketsStart,
  fetchTicketsSuccess,
  fetchTicketsFailure,
  selectTickets,
  selectTicketsLoading,
  selectTicketsError,
} from "../store/ticketSlice";
import apiService from "../services/apiService";
import Sidebar from "../components/Sidebar";

const TicketsPage = () => {
  const dispatch = useDispatch();
  const router = useRouter();
  const tickets = useSelector(selectTickets);
  const loading = useSelector(selectTicketsLoading);
  const error = useSelector(selectTicketsError);

  useEffect(() => {
    const fetchTickets = async () => {
      dispatch(fetchTicketsStart());
      try {
        const response = await apiService.get("/ticket/my-tickets");
        dispatch(fetchTicketsSuccess(response.data.tickets || response.data));
      } catch (err) {
        console.error("Error fetching tickets:", err);
        dispatch(
          fetchTicketsFailure(
            err.response?.data?.message || "خطا در بارگذاری تیکت‌ها"
          )
        );
      }
    };

    fetchTickets();
  }, [dispatch]);

  const handleTicketClick = (ticketId) => {
    router.push(`/ticket/${ticketId}`);
  };

  return (
    <div className="flex h-screen bg-gray-100">
      <Sidebar />
      <main className="flex-1 p-6 overflow-y-auto">
        <h1 className="text-2xl font-bold mb-4 text-neutral-700">تیکت‌ها</h1>

        {loading && <p>در حال بارگذاری...</p>}
        {error && <p className="text-red-500">خطا: {error}</p>}

        {!loading && !error && (
          <div className="bg-white p-4 rounded-lg shadow-md">
            {tickets.length === 0 ? (
              <p className="text-neutral-500">هیچ تیکتی یافت نشد.</p>
            ) : (
              <table className="min-w-full divide-y divide-gray-200">
                <thead className="bg-gray-50">
                  <tr>
                    <th
                      scope="col"
                      className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider"
                    >
                      شماره
                    </th>
                    <th
                      scope="col"
                      className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider"
                    >
                      عنوان
                    </th>
                    <th
                      scope="col"
                      className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider"
                    >
                      وضعیت
                    </th>
                    <th
                      scope="col"
                      className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider"
                    >
                      ایجاد کننده
                    </th>
                    <th
                      scope="col"
                      className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider"
                    >
                      تاریخ ایجاد
                    </th>
                    <th
                      scope="col"
                      className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider"
                    >
                      اولویت
                    </th>
                  </tr>
                </thead>
                <tbody className="bg-white divide-y divide-gray-200">
                  {tickets.map((ticket) => (
                    <tr
                      key={ticket.id}
                      className="hover:bg-gray-50 cursor-pointer"
                      onClick={() => handleTicketClick(ticket.id)}
                    >
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                        {ticket.id}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                        {ticket.title}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                        {ticket.status}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                        {ticket.createdBy?.fullName ||
                          ticket.createdBy?.userName ||
                          "ناشناس"}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                        {/* تبدیل تاریخ به شمسی اینجا لازم است */}
                        {new Date(ticket.createdAt).toLocaleDateString("fa-IR")}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                        {ticket.priority}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>
        )}
      </main>
    </div>
  );
};

export default TicketsPage;
