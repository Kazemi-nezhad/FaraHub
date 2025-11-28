import { useEffect, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { useRouter } from "next/router";
import Sidebar from "../../components/Sidebar"; // توجه به مسیر
import {
  fetchTicketDetailStart,
  fetchTicketDetailSuccess,
  fetchTicketDetailFailure,
  selectSelectedTicket,
  selectTicketDetailLoading,
  selectTicketDetailError,
  addMessageToSelectedTicket,
} from "../../store/ticketSlice"; // توجه به مسیر
import apiService from "../../services/apiService"; // توجه به مسیر
import { setCurrentUser } from "../../services/authService"; // برای بروزرسانی اطلاعات کاربر اگر لازم بود

const TicketDetailPage = () => {
  const router = useRouter();
  const { id: ticketId } = router.query; // گرفتن id از URL
  const dispatch = useDispatch();

  const ticket = useSelector(selectSelectedTicket);
  const loading = useSelector(selectTicketDetailLoading);
  const error = useSelector(selectTicketDetailError);

  // فرم ارسال پیام
  const [messageContent, setMessageContent] = useState("");
  const [attachedFiles, setAttachedFiles] = useState([]); // یه آرایه از فایل‌ها

  useEffect(() => {
    if (ticketId) {
      // فقط وقتی id وجود داشت، درخواست بزن
      const fetchTicketDetail = async () => {
        dispatch(fetchTicketDetailStart());
        try {
          // فرض بر این است که این endpoint جزئیات تیکت و پیام‌های آن را برمی‌گرداند
          // endpoint GET /api/ticket/{id}
          const response = await apiService.get(`/ticket/${ticketId}`);
          dispatch(fetchTicketDetailSuccess(response.data)); // فرض بر این است که ساختار پاسخ درست است
        } catch (err) {
          console.error("Error fetching ticket detail:", err);
          dispatch(
            fetchTicketDetailFailure(
              err.response?.data?.message || "خطا در بارگذاری جزئیات تیکت"
            )
          );
        }
      };

      fetchTicketDetail();
    }
  }, [ticketId, dispatch]); // وابستگی به ticketId و dispatch

  // تابع برای ارسال پیام جدید
  const handleSendMessage = async (e) => {
    e.preventDefault();
    if (!messageContent.trim() && attachedFiles.length === 0) {
      alert("لطفاً متن پیام یا فایل ضمیمه کنید.");
      return;
    }

    const formData = new FormData();
    formData.append("content", messageContent);

    // اضافه کردن فایل‌ها به FormData
    attachedFiles.forEach((file, index) => {
      formData.append("files", file); // 'files' نام فیلدی که در بک‌اند چک می‌شود
    });

    try {
      // فرض بر این است که این endpoint پیام جدید را می‌پذیرد
      // endpoint POST /api/message/send/{ticketId}
      const response = await apiService.post(
        `/message/send/${ticketId}`,
        formData,
        {
          headers: {
            "Content-Type": "multipart/form-data", // خیلی مهم برای FormData
          },
        }
      );

      // پیام جدید را به لیست پیام‌های نمایش داده شده اضافه کن (بهینه‌سازی فوری)
      dispatch(addMessageToSelectedTicket(response.data)); // فرض بر این است که پاسخ شامل پیام ایجاد شده است

      // ریست فرم
      setMessageContent("");
      setAttachedFiles([]);
    } catch (err) {
      console.error("Error sending message:", err);
      alert(
        "خطا در ارسال پیام: " + (err.response?.data?.message || "خطای ناشناخته")
      );
    }
  };

  // تابع برای مدیریت تغییر فایل‌های ضمیمه
  const handleFileChange = (e) => {
    const files = Array.from(e.target.files);
    // می‌توانید اعتبارسنجی حداکثر تعداد/حجم/فرمت فایل اینجا اضافه کنید
    setAttachedFiles(files);
  };

  if (loading) return <div>در حال بارگذاری جزئیات تیکت...</div>;
  if (error) return <div>خطا: {error}</div>;
  if (!ticket) return <div>تیکت یافت نشد.</div>; // اگر بعد از بارگذاری، ticket هنوز null بود

  return (
    <div className="flex h-screen bg-gray-100">
      <Sidebar />
      <main className="flex-1 p-6 overflow-y-auto">
        <div className="bg-white p-4 rounded-lg shadow-md mb-4">
          <h1 className="text-xl font-bold text-neutral-600">{ticket.title}</h1>
          <div className="mt-2 text-sm text-gray-600">
            <p>
              <strong>وضعیت:</strong> {ticket.status}
            </p>
            <p>
              <strong>اولویت:</strong> {ticket.priority}
            </p>
            <p>
              <strong>ایجاد شده توسط:</strong>{" "}
              {ticket.createdBy?.fullName || ticket.createdBy?.userName}
            </p>
            <p>
              <strong>تاریخ ایجاد:</strong>{" "}
              {new Date(ticket.createdAt).toLocaleDateString("fa-IR")}
            </p>
            {ticket.assignedTo && (
              <p>
                <strong>ارجاع شده به:</strong>{" "}
                {ticket.assignedTo?.fullName || ticket.assignedTo?.userName}
              </p>
            )}
            {ticket.customer && (
              <p>
                <strong>مشتری:</strong>{" "}
                {ticket.customer?.fullName || ticket.customer?.userName}
              </p>
            )}
            <p>
              <strong>زمان کل صرف شده:</strong>{" "}
              {ticket.totalTimeSpent || "00:00:00"} (HH:MM:SS)
            </p>{" "}
            {/* فرض بر این است که totalTimeSpent در پاسخ وجود دارد */}
          </div>
        </div>

        {/* چت‌روم */}
        <div className="bg-white p-4 rounded-lg shadow-md mb-4">
          <h2 className="text-lg font-semibold mb-2 text-neutral-600">
            چت‌روم
          </h2>
          <div className="border rounded p-2 max-h-96 overflow-y-auto">
            {ticket.messages && ticket.messages.length > 0 ? (
              ticket.messages.map((msg) => (
                <div key={msg.id} className="mb-2 p-2 border-b">
                  <div className="flex items-start">
                    {/* آواتار (ساده‌شده) */}
                    <div className="bg-gray-200 rounded-full w-8 h-8 flex items-center justify-center mr-2">
                      <span className="text-xs text-neutral-600">
                        {(msg.sentBy?.fullName || msg.sentBy?.userName)?.charAt(
                          0
                        )}
                      </span>
                    </div>
                    <div className="flex-1 text-neutral-600">
                      <p className="text-sm font-medium">
                        {msg.sentBy?.fullName || msg.sentBy?.userName}
                      </p>
                      <p className="text-xs text-gray-500">
                        {new Date(msg.sentAt).toLocaleString("fa-IR")}
                      </p>
                      <p className="mt-1">{msg.content}</p>
                      {msg.attachments && msg.attachments.length > 0 && (
                        <div className="mt-2">
                          <p className="text-xs text-gray-500">
                            فایل‌های پیوست:
                          </p>
                          <ul className="text-xs">
                            {msg.attachments.map((att) => (
                              <li key={att.id} className="flex items-center">
                                <a
                                  href={att.downloadUrl}
                                  target="_blank"
                                  rel="noopener noreferrer"
                                  className="text-blue-500 hover:underline"
                                >
                                  {att.fileName}
                                </a>
                                <span className="mx-1">-</span>
                                <span>
                                  {(att.size / 1024 / 1024).toFixed(2)} MB
                                </span>
                              </li>
                            ))}
                          </ul>
                        </div>
                      )}
                    </div>
                  </div>
                </div>
              ))
            ) : (
              <p className="text-gray-500 text-sm">
                هنوز پیامی ارسال نشده است.
              </p>
            )}
          </div>
        </div>

        {/* فرم ارسال پیام */}
        <div className="bg-white p-4 rounded-lg shadow-md">
          <h2 className="text-lg font-semibold mb-2 text-neutral-600">
            ارسال پیام جدید
          </h2>
          <form onSubmit={handleSendMessage}>
            <div className="mb-2">
              <textarea
                value={messageContent}
                onChange={(e) => setMessageContent(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 text-neutral-600 rounded-md focus:outline-none focus:ring-2 focus:ring-indigo-500"
                rows="3"
                placeholder="متن پیام..."
              />
            </div>
            <div className="mb-2">
              <label className="block text-sm font-medium text-gray-700 mb-1">
                ضمیمه فایل (حداکثر 5 فایل، 50 مگابایت کل)
              </label>
              <input
                type="file"
                multiple // اجازه انتخاب چند فایل
                onChange={handleFileChange}
                className="block w-full text-sm text-gray-500
                  file:mr-4 file:py-2 file:px-4
                  file:rounded-md file:border-0
                  file:text-sm file:font-semibold
                  file:bg-indigo-50 file:text-indigo-700
                  hover:file:bg-indigo-100"
              />
              {attachedFiles.length > 0 && (
                <div className="mt-1 text-xs text-gray-500">
                  فایل‌های انتخاب شده:{" "}
                  {attachedFiles.map((f) => f.name).join(", ")}
                </div>
              )}
            </div>
            <button
              type="submit"
              className="w-full py-2 px-4 border border-transparent text-sm font-medium rounded-md text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
            >
              ارسال
            </button>
          </form>
        </div>
      </main>
    </div>
  );
};

export default TicketDetailPage;
