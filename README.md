# Gaming Center Management System

Hệ thống quản lý phòng máy Cyber Game, cơ chế tính cước đa hình và tổ chức giải đấu LAN Party.

* **Học phần:** Lập trình .NET (C# Windows Forms)
* **Nền tảng:** .NET 10.0 (Windows Desktop), Visual Studio 2026 / 2022
* **Cơ sở dữ liệu:** Microsoft SQL Server (SSMS), kết nối bằng thư viện NuGet `Microsoft.Data.SqlClient`
* **Kiến trúc:** 4 tính chất OOP (Đóng gói, Kế thừa, Đa hình, Trừu tượng), N-Tier Architecture, Unit Testing (xUnit)

---

## 1. Phân công thành viên nhóm

| STT | Thành viên | Nhánh Git | Module phụ trách | Tệp mã nguồn chính |
| :---: | :--- | :--- | :--- | :--- |
| 1 | **Nguyễn Công Bằng** (Leader) | `main` | Kiến trúc hệ thống, CSDL, OOP Đa hình, Dashboard trung tâm | `Form1.cs`, `Database.sql`, `Database.cs`, `OOP/` |
| 2 | **Văn** (`tksforall`) | `Van-Tks` | Quản lý máy trạm, phân khu Zone, sơ đồ trạng thái | `FormStation.cs`, `DBStation.cs` |
| 3 | **Nam** (`LuciferDakin`) | `Nam-Alime` | Quản lý tài khoản hội viên, ví tiền, nạp tiền nhanh | `FormMember.cs`, `DBUser.cs` |
| 4 | **Kiên** (`KDotBlack`) | `Kien-Dot-Black` | Quản lý giải đấu LAN, đăng ký đội, khóa máy thi đấu | `FormTournament.cs`, `DBTournament.cs` |
| 5 | **Khôi** (`koll06`) | `Khoi-Beo` | Quản lý gói cước, dịch vụ và lọc báo cáo doanh thu | `FormBilling.cs`, `DBBilling.cs` |

---

## 2. Thiết kế Hướng đối tượng (OOP)

* **Đóng gói (Encapsulation):** Ẩn giấu dữ liệu qua các private field và thuộc tính kiểm tra logic trong `AppUser`, `ComputerStation`, `GamingSession` (không cho phép số dư ví âm, kiểm tra trạng thái máy trước khi mở phiên).
* **Trừu tượng (Abstraction):** Lớp trừu tượng `TariffCalculatorBase` định nghĩa khuôn mẫu hàm `CalculateCost()` và generic interface `IRepository<T>`.
* **Kế thừa (Inheritance):** Các lớp cước cụ thể `HourlyTariff`, `NightComboTariff`, `TournamentTariff` kế thừa từ `TariffCalculatorBase`.
* **Đa hình (Polymorphism):** Cùng gọi hàm `CalculateCost()`, hệ thống sẽ tự động gọi đúng công thức theo gói cước (tính phút theo giá zone, trọn gói đêm 45k, hoặc giảm giá 50% cho tuyển thủ giải đấu) mà không cần dùng chuỗi if-else.

---

## 3. Danh mục 20 Test Cases kiểm thử tự động (Unit Tests)

Dự án tích hợp project kiểm thử tự động `GamingCenterManagementSystem.Tests` sử dụng framework xUnit, bao phủ toàn bộ các nghiệp vụ tính toán và dữ liệu biên.

| Mã TC | Tên bài test (Test Case) | Dữ liệu đầu vào (Input) | Kết quả kỳ vọng (Expected Output) | Trạng thái |
| :---: | :--- | :--- | :--- | :---: |
| **TC01** | Tính cước theo giờ thường chuẩn xác | Chơi từ 08:00 đến 10:00 (2.0 giờ), giá 10,000đ/h | Tổng tiền = 20,000đ | **PASS** |
| **TC02** | Tính cước giờ có thời lượng tối thiểu | Chơi dưới 6 phút (0.05 giờ), giá 10,000đ/h | Tính mức tối thiểu 0.1 giờ = 1,000đ | **PASS** |
| **TC03** | Tính cước Combo đêm trong khung giờ | Bắt đầu 22:00, kết thúc 05:30 sáng hôm sau | Tính đúng giá cố định combo đêm = 45,000đ | **PASS** |
| **TC04** | Tính cước Combo đêm khi chơi quá 6h sáng | Kết thúc lúc 06:30 sáng (quá 30 phút), giá 10,000đ/h | 45,000đ + (0.5h x 10,000đ) = 50,000đ | **PASS** |
| **TC05** | Tính cước ưu đãi giải đấu LAN | Chơi 2.0 giờ, giá gốc 18,000đ/h | Giảm 50% cước: 36,000đ x 0.5 = 18,000đ | **PASS** |
| **TC06** | Khởi tạo tài khoản hội viên hợp lệ | Tạo user `gamer01`, nạp 50,000đ ban đầu | Tạo thành công, số dư đúng 50,000đ | **PASS** |
| **TC07** | Chặn số dư ví âm của hội viên | Cố tình gán số dư `Balance = -10,000đ` | Ném ngoại lệ `ArgumentException` an toàn | **PASS** |
| **TC08** | Nạp tiền tự động thăng hạng Silver | Nạp cộng dồn số dư ví đạt 250,000đ (>= 200,000đ) | Hạng thẻ tự động đổi thành `Silver` | **PASS** |
| **TC09** | Nạp tiền tự động thăng hạng Gold | Nạp cộng dồn số dư ví đạt 600,000đ (>= 500,000đ) | Hạng thẻ tự động đổi thành `Gold` | **PASS** |
| **TC10** | Trừ tiền ví khi thanh toán phiên chơi | Ví có 100,000đ, thanh toán tiền giờ 30,000đ | Trừ thành công, số dư mới = 70,000đ | **PASS** |
| **TC11** | Chặn trừ tiền khi ví không đủ số dư | Ví có 20,000đ, thanh toán tiền cước 50,000đ | Trả về thất bại, bảo toàn số dư ví | **PASS** |
| **TC12** | Kiểm tra trạng thái máy trạm sẵn sàng | Máy có `Status = "Available"` | Hàm `IsAvailable()` trả về `true` | **PASS** |
| **TC13** | Chặn mở phiên khi máy đang có khách | Máy có `Status = "InUse"` | Hàm `IsAvailable()` trả về `false` | **PASS** |
| **TC14** | Chặn mở phiên khi máy đang bảo trì | Máy có `Status = "Maintenance"` | Hàm `IsAvailable()` trả về `false` | **PASS** |
| **TC15** | Khóa dàn máy thi đấu giải LAN | Gọi khóa 5 máy thi đấu (`PC-06` đến `PC-10`) | Toàn bộ 5 máy chuyển sang `TournamentLocked` | **PASS** |
| **TC16** | Mở khóa dàn máy sau giải đấu | Gọi mở khóa 5 máy thi đấu sau khi trao giải | Toàn bộ 5 máy trở về `Available` | **PASS** |
| **TC17** | Xử lý an toàn khi truyền mảng máy rỗng | Gọi hàm khóa máy với mảng rỗng `int[0]` | Trả về `false` an toàn, không lỗi SQL | **PASS** |
| **TC18** | Đọc dữ liệu SQL an toàn khi có giá trị NULL | Đọc phiên chơi đang chạy (`EndTime = NULL`) | Đọc an toàn, không nổ `InvalidCastException` | **PASS** |
| **TC19** | Lọc danh sách máy theo Zone bằng LINQ | Lọc danh sách theo `ZoneId == 1` trong bộ nhớ | Trả về chính xác các máy thuộc Zone 1 | **PASS** |
| **TC20** | Tính tổng doanh thu ca trực bằng LINQ | Gom danh sách các phiên hoàn thành và dùng `Sum()` | Tính chính xác tổng tiền cước của tất cả phiên | **PASS** |

---

## 4. Hướng dẫn cài đặt và Chạy thử nghiệm

### Bước 1: Khởi tạo Cơ sở Dữ liệu
1. Mở **SQL Server Management Studio (SSMS)** và kết nối tới máy chủ `.\SQLEXPRESS`.
2. Mở tệp `Database.sql` có sẵn trong thư mục gốc của dự án.
3. Bấm **Execute** (`F5`) để tạo cơ sở dữ liệu `GamingCenterDB`, 7 bảng quan hệ và nạp dữ liệu mẫu ban đầu.

### Bước 2: Chạy Kiểm thử Tự động (Run Unit Tests)
1. Mở giải pháp `GamingCenterManagementSystem.sln` bằng **Visual Studio**.
2. Trên thanh menu, chọn **Test** &rarr; **Run All Tests** (hoặc tổ hợp phím `Ctrl + R, A`).
3. Cửa sổ **Test Explorer** sẽ kiểm tra và hiển thị toàn bộ 20 bài test vượt qua (Passed 20/20).

### Bước 3: Khởi chạy Ứng dụng
1. Nhấn **F5** (hoặc nút **Start**) trong Visual Studio để chạy chương trình.
2. Màn hình chính Dashboard hiển thị sơ đồ 10 máy trạm trực quan đổi màu theo thời gian thực:
   * ⚪ **Xám (Available):** Máy trống, sẵn sàng mở cho khách.
   * 🟢 **Xanh lá (InUse):** Máy đang có khách chơi, đếm giờ và tính tiền trực tiếp.
   * 🔴 **Đỏ (Maintenance):** Máy đang bảo trì kỹ thuật.
   * 🟡 **Vàng (TournamentLocked):** Máy được giữ chỗ cho giải đấu LAN Party.
3. Sử dụng các nút điều hướng phía trên để mở 4 màn hình chức năng:
   * **Quản lý máy trạm:** Thêm, sửa, đổi trạng thái khu vực máy.
   * **Quản lý hội viên:** Tìm kiếm thông tin, nạp tiền vào ví điện tử.
   * **Quản lý giải đấu:** Lập giải, đăng ký đội tuyển, khóa và giải phóng dàn máy thi đấu.
   * **Dịch vụ & Doanh thu:** Quản lý bảng giá cước, tính cước thử nghiệm và lọc doanh thu theo ngày.
